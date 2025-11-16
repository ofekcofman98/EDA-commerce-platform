using OrderService.BackgroundServices;
using OrderService.Data;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var rmq = builder.Configuration.GetSection("RabbitMQ");

builder.Services.AddSingleton<ConnectionFactory>(_ =>
{
    return new ConnectionFactory
    {
        HostName = rmq["HostName"] ?? "rabbitmq",
        UserName = rmq["UserName"] ?? "user",
        Password = rmq["Password"] ?? "password",
        Port = int.TryParse(rmq["Port"], out var p) ? p : 5672,
        DispatchConsumersAsync = true
    };
});


builder.Services.AddSingleton<IOrderRepository, OrderRepository>();

//builder.Services.AddSingleton(new ConnectionFactory
//{
//    HostName = "localhost",
//    UserName = "guest",
//    Password = "guest",
//    DispatchConsumersAsync = true
//});

builder.Services.AddHostedService<RabbitMQOrderListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
