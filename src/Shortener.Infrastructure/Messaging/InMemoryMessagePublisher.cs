namespace Shortener.Infrastructure.Messaging;

using System.Collections.Concurrent;
using Shortener.Application.Common.Interfaces;
using Shortener.Domain.Events;

public class InMemoryMessagePublisher : IMessagePublisher
{
    private readonly IClickMetricRepository? _metricRepository;
    public readonly ConcurrentQueue<object> MessageQueue = new();

    public InMemoryMessagePublisher(IClickMetricRepository? metricRepository = null)
    {
        _metricRepository = metricRepository;
    }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        if (message != null)
        {
            MessageQueue.Enqueue(message);

            if (message is LinkClickedEvent click && _metricRepository != null)
            {
                // Processamento assíncrono simulando o Background Service consumidor de RabbitMQ
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _metricRepository.RecordClickAsync(click.ShortCode, click.ClickedAt, click.IpAddress, click.UserAgent);
                    }
                    catch
                    {
                        // Resiliente a falhas
                    }
                });
            }
        }
        return Task.CompletedTask;
    }
}
