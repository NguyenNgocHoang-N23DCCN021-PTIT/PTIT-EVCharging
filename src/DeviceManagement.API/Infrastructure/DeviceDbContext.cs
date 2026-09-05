using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.API.Infrastructure;

/// <summary>
/// Lớp đại diện cho cơ sở dữ liệu của dịch vụ DeviceManagement.
/// Kế thừa DbContext của Microsoft.
/// </summary>
public class DeviceDbContext : DbContext
{
    // Constructor nhận cấu hình từ Dependency Injection (do Aspire tự động truyền vào)
    // base(options): Chuyển cấu hình đó xuống cho lớp cha DbContext xử lý
    public DeviceDbContext(DbContextOptions<DeviceDbContext> options) : base(options)
    {
    }

    // Sau này (ở Task 3.2), ta sẽ khai báo các bảng dữ liệu (DbSet) ở đây. 
    // Ví dụ: public DbSet<Charger> Chargers { get; set; }
    // Hiện tại cứ để trống.
}
