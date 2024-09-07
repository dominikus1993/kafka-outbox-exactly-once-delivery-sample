using System.Text.Json.Serialization;
using Sample.Core.Events;

namespace Sample.Infrastructure.Orders.Serialization;

[JsonSerializable(typeof(OutBoxEvent))]
[JsonSerializable(typeof(OrderSaved))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class OutBoxSerializationConfig : JsonSerializerContext;