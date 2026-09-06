using EventBus.Events; // Kéo thư viện của Phase 1 vào

namespace SharedKernel.Events;

// Dùng dấu ':' để kế thừa IntegrationEvent (mang sẵn Id và CreationDate)
public record ChargerConnectedEvent : IntegrationEvent
{
    public Guid ChargerId { get; init; }
    public string ChargerName { get; init; }

    // Constructor: Truyền base() để class cha tự động sinh ID sự kiện
    public ChargerConnectedEvent(Guid chargerId, string chargerName) : base()
    {
        ChargerId = chargerId;
        ChargerName = chargerName;
        // không cần biến ConnectedAt nữa vì lớp cha đã có sẵn CreationDate rồi!
    }
}
