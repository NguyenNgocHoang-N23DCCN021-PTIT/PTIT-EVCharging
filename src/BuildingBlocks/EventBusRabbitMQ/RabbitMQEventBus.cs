// Import thư viện IEventBus của chúng ta từ Task 1.1
using EventBus.Abstractions;
using EventBus.Events;
// Import thư viện MassTransit
using MassTransit;

namespace EventBusRabbitMQ;

/// <summary>
/// Lớp thực thi (Implementation) cho IEventBus sử dụng MassTransit làm lõi.
/// Đóng vai trò là "Phích cắm điện" cắm vào "Ổ cắm" IEventBus.
/// </summary>
public class RabbitMQEventBus : IEventBus
{
    // IPublishEndpoint là công cụ có sẵn của MassTransit dùng để bắn tin nhắn đi
    private readonly IPublishEndpoint _publishEndpoint;

    // Dependency Injection: ASP.NET Core sẽ tự động truyền IPublishEndpoint vào đây khi chạy app
    public RabbitMQEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    // Đây là nơi logic thực sự xảy ra khi ai đó gọi hàm PublishAsync
    public async Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        // Gọi thư viện MassTransit để ném @event lên RabbitMQ
        // Khối 'await' đảm bảo hệ thống không bị đơ chờ trong lúc truyền qua mạng
        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}
