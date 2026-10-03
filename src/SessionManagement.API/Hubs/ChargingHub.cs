using Microsoft.AspNetCore.SignalR;

namespace SessionManagement.API.Hubs;

/// <summary>
/// Hub SignalR quản lý kênh truyền thông hai chiều (WebSockets) giữa máy chủ và ứng dụng di động.
/// Thay vì Client phải gửi HTTP GET liên tục (Polling gây nghẽn mạch), Hub này cho phép Server 
/// chủ động bắn dữ liệu đo đạc (Telemetry) xuống điện thoại theo thời gian thực.
/// </summary>
public class ChargingHub : Hub
{
    /// <summary>
    /// Cho phép Mobile App đăng ký lắng nghe dữ liệu của một trụ sạc cụ thể.
    /// Kỹ thuật: Phân nhóm (Group) theo chargerId để dữ liệu xe nào chỉ gửi về đúng máy người đó,
    /// tránh việc phát sóng bừa bãi (Broadcast) làm tốn băng thông của tất cả các máy khác.
    /// </summary>
    /// <param name="chargerId">Mã định danh của trụ sạc mà xe đang cắm</param>
    public async Task JoinChargingStation(string chargerId)
    {
        // Context.ConnectionId: Mã định danh duy nhất của phiên kết nối WebSocket từ điện thoại
        // Thêm kết nối này vào nhóm có tên là chargerId
        await Groups.AddToGroupAsync(Context.ConnectionId, chargerId);
    }

    /// <summary>
    /// Khi tài xế rút súng sạc hoặc thoát ứng dụng, điện thoại sẽ rời nhóm nhận tin.
    /// </summary>
    /// <param name="chargerId">Mã định danh của trụ sạc muốn ngừng theo dõi</param>
    public async Task LeaveChargingStation(string chargerId)
    {
        // Xóa kết nối khỏi nhóm để máy chủ ngừng gửi thông số về thiết bị này, tiết kiệm RAM/CPU
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chargerId);
    }
}
