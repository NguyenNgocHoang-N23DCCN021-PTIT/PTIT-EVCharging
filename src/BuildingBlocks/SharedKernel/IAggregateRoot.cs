namespace SharedKernel;

/// <summary>
/// Marker Interface: Interface này hoàn toàn rỗng, không bắt ép viết hàm nào cả.
/// Nó chỉ dùng để dán nhãn (đánh dấu) một Entity là Aggregate Root.
/// Các Repository (Kênh giao tiếp Database) sau này sẽ được thiết kế để chỉ tiếp nhận những Entity có dán nhãn này.
/// </summary>
public interface IAggregateRoot
{
}
