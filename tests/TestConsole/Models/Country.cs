namespace TestConsole.Models;

public class Country : Entity
{
    public string Name { get; set; }
    public bool IsIndependent { get; set; }
    public decimal Population { get; set; }
    public List<City> Cities { get; set; } = [];
}