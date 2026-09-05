using Microsoft.EntityFrameworkCore;
// Import thư mục Models để thấy được class Charger
using DeviceManagement.API.Models;

namespace DeviceManagement.API.Infrastructure;

public class DeviceDbContext : DbContext
{
    public DeviceDbContext(DbContextOptions<DeviceDbContext> options) : base(options)
    {
    }

    // DÒNG MỚI NÀY SẼ RA LỆNH TẠO BẢNG "Chargers" DƯỚI DATABASE POSTGRESQL
    public DbSet<Charger> Chargers { get; set; }
}
