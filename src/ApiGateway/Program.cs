var builder = WebApplication.CreateBuilder(args);

// 1. Nhúng ServiceDefaults (Log, Đo lường...)
builder.AddServiceDefaults();

// 2. Kích hoạt tính năng YARP Reverse Proxy, đọc cấu hình và BẬT BỘ PHÂN GIẢI SERVICE DISCOVERY CỦA ASPIRE
builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
       .AddServiceDiscoveryDestinationResolver(); // <-- Thêm dòng này để YARP hiểu địa chỉ https+http://

var app = builder.Build();

// 3. Khai báo các endpoint mặc định của ServiceDefaults
app.MapDefaultEndpoints();

// 4. Áp dụng các quy tắc điều hướng của YARP vào ứng dụng
app.MapReverseProxy();

app.Run();
