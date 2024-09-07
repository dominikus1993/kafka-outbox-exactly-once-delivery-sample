using System.Text.Json;
using Sample.Core.Events;
using Sample.Infrastructure.Orders.Serialization;

namespace Sample.Infrastructure.Orders.Model;

public sealed class OutBox
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public long CreatedAtTimestamp { get; set; }
    public long? ProcessedAtTimestamp { get; set; }

    public OutBox()
    {
        
    }

    public OutBox(OutBoxEvent boxEvent, TimeProvider provider)
    {
        Type = boxEvent.GetType().FullName ?? string.Empty;
        Name = boxEvent.Name;
        Data = JsonSerializer.Serialize(boxEvent, OutBoxSerializationConfig.Default.OutBoxEvent);
        CreatedAtTimestamp = provider.GetUtcNow().ToUnixTimeMilliseconds();
        Id = boxEvent.Id;
    }
}