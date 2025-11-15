using CartService;
using CartService.Generators.Random;
using CartService.Generators;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton(new ConnectionFactory { HostName = "localhost" });
builder.Services.AddSingleton<IEventProducer, RabbitMQPublisher>();

builder.Services.AddScoped<IOrderGenerator, RandomOrderGenerator>();
builder.Services.AddScoped<IItemGenerator, RandomItemGenerator>();

builder.Services.AddScoped<IOrderCreationService, OrderCreationService>();



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
