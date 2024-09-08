using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Common.Messaging.Abstractions;
using Sample.Infrastructure.Messaging.Kafka.Configuration;
using Sample.Infrastructure.Messaging.Kafka.Publisher;
using Sample.Infrastructure.Messaging.Kafka.Serialization;

namespace Sample.Infrastructure.Messaging.Kafka.Extensions;

[Serializable]
public sealed class NoProducerConfigurationException : Exception
{
    public NoProducerConfigurationException() : base("Kafka::ProducerConfig should be defined")
    {
    }

    public NoProducerConfigurationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

public interface IKafkaProducerBuilder
{
    public IServiceCollection Services { get; }
    public bool IsEnabled { get; }
}

internal sealed class KafkaProducerBuilder : IKafkaProducerBuilder
{
    public KafkaProducerBuilder(IServiceCollection services, bool isEnabled)
    {
        Services = services;
        IsEnabled = isEnabled;
    }

    public IServiceCollection Services { get; }
    public bool IsEnabled { get; }
}

public sealed class KafkaPublisherOptions<T> where T : class, IMessage
{
    public Func<T, string>? KeySelector { get; set; }
    internal IKafkaMessageSerializer<T> Serializer { get; set; } = new KafkaSystemTextJsonMessageSerializer<T>();
    
    public void WithKeySelector(Func<T, string> keySelector)
    {
        KeySelector = keySelector;
    }
    
    public void EnableSystemTextJson(JsonSerializerOptions? options = null)
    {
        Serializer = new KafkaSystemTextJsonMessageSerializer<T>(options);
    }
}

public static class KafkaServicesCollectionExtensions
{
    public static IKafkaProducerBuilder AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
    {
        var cfg = configuration.GetRequiredSection("Kafka").Get<KafkaConfiguration>()!;
        if (cfg.Enabled)
        {
            if (cfg is { ProducerConfig: null })
            {
                throw new NoProducerConfigurationException();
            }

            services.AddSingleton<ProducerConfig>(cfg.ProducerConfig);
            services.AddSingleton<KafkaClientHandle>(sp =>
            {
                var config = sp.GetRequiredService<ProducerConfig>();
                return new KafkaClientHandle(config);
            });
            services.AddSingleton<KafkaDependentProducer<Null, byte[]>>();
            services.AddSingleton<KafkaDependentProducer<string, byte[]>>();
            services.AddTransient<IMessagePublisher, KafkaMessagePublisher>();
            return new KafkaProducerBuilder(services, true);
        }
        services.AddTransient<IMessagePublisher, NoopMessagePublisher>();
        return new KafkaProducerBuilder(services, false);
    }
    
    
    public static IKafkaProducerBuilder AddPublisher<T>(this IKafkaProducerBuilder builder, string? topic, Action<KafkaPublisherOptions<T>>? setup = null) where T : class, IMessage
    {
        if (string.IsNullOrEmpty(topic))
        {
            throw new ArgumentNullException(nameof(topic), "Topic should be neither null or empty");
        }
        
        var options = new KafkaPublisherOptions<T>();
        setup?.Invoke(options);

        builder.Services.AddSingleton(options.Serializer);
        builder.Services.AddSingleton(new KafkaMessagePublisherConfig<T>(topic, options.KeySelector));

        return builder;
    }
}