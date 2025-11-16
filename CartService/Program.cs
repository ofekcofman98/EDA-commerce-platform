using CartService.Generators.Random;
using CartService.Generators;
using RabbitMQ.Client;
using CartService.Producer;
using CartService.OrderCreation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// RabbitMQ
//builder.Services.AddSingleton(new ConnectionFactory
//{
//    HostName = "localhost",
//    UserName = "guest",
//    Password = "guest",
//    DispatchConsumersAsync = true
//});

var rmq = builder.Configuration.GetSection("RabbitMQ");
builder.Services.AddSingleton(new ConnectionFactory
{
    HostName = rmq["HostName"] ?? "localhost",
    UserName = rmq["UserName"] ?? "guest",
    Password = rmq["Password"] ?? "guest",
    Port = int.TryParse(rmq["Port"], out var p) ? p : 5672,
    DispatchConsumersAsync = true
});

builder.Services.AddSingleton<IEventProducer, RabbitMQPublisher>();

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
