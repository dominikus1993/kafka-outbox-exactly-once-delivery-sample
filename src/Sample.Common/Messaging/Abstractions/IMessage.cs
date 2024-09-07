namespace Sample.Common.Messaging.Abstractions;

public interface IMessage
{
    Guid MessageId { get; }
    long UnixTimestamp { get; }
}