// Import các thư viện cần thiết
using Microsoft.EntityFrameworkCore;
using Identity.API.Models;

// Khởi tạo bộ máy để lắp ráp các linh kiện của ứng dụng
var builder = WebApplication.CreateBuilder(args);

// BƯỚC 4.1: CẤU HÌNH DATABASE
// Dùng kiến trúc Aspire để kết nối thẳng vào ống nước mang tên "identity-db" đã được định nghĩa bên AppHost.
builder.AddNpgsqlDbContext<Identity.API.Infrastructure.IdentityDbContext>("identity-db");

// BƯỚC 4.2: CẤU HÌNH SWAGGER (GIAO DIỆN TEST API)
// Thu thập danh sách các API và sinh ra tài liệu chuẩn OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Chốt sổ, khóa bộ máy lắp ráp lại và tạo ra ứng dụng thực tế
var app = builder.Build();

// BƯỚC 4.3: TỰ ĐỘNG CẬP NHẬT DATABASE
// Mở một vùng bộ nhớ tạm (Scope) để lấy IdentityDbContext ra làm việc
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<Identity.API.Infrastructure.IdentityDbContext>();
    // EF Core tạo Kế hoạch tác chiến: Nếu DB rớt mạng, tự động chờ và thử lại
    var strategy = dbContext.Database.CreateExecutionStrategy();
    // Chạy lệnh so sánh Code và DB, tự động tạo Bảng Users nếu chưa có
    strategy.Execute(() => dbContext.Database.Migrate());
}

// BƯỚC 4.4: BẬT GIAO DIỆN WEB SWAGGER (Chỉ bật khi đang viết code)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==========================================
// BƯỚC 4.5: KHAI BÁO CÁC MINIMAL API
// ==========================================

// 1. API LẤY DANH SÁCH (GET)
// Endpoint: /api/users - Hệ thống tự bơm IdentityDbContext vào tham số 'db'
app.MapGet("/api/users", async (Identity.API.Infrastructure.IdentityDbContext db) =>
{
    // Bắn lệnh "SELECT * FROM Users" bất đồng bộ để chống đứng máy chủ
    var users = await db.Users.ToListAsync();
    // Trả về Mã 200 (Thành công) kèm theo dữ liệu json
    return Results.Ok(users);
})
.WithName("GetUsers") // Đặt định danh cho API
.WithOpenApi();

// 2. API TẠO TÀI KHOẢN (POST)
// Nhận vào 2 tham số username và email từ đường dẫn (URL)
app.MapPost("/api/users", async (string username, string email, Identity.API.Infrastructure.IdentityDbContext db) =>
{
    // Tạo 1 đối tượng User mới trong RAM C#
    var newUser = new User(username, email);
    // Lưu đối tượng này vào hàng đợi của EF Core
    db.Users.Add(newUser);
    // Bắn lệnh "INSERT INTO Users..." xuống DB để lưu vĩnh viễn
    await db.SaveChangesAsync();
    
    // Tuân thủ triết lý RESTful: Trả về Mã 201 (Created) kèm link truy cập và dữ liệu vừa tạo
    return Results.Created($"/api/users/{newUser.Id}", newUser);
})
.WithName("CreateUser")
.WithOpenApi();

// Bật động cơ Kestrel, bắt đầu lắng nghe các luồng mạng đi vào
app.Run();
