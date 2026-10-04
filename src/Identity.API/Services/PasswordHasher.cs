using System.Security.Cryptography;
using System.Text;

namespace Identity.API.Services;

/// <summary>
/// Tiện ích băm và kiểm tra mật khẩu an toàn bằng thuật toán SHA-256
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// Băm mật khẩu thô thành chuỗi mã hóa một chiều Base64
    /// </summary>
    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return string.Empty;
        
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Kiểm tra mật khẩu người dùng nhập vào có khớp với mã băm trong Database không
    /// </summary>
    public static bool Verify(string password, string passwordHash)
    {
        return Hash(password) == passwordHash;
    }
}
