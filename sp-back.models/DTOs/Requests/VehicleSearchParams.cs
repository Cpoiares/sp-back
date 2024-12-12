using sp_back.models.Enums;

namespace sp_back.models.Models;

public class VehicleSearchParams
{
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public VehicleType? Type { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
}