// Nhúng namespace của Database (Infrastructure) và Các Model (Domain)
using Billing.API.Infrastructure;
using Billing.API.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Events;

// Khai báo hộ khẩu Application
namespace Billing.API.Consumers;

public class SessionEndedEventConsumer : IConsumer<SessionEndedEvent>
{
    private readonly BillingDbContext _dbContext;

    public SessionEndedEventConsumer(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<SessionEndedEvent> context)
    {
        var message = context.Message;
        decimal pricePerMinute = 5000m; // 5k VNĐ / 1 phút

        // 1. TÍNH TIỀN & LẬP HÓA ĐƠN
        var invoice = new Invoice(message.ChargerId, message.DurationMinutes, pricePerMinute);
        _dbContext.Invoices.Add(invoice);

        // 2. TÌM VÍ ĐIỆN TỬ & TRỪ TIỀN
        var wallet = await _dbContext.Wallets.FirstOrDefaultAsync(w => w.UserId == message.ChargerId);
        
        if (wallet == null)
        {
            // Khuyến mãi khởi nghiệp cho khách hàng mới
            wallet = new Wallet(message.ChargerId, 500000m);
            _dbContext.Wallets.Add(wallet);
        }

        // Gọi hàm trừ tiền trong Model (DDD)
        wallet.Deduct(invoice.TotalAmount);

        // 3. CHỐT SỔ TẤT CẢ GIAO DỊCH VÀO DATABASE
        await _dbContext.SaveChangesAsync();

        Console.WriteLine($"[BILLING] Trạm {message.ChargerId}: Hóa đơn {invoice.TotalAmount:C}. Số dư ví còn lại: {wallet.Balance:C}");
    }
}
