using MassTransit;
using SearchService.Consumers;
using SearchService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(x => 
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    x.UsingRabbitMq((context, conf) =>
    {
        conf.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest")!);
            host.Password(builder.Configuration.GetValue("RabbitMq:Username", "guest")!);
        });
        
        conf.ReceiveEndpoint("search-auction-created", y =>
        {
            y.UseMessageRetry(z => z.Interval(5, 5));
            y.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        conf.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();
