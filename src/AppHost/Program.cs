var builder = DistributedApplication.CreateBuilder(args);

// Khai báo các tài nguyên hạ tầng
var redis = builder.AddRedis("redis-cache");
var postgres = builder.AddPostgres("postgres-db").WithPgAdmin();
var rabbitmq = builder.AddRabbitMQ("rabbitmq-bus").WithManagementPlugin();

// 1. Gateway giao tiếp với trụ sạc, nó cần đẩy sự kiện lên RabbitMQ
builder.AddProject<Projects.Ocpp_Gateway>("ocpp-gateway")
       .WithReference(rabbitmq);



// 3. SessionManagement quản lý phiên sạc thời gian thực, cần Redis để đọc/ghi tốc độ cao
builder.AddProject<Projects.SessionManagement_API>("session-management-api")
       .WithReference(redis)
       .WithReference(rabbitmq);

// 4. SmartCharging xử lý thuật toán cân bằng tải, chỉ cần RabbitMQ để nhận lệnh
builder.AddProject<Projects.SmartCharging_API>("smart-charging-api")
       .WithReference(rabbitmq);

// 5. Billing lưu hóa đơn vào Postgres và tính tiền
builder.AddProject<Projects.Billing_API>("billing-api")
       .WithReference(postgres)
       .WithReference(rabbitmq);

// 6. Identity quản lý user, lưu tài khoản vào Postgres
builder.AddProject<Projects.Identity_API>("identity-api")
       .WithReference(postgres);
// Đăng ký API Gateway vào hệ thống
var deviceApi = builder.AddProject<Projects.DeviceManagement_API>("device-management-api")
                       .WithReference(postgres)
                       .WithReference(rabbitmq);
// Sửa lại đăng ký API Gateway (Thêm WithReference)
builder.AddProject<Projects.ApiGateway>("api-gateway")
       .WithReference(deviceApi);
builder.Build().Run();
