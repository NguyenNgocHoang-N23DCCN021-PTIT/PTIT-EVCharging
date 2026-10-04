using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Identity.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Services;

/// <summary>
/// Giao diện cấp phát Token theo nguyên lý Dependency Inversion (SOLID)
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
}

/// <summary>
/// Dịch vụ sinh chuỗi JSON Web Token (JWT) có chữ ký số bí mật
/// </summary>
public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        // 1. Đọc các tham số cấu hình từ appsettings.json (Tuân thủ No hardcoding)
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] 
            ?? throw new InvalidOperationException("Chưa cấu hình Jwt:Key trong appsettings.json");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expireMinutes = double.Parse(jwtSettings["ExpireMinutes"] ?? "1440");

        // 2. Định nghĩa các "Claims" (Thông tin định danh đính kèm trong token)
        var claims = new[]
        {
            // sub (Subject): ID người dùng
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // name: Tên tài khoản
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            // email: Địa chỉ thư điện tử
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            // role: Vai trò phân quyền (Driver hoặc Admin)
            new Claim(ClaimTypes.Role, user.Role),
            // jti (JWT ID): Mã ngẫu nhiên duy nhất cho mỗi Token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 3. Tạo chữ ký số bí mật bằng thuật toán HMAC-SHA256
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. Đóng gói Token hoàn chỉnh với thời hạn hết hạn
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        // 5. Chuyển đổi đối tượng Token thành chuỗi ký tự Base64 gửi cho Client
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
