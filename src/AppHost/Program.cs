var builder = DistributedApplication.CreateBuilder(args);

// Khai báo các tài nguyên hạ tầng
var redis = builder.AddRedis("redis-cache");
var postgresServer = builder.AddPostgres("postgres-server").WithPgAdmin();

// Tạo các Database riêng biệt cho từng Domain theo chuẩn Microservices (Database-per-Service)
var deviceDb = postgresServer.AddDatabase("device-db");
var identityDb = postgresServer.AddDatabase("identity-db");
var billingDb = postgresServer.AddDatabase("billing-db");
var rabbitmq = builder.AddRabbitMQ("rabbitmq-bus").WithManagementPlugin();

// 1. Gateway giao tiếp trụ sạc phần cứng qua giao thức OCPP
builder.AddProject<Projects.Ocpp_Gateway>("ocpp-gateway")
       .WithReference(rabbitmq);

// 2. SessionManagement quản lý phiên sạc thời gian thực, lưu biến var sessionApi để Gateway tham chiếu
var sessionApi = builder.AddProject<Projects.SessionManagement_API>("session-management-api")
       .WithReference(redis)
       .WithReference(rabbitmq);

// 3. SmartCharging xử lý thuật toán cân bằng tải điện
builder.AddProject<Projects.SmartCharging_API>("smart-charging-api")
       .WithReference(rabbitmq);

// 4. Billing lưu hóa đơn vào Postgres và tính tiền, lưu biến var billingApi để Gateway tham chiếu
var billingApi = builder.AddProject<Projects.Billing_API>("billing-api")
       .WithReference(billingDb)
       .WithReference(rabbitmq);

// 5. Identity quản lý user và phân quyền, lưu biến var identityApi để Gateway tham chiếu
var identityApi = builder.AddProject<Projects.Identity_API>("identity-api")
       .WithReference(identityDb);

// 6. Notification lắng nghe sự kiện từ RabbitMQ để nhắn tin "Ting ting"
builder.AddProject<Projects.Notification_API>("notification-api")
       .WithReference(rabbitmq);

// 7. DeviceManagement quản lý danh mục và tọa độ trạm sạc
var deviceApi = builder.AddProject<Projects.DeviceManagement_API>("device-management-api")
                       .WithReference(deviceDb)
                       .WithReference(rabbitmq);

// 8. Đăng ký API Gateway (YARP Reverse Proxy) và nạp Service Discovery tới tất cả các Microservices
builder.AddProject<Projects.ApiGateway>("api-gateway")
       .WithReference(deviceApi)       // Cho phép điều hướng tới DeviceManagement.API
       .WithReference(sessionApi)      // Cho phép điều hướng tới SessionManagement.API (Start/Stop sạc & SignalR Hub)
       .WithReference(billingApi)      // Cho phép điều hướng tới Billing.API
       .WithReference(identityApi);    // Cho phép điều hướng tới Identity.API

builder.Build().Run();
