using EventBus.Events;
namespace SharedKernel.Events;

// Khai báo bản ghi sự kiện Thanh toán hoàn tất
// Sử dụng 'record' thay vì 'class' vì sự kiện là dữ liệu lịch sử (không được phép sửa đổi)
public record PaymentCompletedEvent : IntegrationEvent
{
    // Cần gửi tin nhắn cho ai?
    public string ChargerId { get; init; }
    
    // Đã trừ bao nhiêu tiền?
    public decimal AmountDeducted { get; init; }

    public PaymentCompletedEvent(string chargerId, decimal amountDeducted)
    {
        ChargerId = chargerId;
        AmountDeducted = amountDeducted;
    }
}
