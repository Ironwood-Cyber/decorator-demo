using System.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Shared.Configuration.Dll;

namespace Shared;

public static class Extensions
{
    /// <summary>
    /// Adds MongoDB as a singleton service
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <returns>The <see cref="IServiceCollection"/> with MongoDB added</returns>
    public static IServiceCollection AddMongoDB(this IServiceCollection services) =>
        services.AddSingleton(_ =>
        {
            string mongoUri = "mongodb://admin:admin@localhost:27017/test";
            return new MongoClient(mongoUri).GetDatabase("test");
        });

    /// <summary>
    /// Adds MassTransit with RabbitMq as the transport
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <returns>The <see cref="IServiceCollection"/> with MassTransit added</returns>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services) => services.AddRabbitMq((Assembly[]?)null);

    /// <summary>
    /// Adds MassTransit with RabbitMq as the transport
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="assembly">The <see cref="Assembly"/> containing consumers to register with MassTransit</param>
    /// <returns>The <see cref="IServiceCollection"/> with MassTransit added</returns>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, Assembly assembly) => services.AddRabbitMq([assembly]);

    /// <summary>
    /// Adds MassTransit with RabbitMq as the transport
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="assemblies">A list of <see cref="Assembly"/>s containing consumers to register with MassTransit</param>
    /// <returns>The <see cref="IServiceCollection"/> with MassTransit added</returns>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, Assembly[]? assemblies) =>
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            x.AddConsumers(Assembly.GetExecutingAssembly());
            if (assemblies is not null)
            {
                x.AddConsumers(assemblies);
            }
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

    /// <summary>
    /// Adds MassTransit with RabbitMq as the transport
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/></param>
    /// <param name="config">The <see cref="DllHandlerConfig"/> containing the base and decorator service DLLs with consumers to register with MassTransit</param>
    /// <returns>The <see cref="IServiceCollection"/> with MassTransit added</returns>
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, DllHandlerConfig config) =>
        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // Add the current assembly consumers
            x.AddConsumers(Assembly.GetExecutingAssembly());

            // Add the base service assembly consumers
            Assembly baseAssembly = Assembly.LoadFrom(config.BaseServiceDll);
            x.AddConsumers(baseAssembly);

            // Add the decorator service assembly consumers
            config.DecoratorServiceDlls.ForEach(decoratorServiceDll =>
            {
                Assembly decoratorAssembly = Assembly.LoadFrom(decoratorServiceDll.ServiceDll);
                x.AddConsumers(decoratorAssembly);
            });

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
