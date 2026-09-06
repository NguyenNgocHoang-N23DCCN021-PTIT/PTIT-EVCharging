// Import các "bảo bối"
using EventBus.Abstractions;
using SharedKernel.Events;
using MassTransit;          // Thư viện lõi xử lý RabbitMQ
using EventBusRabbitMQ;     // Class RabbitMQEventBus của chúng ta

var builder = WebApplication.CreateBuilder(args);

// BƯỚC 2.1: LẮP RÁP HỆ THỐNG LOA (MASS TRANSIT)
// Gọi thẳng thư viện MassTransit và bảo nó: "Hãy nối dây vào cái Server RabbitMQ có tên là rabbitmq-bus do ông Aspire cấu hình nhé!"
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        // Lấy đường dẫn (Connection String) từ "Nhạc trưởng" AppHost truyền sang
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));
    });
});

// BƯỚC 2.2: ĐĂNG KÝ DỊCH VỤ IEventBus
// Dòng này báo cho hệ thống biết: Bất cứ khi nào có ai xin xài IEventBus, hãy ném cho họ cái RabbitMQEventBus.
builder.Services.AddScoped<IEventBus, RabbitMQEventBus>();

var app = builder.Build();

// ==========================================
// BƯỚC 2.3: GIẢ LẬP TRẠM SẠC KẾT NỐI (API)
// ==========================================
app.MapPost("/api/simulate-connection", async (string chargerName, IEventBus eventBus) =>
{
    // B1: Trạm sạc báo danh, tự sinh ra 1 cái ID đại diện
    var fakeChargerId = Guid.NewGuid();
    
    // B2: Viết một tờ giấy thông báo (Event)
    var connectedEvent = new ChargerConnectedEvent(fakeChargerId, chargerName);
    
    // B3: Đưa tờ giấy vào loa và HÉT LÊN mạng lưới RabbitMQ (Publish)
    await eventBus.PublishAsync(connectedEvent);
    
    // B4: Báo cho người dùng biết là đã hét thành công
    return Results.Ok(new { Message = "Đã phát loa thành công lên RabbitMQ!", EventData = connectedEvent });
});

app.Run();
