// Nạp thư viện lõi DDD của chúng ta để dùng được class Entity
using SharedKernel;

namespace DeviceManagement.API.Models;

/// <summary>
/// Thực thể Trạm sạc xe điện. 
/// </summary>
public class Charger : Entity<Guid>, IAggregateRoot
{
    // 'private set': Chỉ cho phép đổi tên từ các hàm bên trong class này để bảo vệ dữ liệu
    // Bên ngoài (như Controller) không được quyền gán đè trực tiếp (vd: charger.Name = "Abc")
    public string Name { get; private set; }
    
    public string Status { get; private set; }

    // Constructor dùng để khởi tạo trạm sạc mới
    public Charger(string name)
    {
        Id = Guid.NewGuid(); // Tự sinh mã định danh ngẫu nhiên
        Name = name;
        Status = "Available"; // Trạng thái mặc định ban đầu khi vừa lắp đặt
    }

    // Constructor ẩn (dành riêng cho Entity Framework Core)
    // Lúc EF lấy dữ liệu từ Database lên, nó bắt buộc phải gọi hàm này để nhét data vào object
    protected Charger() { }

    // Đóng gói logic nghiệp vụ (Encapsulation): 
    // Thay vì để ai đó tùy tiện sửa Status, ta tạo ra hàm rõ ràng để đổi trạng thái
    public void SetStatus(string newStatus)
    {
        Status = newStatus;
    }
}
