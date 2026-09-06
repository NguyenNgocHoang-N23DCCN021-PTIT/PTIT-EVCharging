// Import thư viện gốc của Microsoft để xài các tính năng của Entity Framework Core
using Microsoft.EntityFrameworkCore;
// Import thư mục Models để class này nhận diện được class User
using Identity.API.Models;

// Đặt class này vào không gian tên Infrastructure (Tầng Hạ tầng giao tiếp với DB)
namespace Identity.API.Infrastructure;

// Bắt buộc kế thừa từ DbContext để có sức mạnh thao tác với Database
public class IdentityDbContext : DbContext
{
    // Constructor nhận vào các cấu hình (chuỗi kết nối, mật khẩu...) từ file Program.cs truyền sang
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    // DbSet là đại diện cho 1 Bảng (Table) dưới Database. 
    // Dòng này sẽ yêu cầu EF Core tạo ra một bảng tên là "Users", với các cột tương ứng với các Property trong class User.
    public DbSet<User> Users { get; set; }
}
