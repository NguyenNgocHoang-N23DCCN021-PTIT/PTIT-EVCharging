// Nạp thư viện hỗ trợ biến đổi object thành định dạng JSON để truyền qua mạng
using System.Text.Json.Serialization;

namespace EventBus.Events;

// Dùng 'record' thay cho 'class' vì event là "sự thật đã xảy ra trong quá khứ", 
// dữ liệu của nó phải bị khóa chặt (bất biến - immutable) không cho ai sửa đổi.
public record IntegrationEvent
{
    // [JsonInclude] ép buộc bộ chuyển đổi JSON phải lấy giá trị này gửi đi.
    // 'private init' đảm bảo Id chỉ được sinh ra đúng 1 lần khi new() và bị khóa lại.
    [JsonInclude]
    public Guid Id { get; private init; }

    // Lưu lại thời điểm sự kiện phát sinh để sau này dễ truy vết log
    [JsonInclude]
    public DateTime CreationDate { get; private init; }

    // Constructor mặc định (tự động được gọi khi lập trình viên tạo mới event)
    public IntegrationEvent()
    {
        // Sinh Id ngẫu nhiên chống trùng lặp
        Id = Guid.NewGuid();
        // Lấy giờ quốc tế UTC để tránh sai lệch múi giờ giữa các server khác nhau
        CreationDate = DateTime.UtcNow;
    }

    // Constructor này dành riêng cho thư viện JSON.
    // Khi một service nhận được JSON từ RabbitMQ, hệ thống sẽ dùng hàm này 
    // để nhét dữ liệu từ chuỗi JSON ngược trở lại thành Object C#.
    [JsonConstructor]
    public IntegrationEvent(Guid id, DateTime createDate)
    {
        Id = id;
        CreationDate = createDate;
    }
}
