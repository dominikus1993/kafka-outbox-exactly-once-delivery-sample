using System.Text.Json.Serialization;
using Sample.Core.Order.Model;

namespace Sample.Core.Events;


[JsonPolymorphic]
[JsonDerivedType(typeof(OrderSaved), OrderSaved.EventName)]
public abstract class OutBoxEvent
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string Name { get; set; } = string.Empty;
}


public sealed class OrderSaved : OutBoxEvent
{
    public const string EventName = "order.saved";
    public Guid OrderId { get; set; }
    public IReadOnlyList<OrderItem> Items { get; set; } = null!;

    public OrderSaved()
    {
        
    }
    
    public OrderSaved(Order.Model.Order order, TimeProvider provider)
    {
        ArgumentNullException.ThrowIfNull(order);
        Id = order.Id;
        CreatedAt = provider.GetUtcNow();
        Name = EventName;
        OrderId = order.Id;
        Items = order.Items;
    }
}