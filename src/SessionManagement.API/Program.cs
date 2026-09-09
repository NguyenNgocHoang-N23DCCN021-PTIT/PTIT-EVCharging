using Microsoft.Extensions.Caching.Distributed;
using EventBus.Abstractions;
using EventBusRabbitMQ;
using MassTransit;
using SharedKernel.Events;
var builder = WebApplication.CreateBuilder(args);

// Khởi tạo Swagger để có giao diện kiểm thử API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CẤU HÌNH HẠ TẦNG (INFRASTRUCTURE)
// Yêu cầu ASP.NET Core kết nối tới server Redis có tên "redis-cache" do AppHost cung cấp
builder.AddRedisDistributedCache("redis-cache");

// Cấu hình sẵn hệ thống phát sự kiện MassTransit/RabbitMQ chuẩn bị cho Task 5.2
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("rabbitmq-bus"));
    });
});
builder.Services.AddScoped<IEventBus, RabbitMQEventBus>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ENDPOINT 1: BẮT ĐẦU PHIÊN SẠC
app.MapPost("/api/sessions/start", async (string chargerId, IDistributedCache cache) =>
{
    // Sử dụng UTC để chuẩn hóa thời gian trên toàn cầu, tránh lỗi lệch múi giờ
    var startTime = DateTime.UtcNow;
    
    // Lưu trạng thái vào Redis RAM. 
    // - Cấu trúc Key: Gắn tiền tố "Session_" + ID trạm để tránh xung đột dữ liệu (Colission).
    // - Cấu trúc Value: Định dạng ISO 8601 (ToString("O")) để bảo toàn độ chính xác của DateTime khi ép sang chuỗi.
    await cache.SetStringAsync($"Session_{chargerId}", startTime.ToString("O"));
    
    return Results.Ok(new { ChargerId = chargerId, StartTime = startTime, Message = "Đã bắt đầu bơm điện!" });
})
.WithName("StartSession")
.WithOpenApi();

// ENDPOINT 2: KẾT THÚC PHIÊN SẠC
app.MapPost("/api/sessions/stop", async (string chargerId, IDistributedCache cache, IEventBus eventBus) =>
{
    // Đọc thời điểm bắt đầu sạc trực tiếp từ Redis bằng Key tương ứng
    var startTimeString = await cache.GetStringAsync($"Session_{chargerId}");
    
    // Bắt lỗi: Nếu không có dữ liệu nghĩa là trạm gửi tín hiệu Stop ảo hoặc Redis đã bị rớt mạng/mất dữ liệu
    if (string.IsNullOrEmpty(startTimeString))
    {
        return Results.BadRequest("Lỗi: Không tìm thấy phiên sạc nào đang chạy cho trạm này!");
    }

    // Tính toán thời lượng sạc (tính bằng phút) để phục vụ cho việc tính tiền sau này
    var startTime = DateTime.Parse(startTimeString);
    var endTime = DateTime.UtcNow;
    var durationMinutes = (endTime - startTime).TotalMinutes;

    // QUAN TRỌNG: Xóa Key ngay lập tức khỏi Redis để giải phóng RAM, tránh rò rỉ bộ nhớ (Memory Leak)
    await cache.RemoveAsync($"Session_{chargerId}");

    // GỬI SỰ KIỆN ĐỂ BỘ PHẬN TÍNH TIỀN NGHE
    // Tạo ra đối tượng chứa dữ liệu sự kiện
    var sessionEndedEvent = new SessionEndedEvent(chargerId, durationMinutes);

    // Gửi dữ liệu này đi. 2 tham số bên trong là tên Exchange (tên nhóm) vàrouting key
    await eventBus.PublishAsync(sessionEndedEvent);

    // Trả về kết quả thành công cho người gọi (bộ điều khiển trạm)
    return Results.Ok(new 
    { 
        ChargerId = chargerId, 
        DurationMinutes = durationMinutes, 
        Message = "Đã dừng sạc thành công & Gửi thông tin tính tiền xong!" 
    });
})
.WithName("StopSession")
.WithOpenApi();

app.Run();
