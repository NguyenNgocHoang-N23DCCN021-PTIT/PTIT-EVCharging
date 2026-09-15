using SharedKernel;

namespace Billing.API.Models;

// Kế thừa Entity (thuộc tính Id) và IAggregateRoot để đánh dấu đây là đối tượng chính
public class Invoice : Entity<Guid>, IAggregateRoot
{
    // Sử dụng "private set" tuân thủ nguyên lý DDD (Bảo vệ dữ liệu không bị sửa bậy bạ từ bên ngoài)
    public string ChargerId { get; private set; }
    public double DurationMinutes { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Constructor trống bắt buộc để Entity Framework Core có thể tự động tạo đối tượng khi đọc từ DB
    protected Invoice() { }

    // Chỉ cho phép tạo Hóa đơn thông qua Constructor này
    public Invoice(string chargerId, double durationMinutes, decimal pricePerMinute)
    {
        ChargerId = chargerId;
        DurationMinutes = durationMinutes;
        
        // Tính tiền: Ép kiểu double sang decimal (chuẩn tiền tệ) để không bị sai số thập phân
        TotalAmount = (decimal)durationMinutes * pricePerMinute;
        
        CreatedAt = DateTime.UtcNow;
    }
}
