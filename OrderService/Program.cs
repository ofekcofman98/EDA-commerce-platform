using Confluent.SchemaRegistry;
using OrderService.BackgroundServices;
using OrderService.Data;
using OrderService.Interfaces;
using OrderService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Auto-register all IOrderEventHandler implementations as Singletons
var handlerType = typeof(IOrderEventHandler);
var handlerImplementations = handlerType.Assembly.GetTypes()
    .Where(type => handlerType.IsAssignableFrom(type) && 
                   type.IsClass && 
                   !type.IsAbstract)
    .ToList();

foreach (var implementation in handlerImplementations)
{
    builder.Services.AddSingleton(handlerType, implementation);
}


builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

// Schema Registry
builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return new CachedSchemaRegistryClient(new SchemaRegistryConfig
    {
        Url = config["Kafka:SchemaRegistryUrl"] ?? "http://schema-registry:8081"
    });
});

builder.Services.AddHostedService<KafkaOrderListener>();

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
