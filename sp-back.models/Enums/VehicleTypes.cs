using System.Text.Json.Serialization;

namespace sp_back.models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VehicleType
{
    Hatchback,
    Sedan,
    Suv,
    Truck
}