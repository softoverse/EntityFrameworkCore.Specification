namespace Benchmark.Models;

public class Article : Entity
{
    public string TitleEn { get; set; }
    public string TitleBn { get; set; }
    public string TitleAr { get; set; }
    public string TitleHi { get; set; }

    public string ShortDescriptionEn { get; set; }
    public string ShortDescriptionBn { get; set; }
    public string ShortDescriptionAr { get; set; }
    public string ShortDescriptionHi { get; set; }

    public string DescriptionEn { get; set; }
    public string DescriptionBn { get; set; }
    public string DescriptionAr { get; set; }
    public string DescriptionHi { get; set; }

    public string ImageUrl { get; set; }
}

public abstract class Entity
{
    public long Id { get; set; }
    public Guid RowId { get; set; } = Guid.CreateVersion7(); // For Audit Logs
}