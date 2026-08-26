var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis-cache");
var postgres = builder.AddPostgres("postgres-db").WithPgAdmin();
var rabbitmq = builder.AddRabbitMQ("rabbitmq-bus").WithManagementPlugin();
builder.AddProject<Projects.Ocpp_Gateway>("ocpp-gateway");
builder.AddProject<Projects.DeviceManagement_API>("device-management-api");
builder.AddProject<Projects.SessionManagement_API>("session-management-api");
builder.AddProject<Projects.SmartCharging_API>("smart-charging-api");
builder.AddProject<Projects.Billing_API>("billing-api");
builder.AddProject<Projects.Identity_API>("identity-api");
builder.Build().Run();
