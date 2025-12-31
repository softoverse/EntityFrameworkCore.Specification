using System.ComponentModel.DataAnnotations.Schema;

namespace TestConsole.Models;

public abstract class Entity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public Guid RowId { get; set; } = Guid.CreateVersion7(); // For Audit Logs
}