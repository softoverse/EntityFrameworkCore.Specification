namespace TestConsole;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsCapital { get; set; }
    public Country Country { get; set; }
}

public class Country
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsIndependent { get; set; }
    public decimal Population { get; set; }
}