using SharedKernel;

namespace Billing.API.Models;

public class Wallet : Entity<Guid>, IAggregateRoot
{
    // ID của chủ ví (Tạm thời chúng ta sẽ dùng ChargerId để đại diện cho khách hàng đang sạc ở trụ đó)
    public string UserId { get; private set; }
    public decimal Balance { get; private set; }

    protected Wallet() { }

    public Wallet(string userId, decimal initialBalance)
    {
        UserId = userId;
        Balance = initialBalance;
    }

    // Hành vi của Ví: Rút tiền (Chuẩn DDD - Nhốt logic vào bên trong Model)
    public void Deduct(decimal amount)
    {
        if (Balance < amount)
        {
            // Nếu không đủ tiền, ném ra lỗi (Ở đời thực thì sinh ra nợ xấu)
            throw new Exception("Số dư trong ví không đủ để thanh toán!");
        }
        Balance -= amount;
    }
}
