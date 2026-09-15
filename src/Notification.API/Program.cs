using MassTransit;
using Notification.API.Consumers;

var builder = WebApplication.CreateBuilder(args);

// 1. Kế thừa các cấu hình chuẩn của Aspire (Tracing, Metrics, Log)
builder.AddServiceDefaults();

// 2. Cấu hình đội quân Sát thủ đi săn tin trên RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Đăng ký Sát thủ vừa tạo ở Bước 6.5
    x.AddConsumer<PaymentCompletedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Kết nối vào Bưu điện RabbitMQ (chuỗi kết nối do AppHost bơm vào)
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq"));
        
        // Tự động xây Hộp thư (Queue) và móc dây (Binding) cho Sát thủ
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// API rác để thử tải xem dịch vụ có sống không
app.MapGet("/", () => "Notification.API đang trực chiến!");

app.Run();
