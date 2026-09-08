using Microsoft.EntityFrameworkCore;
using DeviceManagement.API.Models;
using MassTransit; // Khai báo thư viện MassTransit
using DeviceManagement.API.Consumers; // Khai báo thư mục chứa đài Radio

var builder = WebApplication.CreateBuilder(args);

// Khởi tạo Entity Framework Core kết nối với PostgreSQL thông qua kiến trúc Aspire.
builder.AddNpgsqlDbContext<DeviceManagement.API.Infrastructure.DeviceDbContext>("device-db");

// BƯỚC MỚI: CẤU HÌNH ĐÀI RADIO MASSTRANSIT LẮNG NGHE RABBITMQ
builder.Services.AddMassTransit(x =>
{
    // Đưa đài Radio của chúng ta vào danh sách quản lý
    x.AddConsumer<ChargerConnectedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Kéo cáp từ trạm phát sóng RabbitMQ của Aspire về
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));
        // Tự động thiết lập đường truyền (Endpoints) cho các đài Radio
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// === KHỐI CODE Tự động chạy Migration mỗi khi bật App ===
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DeviceManagement.API.Infrastructure.DeviceDbContext>();
    var strategy = dbContext.Database.CreateExecutionStrategy();
    strategy.Execute(() => dbContext.Database.Migrate());
}
// ================================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==========================================
// 1. API Lấy danh sách toàn bộ Trạm sạc (GET)
// ==========================================
app.MapGet("/api/chargers", async (DeviceManagement.API.Infrastructure.DeviceDbContext db) =>
{
    var chargers = await db.Chargers.ToListAsync();
    return Results.Ok(chargers);
})
.WithName("GetChargers")
.WithOpenApi();

// ==========================================
// 2. API Thêm mới một Trạm sạc (POST)
// ==========================================
app.MapPost("/api/chargers", async (string name, DeviceManagement.API.Infrastructure.DeviceDbContext db) =>
{
    var newCharger = new Charger(name);
    db.Chargers.Add(newCharger);
    await db.SaveChangesAsync();
    return Results.Created($"/api/chargers/{newCharger.Id}", newCharger);
})
.WithName("CreateCharger")
.WithOpenApi();

app.Run();
