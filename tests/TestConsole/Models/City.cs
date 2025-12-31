using System.ComponentModel.DataAnnotations.Schema;

namespace TestConsole.Models;

public class City : Entity
{
    public string Name { get; set; }
    public bool IsCapital { get; set; }
    
    [ForeignKey(nameof(CountryId))]
    public long CountryId { get; set; }
    public Country? Country { get; set; }
}