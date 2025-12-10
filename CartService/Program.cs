using CartService.Generators.Random;
using CartService.Generators;
using RabbitMQ.Client;
using CartService.Producer;
using CartService.OrderCreation;
using CartService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// RabbitMQ
var rmq = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddSingleton(new ConnectionFactory
{
    HostName = rmq["HostName"] ?? "localhost",
    UserName = rmq["UserName"] ?? "guest",
    Password = rmq["Password"] ?? "guest",
    Port = int.TryParse(rmq["Port"], out var p) ? p : 5672,
    DispatchConsumersAsync = true
});

// Publisher
builder.Services.AddSingleton<IEventProducer, RabbitMQPublisher>();

// In-memory Order Repository
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// Generators
builder.Services.AddScoped<IOrderGenerator, RandomOrderGenerator>();
builder.Services.AddScoped<IItemGenerator, RandomItemGenerator>();

// Services
builder.Services.AddScoped<IOrderCreationService, OrderCreationService>();


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
