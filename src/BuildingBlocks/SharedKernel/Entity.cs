namespace SharedKernel;

/// <summary>
/// Lớp cơ sở cho mọi Entity trong hệ thống (như Trạm Sạc, Người Dùng, Giao Dịch).
/// Chữ 'TId' là generic type (kiểu dữ liệu động), cho phép Id có thể là int, string, hoặc Guid tùy ý.
/// </summary>
public abstract class Entity<TId>
{
    // Bất kỳ Entity nào cũng bắt buộc phải có một định danh (Id)
    // protected set: Chỉ bản thân lớp này và các lớp kế thừa mới được sửa Id
    public TId Id { get; protected set; }

    // Override phương thức Equals của C# để so sánh 2 Entity.
    // Nguyên lý DDD: Hai Entity là MỘT nếu chúng cùng kiểu và có chung Id.
    public override bool Equals(object? obj)
    {
        // Nếu object truyền vào rỗng, hoặc không phải là Entity cùng kiểu, thì chắc chắn không bằng nhau
        if (obj is not Entity<TId> other)
            return false;

        // Nếu cả 2 đều trỏ đến cùng 1 vùng nhớ bộ nhớ Ram, thì chắc chắn là bằng nhau
        if (ReferenceEquals(this, other))
            return true;

        // Nếu Id chưa được gán giá trị, hoặc Id của 2 đối tượng khác nhau, thì không bằng nhau
        if (Id!.Equals(default) || other.Id!.Equals(default))
            return false;

        // Tiến hành so sánh 2 chuỗi/số Id với nhau
        return Id.Equals(other.Id);
    }

    // Override GetHashCode để đồng bộ với hàm Equals phía trên (quy định bắt buộc của C#)
    public override int GetHashCode()
    {
        return Id!.GetHashCode();
    }
}
