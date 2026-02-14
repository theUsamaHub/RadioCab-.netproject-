namespace RadioCab.Models;

public class DriverServiceVM
{
    public List<DriverService> DriverServices { get; set; } = new();
    public DriverServiceValidate DriverSerVal { get; set; } = new();

    public List<Service> AvailableServices { get; set; } = new();
}
