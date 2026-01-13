using System.ComponentModel.DataAnnotations.Schema;

namespace TestConsole.Models;

public class District : Entity
{
    public string Name { get; set; }
    public int Population { get; set; }
    
    [ForeignKey(nameof(CityId))]
    public long CityId { get; set; }
    public City? City { get; set; }
}

