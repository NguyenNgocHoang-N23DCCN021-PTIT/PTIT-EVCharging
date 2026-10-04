using Billing.API.Consumers;
using Billing.API.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Gắn chip theo dõi OpenTelemetry (Aspire Dashboard)
builder.AddServiceDefaults();

// 2. KẾT NỐI DATABASE (POSTGRESQL)
// Aspire sẽ tự động tìm chuỗi kết nối "billing-db" do AppHost cấp phát
builder.AddNpgsqlDbContext<BillingDbContext>("billing-db");

// 3. KẾT NỐI RABBITMQ & ĐĂNG KÝ NGƯỜI LẮNG NGHE (CONSUMER)
builder.Services.AddMassTransit(x =>
{
    // Báo cho MassTransit biết sự tồn tại của Sát thủ thầm lặng
    x.AddConsumer<SessionEndedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));
        
        // Lệnh này cực kỳ quan trọng: Nó bảo RabbitMQ tự động tạo các Hộp thư (Queue)
        // và gắn Consumer của chúng ta vào đó để chờ chực sự kiện bay tới.
        cfg.ConfigureEndpoints(context); 
    });
});

// Thêm Swagger để dễ Test
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Tự động Apply Migration khi khởi động App (Tuyệt chiêu của hệ thống Microservices)
// Tự động Apply Migration khi khởi động App với cơ chế Retry an toàn (chờ PostgreSQL sẵn sàng)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    // Thử kết nối và áp dụng Migration, lặp lại tối đa 10 lần nếu database đang khởi tạo
    for (int retry = 1; retry <= 10; retry++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("✅ Kết nối và khởi tạo Billing Database thành công!");
            break; // Thành công thì thoát vòng lặp để app chạy tiếp
        }
        catch (Exception ex)
        {
            if (retry == 10)
            {
                logger.LogError(ex, "❌ Không thể kết nối tới Database sau 10 lần thử.");
                throw;
            }
            logger.LogWarning("⏳ PostgreSQL đang khởi tạo cơ sở dữ liệu... Đợi 3 giây và thử lại lần {Retry}/10", retry);
            Thread.Sleep(3000);
        }
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. API KIỂM TRA SỐ DƯ VÍ
app.MapGet("/api/wallets/{chargerId}", async (string chargerId, BillingDbContext db) =>
{
    var wallet = await db.Wallets.FirstOrDefaultAsync(w => w.UserId == chargerId);
    if (wallet == null) return Results.NotFound("Khách hàng này chưa có ví điện tử.");
    return Results.Ok(new { KhachHang = chargerId, SoDu = wallet.Balance });
})
.WithName("GetWalletBalance")
.WithOpenApi();

app.Run();
