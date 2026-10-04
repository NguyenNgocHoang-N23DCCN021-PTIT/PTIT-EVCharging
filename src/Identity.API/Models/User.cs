// Import lõi của DDD
using SharedKernel;

namespace Identity.API.Models;

/// <summary>
/// Thực thể Domain đại diện cho Tài khoản người dùng trong hệ sinh thái EV Charging.
/// Áp dụng nguyên lý DDD: Đóng gói (Encapsulation), không cho phép bên ngoài gán giá trị tự do.
/// </summary>
public class User : Entity<Guid>, IAggregateRoot
{
    // private set: Chỉ cho phép gán giá trị ở bên trong class này
    // Tên đăng nhập của tài khoản
    public string Username { get; private set; } = string.Empty;

    // Email của người dùng để liên lạc hoặc gửi hóa đơn tiền điện
    public string Email { get; private set; } = string.Empty;

    // Chuỗi băm mật khẩu bảo mật (Tuyệt đối không lưu mật khẩu thô vào Database)
    public string PasswordHash { get; private set; } = string.Empty;

    // Vai trò phân quyền: Mặc định là "Driver" (Tài xế sạc xe), hoặc "Admin" (Quản trị viên)
    public string Role { get; private set; } = "Driver";
    
    /// <summary>
    /// Constructor khởi tạo tài khoản mới đầy đủ thông tin bảo mật và phân quyền
    /// </summary>
    public User(string username, string email, string passwordHash, string role = "Driver")
    {
        Id = Guid.NewGuid(); // Tự động sinh ID ngẫu nhiên không trùng lặp
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = string.IsNullOrWhiteSpace(role) ? "Driver" : role;
    }

    /// <summary>
    /// Constructor phụ tương thích ngược cho mã nguồn cũ
    /// </summary>
    public User(string username, string email) : this(username, email, string.Empty, "Driver")
    {
    }

    /// <summary>
    /// Hàm nghiệp vụ cho phép cập nhật mật khẩu mới theo chuẩn DDD
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Mật khẩu băm không được để trống.", nameof(newPasswordHash));
            
        PasswordHash = newPasswordHash;
    }

    // Constructor rỗng bắt buộc phải có để EF Core tự động nạp dữ liệu từ Database lên Object C#
    protected User() { } 
}
