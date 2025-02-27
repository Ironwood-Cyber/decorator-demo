using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Shared;

public static class Extensions
{
    public static IServiceCollection AddMongoDB(this IServiceCollection services) =>
        services.AddSingleton(_ =>
        {
            string mongoUri = "mongodb://admin:admin@localhost:27017/test";
            return new MongoClient(mongoUri).GetDatabase("test");
        });

    public static IServiceCollection AddRabbitMq(this IServiceCollection services) =>
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumers(Assembly.GetExecutingAssembly());
            x.UsingRabbitMq(
                (context, cfg) =>
                {
                    cfg.Host(
                        "localhost",
                        "/",
                        h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        }
                    );

                    cfg.ConfigureEndpoints(context);
                }
            );
        });
}
