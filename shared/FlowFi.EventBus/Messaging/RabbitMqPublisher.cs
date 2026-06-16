using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace FlowFi.EventBus.Messaging;

public sealed class RabbitMqPublisher
{
    private readonly ConnectionFactory _factory;

    public RabbitMqPublisher(IConfiguration configuration)
    {
        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:Host"] ?? "localhost",
            UserName = configuration["RabbitMq:Username"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest"
        };
    }

    public Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("flowfi.events", ExchangeType.Topic, durable: true);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish("flowfi.events", routingKey, basicProperties: null, body: body);
        return Task.CompletedTask;
    }
}

