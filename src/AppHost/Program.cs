var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis-cache");
var postgres = builder.AddPostgres("postgres-db").WithPgAdmin();
var rabbitmq = builder.AddRabbitMQ("rabbitmq-bus").WithManagementPlugin();

builder.Build().Run();
