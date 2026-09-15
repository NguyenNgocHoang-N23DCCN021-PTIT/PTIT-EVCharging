using Billing.API.Models;
using Microsoft.EntityFrameworkCore;

// Đổi Hộ khẩu về Infrastructure
namespace Billing.API.Infrastructure;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
}
