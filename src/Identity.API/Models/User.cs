// Import lõi của DDD
using SharedKernel;

namespace Identity.API.Models;

/// <summary>
/// Thực thể đại diện cho Tài khoản người dùng trong hệ thống
/// </summary>
public class User : Entity<Guid>, IAggregateRoot
{
    // private set: Chỉ cho phép gán giá trị ở bên trong class này
    public string Username { get; private set; }
    public string Email { get; private set; }
    
    // Constructor dùng khi người dùng đăng ký tài khoản mới
    public User(string username, string email)
    {
        Id = Guid.NewGuid(); // Tự động sinh ID ngẫu nhiên không đụng hàng
        Username = username;
        Email = email;
    }

    // Constructor rỗng bắt buộc phải có để EF Core tự động nạp dữ liệu từ Database lên
    protected User() { } 
}
