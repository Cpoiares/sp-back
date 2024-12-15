using sp_back.models.Enums;

namespace sp_back.models.DTOs.Requests;

public class VehicleSearchParams
{
    public VehicleSearchParams(string? model, string? manufacturer, VehicleType? type, int? yearFrom, int? yearTo, bool? hideNonAvailable, bool? hideSold, bool? showDeleted)
    {
        Model = model;
        Manufacturer = manufacturer;
        Type = type;
        YearFrom = yearFrom;
        YearTo = yearTo;
        HideNonAvailable = hideNonAvailable;
        HideSold = hideSold;
        ShowDeleted = showDeleted;
    }

    public string GetAsString()
    {
        return $"Model: {Model} - Manufacturer: {Manufacturer} - Type: {Type} - YearFrom: {YearFrom} - YearTo: {YearTo} - HideNonAvailable: {HideNonAvailable} - HideSold: {HideSold} - ShowDeleted: {ShowDeleted}";
    }

    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public VehicleType? Type { get; set; }
    public int? YearFrom { get; set; }
    public int? YearTo { get; set; }
    public bool? HideNonAvailable { get; set; }
    public bool? HideSold { get; set; }
    public bool? ShowDeleted { get; set; }
}