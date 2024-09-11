using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Sample.Common.Messaging.Abstractions;
using Sample.Common.Types;

namespace Sample.Infrastructure.Messaging.Kafka.Consumer;

public sealed class KafkaMessageProcessor<T> where T: class, IMessage
{
    private readonly IServiceProvider _serviceProvider;

    public KafkaMessageProcessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<Result<Unit>> Process(ConsumeResult<Ignore, byte[]> cr, CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        
        
        return Result.UnitResult;
    }
}