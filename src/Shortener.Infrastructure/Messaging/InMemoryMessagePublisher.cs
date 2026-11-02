namespace Shortener.Infrastructure.Messaging;

using System.Collections.Concurrent;
using Shortener.Application.Common.Interfaces;

public class InMemoryMessagePublisher : IMessagePublisher
{
    public readonly ConcurrentQueue<object> MessageQueue = new();

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        if (message != null)
        {
            MessageQueue.Enqueue(message);
        }
        return Task.CompletedTask;
    }
}
