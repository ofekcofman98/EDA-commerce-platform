using CartService.Generators.Random;
using CartService.Generators;
using CartService.Producer;
using CartService.Interfaces;
using CartService.Services;
using CartService.Data;
using CartService.Validator.Factories;
using Confluent.SchemaRegistry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Publisher
builder.Services.AddSingleton<IEventProducer, KafkaPublisher>();

// In-memory Order Repository
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// Generators (stateless - Singleton)
builder.Services.AddSingleton<IOrderGenerator, RandomOrderGenerator>();
builder.Services.AddSingleton<IItemGenerator, RandomItemGenerator>();

// Validation Factory (stateless - Singleton)
builder.Services.AddSingleton<IValidationFactory, ValidationFactory>();

// Services (business logic - Transient)
builder.Services.AddTransient<IOrderService, OrderService>();

// Schema Registry
builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new CachedSchemaRegistryClient(new SchemaRegistryConfig
    {
        Url = config["Kafka:SchemaRegistryUrl"] ?? "http://schema-registry:8081"
    });
});

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
