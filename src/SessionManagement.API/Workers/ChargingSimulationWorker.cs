using Microsoft.AspNetCore.SignalR;
using SessionManagement.API.Hubs;

namespace SessionManagement.API.Workers;

/// <summary>
/// Tiến trình chạy ngầm (Background Service) giả lập phần cứng trụ sạc đẩy thông số đo đạc (Telemetry).
/// Kế thừa từ BackgroundService của Microsoft.Extensions.Hosting.
/// </summary>
public class ChargingSimulationWorker : BackgroundService
{
    // IHubContext cho phép gửi thông điệp từ bên ngoài Hub vào các Client đang kết nối
    private readonly IHubContext<ChargingHub> _hubContext;
    private readonly ILogger<ChargingSimulationWorker> _logger;

    // Giả lập trạng thái của một trụ sạc mẫu có ID là "CHARGER_01"
    private const string SimulatedChargerId = "CHARGER_01";
    private double _currentKWh = 5.2; // Số kWh khởi điểm đã nạp vào xe
    private int _batteryPercent = 35; // Phần trăm pin khởi điểm của xe

    public ChargingSimulationWorker(IHubContext<ChargingHub> hubContext, ILogger<ChargingSimulationWorker> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Vòng lặp chính chạy ngầm trong suốt vòng đời của Microservice
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Charging Simulation Worker đã khởi động, bắt đầu giả lập đo đạc thông số sạc...");

        // Chạy liên tục cho đến khi có tín hiệu tắt server (stoppingToken được kích hoạt)
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Giả lập thông số điện đo đạc từ trạm sạc chuẩn sạc nhanh DC
            var random = new Random();
            double voltage = Math.Round(395.0 + random.NextDouble() * 10, 1); // Dao động quanh 395V - 405V
            double current = Math.Round(30.0 + random.NextDouble() * 4, 1);   // Dao động quanh 30A - 34A
            double powerKw = Math.Round((voltage * current) / 1000.0, 2);    // Công suất sạc P = U * I (kW)

            // Mỗi 5 giây tăng một lượng điện tích lũy và % pin
            _currentKWh = Math.Round(_currentKWh + 0.15, 2);
            if (_batteryPercent < 100 && random.Next(0, 3) == 0)
            {
                _batteryPercent += 1;
            }

            // Gói dữ liệu gửi về Client (Mobile App)
            var telemetryData = new
            {
                ChargerId = SimulatedChargerId,
                Voltage = voltage,           // Vôn (V)
                Current = current,           // Ampe (A)
                Power = powerKw,             // Công suất (kW)
                TotalKWh = _currentKWh,      // Tổng số kWh đã vào pin
                BatterySoC = _batteryPercent,// Phần trăm pin hiện tại (%)
                Timestamp = DateTime.UtcNow  // Thời gian đo theo chuẩn UTC
            };

            // 2. Bắn dữ liệu tới Group riêng của trụ sạc này thông qua SignalR
            // Tên sự kiện lắng nghe trên Client là "ReceiveChargingMetrics"
            await _hubContext.Clients.Group(SimulatedChargerId)
                .SendAsync("ReceiveChargingMetrics", telemetryData, stoppingToken);

            _logger.LogInformation("⚡ [SignalR Push] Trụ {Charger}: {Power} kW | {SoC}% Pin | {KWh} kWh", 
                SimulatedChargerId, powerKw, _batteryPercent, _currentKWh);

            // 3. Tạm nghỉ 5 giây trước khi thực hiện lần đo tiếp theo
            await Task.Delay(5000, stoppingToken);
        }
    }
}
