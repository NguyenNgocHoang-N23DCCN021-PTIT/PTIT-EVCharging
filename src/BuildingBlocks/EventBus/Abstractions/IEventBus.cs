// Tham chiếu đến không gian tên chứa IntegrationEvent
using EventBus.Events;

namespace EventBus.Abstractions;

// Interface này là một hợp đồng trừu tượng, không chứa logic thực tế.
public interface IEventBus
{
    // Hàm PublishAsync làm nhiệm vụ ném sự kiện vào hàng đợi (Queue).
    // Tham số @event: Nhận vào mọi đối tượng miễn là kế thừa từ IntegrationEvent.
    // Tham số cancellationToken: Dùng để an toàn hủy bỏ thao tác mạng nếu server bất ngờ bị tắt.
    // 'Task' thể hiện đây là hành động gọi mạng (I/O) chạy bất đồng bộ, không làm đơ ứng dụng.
    Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);
}
