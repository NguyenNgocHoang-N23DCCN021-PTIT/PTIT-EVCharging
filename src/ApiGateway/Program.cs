var builder = WebApplication.CreateBuilder(args);

// 1. Nhúng ServiceDefaults (Log, Đo lường...)
builder.AddServiceDefaults();

// 2. Kích hoạt tính năng YARP Reverse Proxy và đọc cấu hình từ file appsettings.json
builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 3. Khai báo các endpoint mặc định của ServiceDefaults
app.MapDefaultEndpoints();

// 4. Áp dụng các quy tắc điều hướng của YARP vào ứng dụng
app.MapReverseProxy();

app.Run();
