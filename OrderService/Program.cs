using OrderService.BackgroundServices;
using OrderService.Data;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IOrderRepository, OrderRepository>();

builder.Services.AddSingleton(new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest",
    DispatchConsumersAsync = true
});

builder.Services.AddHostedService<RabbitMQOrderListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
