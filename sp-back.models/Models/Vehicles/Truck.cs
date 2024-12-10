using sp_back.models.Exceptions;

namespace sp_back.models.Models.Vehicles;

public class Truck : Vehicle
{
    public TruckInfo GetTruckInfo()
    {
        return new TruckInfo()
        {
            NumberOfSeats = NumberOfSeats,
            Make = Make,
            Model = Model,
            ProductionDate = ProductionDate.ToString()
        };
    }
}

public class TruckInfo
{
    public uint NumberOfSeats { get; set; }
    public string Make { get; set;}
    public string Model { get; set;}
    public string ProductionDate { get; set;}
}