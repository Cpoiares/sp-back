namespace sp_back.models.DTOs.Requests;

public record CreateVehicleRequest
{
    public CreateVehicleRequest(string manufacturer, string model, DateTime productionDate, double startingPrice, string vin)
    {
        Manufacturer = manufacturer;
        Model = model;
        ProductionDate = productionDate;
        StartingPrice = startingPrice;
        Vin = vin;
    }

    public string Manufacturer { get; set;}
    public string Model { get; set;}
    public DateTime ProductionDate { get; set;}
    public double StartingPrice { get; set;}
    public string Vin { get; set;}
}
