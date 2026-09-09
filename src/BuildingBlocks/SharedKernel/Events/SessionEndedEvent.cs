using System;
using EventBus.Events;

namespace SharedKernel.Events;

// Kế thừa IntegrationEvent để đánh dấu đây là sự kiện liên dịch vụ (vượt ra ngoài phạm vi 1 API)
public record SessionEndedEvent : IntegrationEvent
{
    public string ChargerId { get; private set; }
    
    // Thời lượng sạc (tính bằng phút) để Billing dùng nhân với đơn giá
    public double DurationMinutes { get; private set; }

    public SessionEndedEvent(string chargerId, double durationMinutes)
    {
        ChargerId = chargerId;
        DurationMinutes = durationMinutes;
    }
}
