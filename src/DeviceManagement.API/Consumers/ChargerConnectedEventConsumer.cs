// Import các thư viện cần thiết
using MassTransit;
using SharedKernel.Events;
using DeviceManagement.API.Models;
using DeviceManagement.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DeviceManagement.API.Consumers;

// Class này phải kế thừa IConsumer<T> của MassTransit, với T là tần số sự kiện muốn nghe
public class ChargerConnectedEventConsumer : IConsumer<ChargerConnectedEvent>
{
    private readonly DeviceDbContext _dbContext;

    // Dependency Injection: Tự động bơm ống nối Database vào đây
    public ChargerConnectedEventConsumer(DeviceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Hàm này sẽ tự động được kích hoạt (Trigger) MỖI KHI có người hét sự kiện lên RabbitMQ
    public async Task Consume(ConsumeContext<ChargerConnectedEvent> context)
    {
        // 1. Lấy tờ giấy thông báo ra đọc
        var eventData = context.Message;
        
        // 2. Kiểm tra xem Trạm sạc này đã có trong Database chưa
        var existingCharger = await _dbContext.Chargers
            .FirstOrDefaultAsync(c => c.Id == eventData.ChargerId);

        if (existingCharger == null)
        {
            // Đi qua cổng chính: Dùng Constructor thay vì gán trực tiếp
            var newCharger = new Charger(eventData.ChargerId, eventData.ChargerName);
            _dbContext.Chargers.Add(newCharger);
        }
        else
        {
            // Đi qua cổng phụ: Dùng hàm SetStatus đã được mở sẵn
            existingCharger.SetStatus("Connected");
        }


        // 3. Bấm nút lưu xuống PostgreSQL
        await _dbContext.SaveChangesAsync();
        
        // Ghi ra màn hình Console để chúng ta dễ theo dõi
        Console.WriteLine($"[RabbitMQ] Bắt được sự kiện! Trạm sạc {eventData.ChargerName} đã kết nối và lưu vào DB.");
    }
}
