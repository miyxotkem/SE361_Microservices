using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace BuildingBlocks.Messaging.MassTransit;
public static class Extensions
{
    public static IServiceCollection AddMessageBroker(
        this IServiceCollection services, 
        IConfiguration configuration, 
        Assembly? assembly = null, 
        Action<IBusRegistrationConfigurator>? configure = null,
        Action<IBusRegistrationContext, IRabbitMqBusFactoryConfigurator>? configureBus = null)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            if (assembly != null)
                config.AddConsumers(assembly);

            configure?.Invoke(config);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(configuration["MessageBroker:Host"] ?? "rabbitmq://localhost"), host =>
                {
                    host.Username(configuration["MessageBroker:UserName"] ?? "guest");
                    host.Password(configuration["MessageBroker:Password"] ?? "guest");
                });

                configureBus?.Invoke(context, configurator);

                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    public static IHealthChecksBuilder AddRabbitMQ(this IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var host = configuration["MessageBroker:Host"] ?? "rabbitmq://localhost";
        var username = configuration["MessageBroker:UserName"] ?? "guest";
        var password = configuration["MessageBroker:Password"] ?? "guest";

        var amqpHost = host.StartsWith("rabbitmq://") 
            ? host.Replace("rabbitmq://", "amqp://") 
            : host;

        var uri = new Uri(amqpHost);
        var port = uri.Port == -1 ? 5672 : uri.Port;
        var connectionString = $"amqp://{username}:{password}@{uri.Host}:{port}";

        return builder.AddRabbitMQ(connectionString, name: "rabbitmq");
    }
}
