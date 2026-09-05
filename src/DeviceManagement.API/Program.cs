using Microsoft.EntityFrameworkCore;
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
    // Lệnh này bắt buộc EF Core phải tạo Database và các Bảng nếu nó chưa tồn tại
    dbContext.Database.Migrate();
}
// ================================================================

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
