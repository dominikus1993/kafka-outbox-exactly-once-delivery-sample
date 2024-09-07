namespace Sample.Common.Messaging.Abstractions;

public interface IMessage
{
    Guid MessageId { get; init; }
    long SendAt { get; init; }
}