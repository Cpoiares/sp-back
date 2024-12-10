using sp_back.models.Enums;

namespace sp_back.models.Models;

public class VehicleSearchParams
{
    public string? Make { get; set; }
    public string? Model { get; set; }
    public VehicleType? Type { get; set; }
    public double? LoadCapacity { get; set; }
    public int? NumberOfSeats { get; set; }
    public int? NumberOfDoors { get; set; }
    public int? Year { get; set; }
    public decimal? StartingPrice { get; set; }
    public decimal? EndingPrice { get; set; }
    public bool? IsAvailable { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}