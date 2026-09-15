using MassTransit;
using SharedKernel.Events;

namespace Notification.API.Consumers;

// Consumer này chuyên đi hứng các tờ giấy mang tên PaymentCompletedEvent
public class PaymentCompletedEventConsumer : IConsumer<PaymentCompletedEvent>
{
    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;

        // Giả lập độ trễ của việc gọi API bên thứ 3 (Zalo/Firebase) để gửi tin nhắn
        await Task.Delay(1000); 

        // Gửi Push Notification (Ở đây ta in ra màn hình giả lập)
        Console.WriteLine("\n=======================================================");
        Console.WriteLine($"[NOTIFICATION] TING TING! Màn hình điện thoại sáng lên:");
        Console.WriteLine($"Gửi tới tài xế ở trạm: {message.ChargerId}");
        Console.WriteLine($"Nội dung: 'Xe của bạn đã sạc xong! Ví điện tử vừa bị trừ {message.AmountDeducted:C}'");
        Console.WriteLine("=======================================================\n");
    }
}
