using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sample.Common.Messaging.Abstractions;
using Sample.Infrastructure.Messaging.Kafka.Configuration;

namespace Sample.Infrastructure.Messaging.Kafka.Consumer;

public sealed class KafkaEventConsumer<T> : BackgroundService where T: class, IMessage
{
    private string TypeConsumerName = typeof(T).Name;
    private readonly ConsumerConfig _consumerConfig;
    private KafkaConsumerConfig<T> _kafkaConsumerConfig;
    private IConsumer<Ignore, byte[]>? _consumer;
    private KafkaMessageProcessor<T> _messageProcessor;
    private ILogger<KafkaEventConsumer<T>> _logger;
    private IHostApplicationLifetime _application;
    public KafkaEventConsumer(ConsumerConfig consumerConfig, KafkaConsumerConfig<T> kafkaConsumerConfig, KafkaMessageProcessor<T> messageProcessor, ILogger<KafkaEventConsumer<T>> logger, IHostApplicationLifetime application)
    {
        _consumerConfig = consumerConfig;
        _kafkaConsumerConfig = kafkaConsumerConfig;
        _messageProcessor = messageProcessor;
        _logger = logger;
        _application = application;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Consume(stoppingToken), stoppingToken);
    }
    
    private async Task Consume(CancellationToken stoppingToken)
    {
        var clientId = _kafkaConsumerConfig.ClientId;
        _consumerConfig.ClientId = clientId + "_consumer";
        _consumer = new ConsumerBuilder<Ignore, byte[]>(_consumerConfig).Build();
        
        
        _consumer.Subscribe(_kafkaConsumerConfig.Topic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                var cr = _consumer.Consume(stoppingToken);
                var result = await _messageProcessor.Process(cr, stoppingToken);
                if (result.IsSuccess)
                {
                    
                }
                _consumer.StoreOffset(cr);
                
                if (cr.IsPartitionEOF)
                {
                    // _logger.LogPartitionEOF(cr.Topic, cr.Partition.Value, cr.Offset.Value);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e) when (e.Error.IsFatal)
            {
                // _logger.LogFatalError(e, _topic.Name);
                _application.StopApplication();
                break;
            }
            catch (ConsumeException e)
            {
                    
                if (e.Error.IsFatal)
                {
                    // _logger.LogFatalError(e, _topic.Name);
                    _application.StopApplication();
                    break;

                }
                // _logger.LogConsumeError(e, _topic.Name);
            }
            catch (Exception e)
            {
                // _logger.LogUnexpectedError(e, _topic.Name);
            }
        }

    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}