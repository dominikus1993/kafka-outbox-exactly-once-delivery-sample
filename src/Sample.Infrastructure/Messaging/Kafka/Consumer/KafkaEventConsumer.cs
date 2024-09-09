using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Sample.Common.Messaging.Abstractions;

namespace Sample.Infrastructure.Messaging.Kafka.Consumer;

public sealed class KafkaEventConsumer<T> : BackgroundService where T: IMessage
{
    private readonly ConsumerConfig _consumerConfig;

    public KafkaEventConsumer(ConsumerConfig consumerConfig)
    {
        _consumerConfig = consumerConfig;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Consume(stoppingToken), stoppingToken);
    }
    
    private async Task Consume(CancellationToken stoppingToken)
    {
        var pConfig = new ProducerConfig
        {
            BootstrapServers = _consumerConfig.BootstrapServers,
            ClientId = clientId + "_producer",
            // The TransactionalId identifies this instance of the map words processor.
            // If you start another instance with the same transactional id, the existing
            // instance will be fenced.
            TransactionalId = TransactionalIdPrefix_MapWords + "-" + clientId
        };

        var cConfig = new ConsumerConfig
        {
            BootstrapServers = brokerList,
            ClientId = clientId + "_consumer",
            GroupId = ConsumerGroup_MapWords,
            // AutoOffsetReset specifies the action to take when there
            // are no committed offsets for a partition, or an error
            // occurs retrieving offsets. If there are committed offsets,
            // it has no effect.
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // Offsets are committed using the producer as part of the
            // transaction - not the consumer. When using transactions,
            // you must turn off auto commit on the consumer, which is
            // enabled by default!
            EnableAutoCommit = false,
            // Enable incremental rebalancing by using the CooperativeSticky
            // assignor (avoid stop-the-world rebalances).
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky
        };

    }
}