using Microsoft.EntityFrameworkCore;
using DeviceManagement.API.Models;
var builder = WebApplication.CreateBuilder(args);

// Khởi tạo Entity Framework Core kết nối với PostgreSQL thông qua kiến trúc Aspire.
// "postgres-db" là tên bí danh phải khớp 100% với tên đã đăng ký trong AppHost.
builder.AddNpgsqlDbContext<DeviceManagement.API.Infrastructure.DeviceDbContext>("postgres-db");
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// === KHỐI CODE MỚI THÊM: Tự động chạy Migration mỗi khi bật App ===
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DeviceManagement.API.Infrastructure.DeviceDbContext>();
    // Dùng chiến lược thử lại (Retry) của EF Core: Nếu Database chưa kịp bật lên, nó sẽ chờ và thử lại thay vì sập luôn.
    var strategy = dbContext.Database.CreateExecutionStrategy();
    strategy.Execute(() => dbContext.Database.Migrate());
}
// ================================================================

// Configure the HTTP request pipeline.
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
    // Dùng EF Core lấy toàn bộ dữ liệu từ bảng Chargers trong PostgreSQL
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
    // C# tạo ra một trạm sạc mới
    var newCharger = new Charger(name);
    
    // Đưa vào vùng nhớ của EF Core
    db.Chargers.Add(newCharger);
    
    // Lưu thẳng xuống Database PostgreSQL
    await db.SaveChangesAsync();

    // Trả về mã 201 (Created) kèm data vừa tạo (Đúng chuẩn RESTful)
    return Results.Created($"/api/chargers/{newCharger.Id}", newCharger);
})
.WithName("CreateCharger")
.WithOpenApi();

app.Run();
