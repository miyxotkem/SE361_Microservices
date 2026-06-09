using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.MassTransit;
public static class Extensions
{
    public static IServiceCollection AddMessageBroker
        (this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            if (assembly != null)
                config.AddConsumers(assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(configuration["MessageBroker:Host"] ?? "rabbitmq://localhost"), host =>
                {
                    host.Username(configuration["MessageBroker:UserName"] ?? "guest");
                    host.Password(configuration["MessageBroker:Password"] ?? "guest");
                });
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
