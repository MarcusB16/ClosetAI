namespace ClosetAPI.Domain.Items;

public class Item
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public Category Category { get; set; }
    public Slot Slot { get; set; }
    public string? ImageUri { get; set; }
    public string Type { get; set; } = "";
    public Dictionary<string, string>? Variant { get; set; }
    public string Color { get; set; } = "";
    public string ColorName { get; set; } = "";
    public int Formality { get; set; }
    public string Fabric { get; set; } = "";
    public string Size { get; set; } = "";
    public decimal? Price { get; set; }
    public int TimesWorn { get; set; }
    public DateOnly? LastWorn { get; set; }
    public List<string>? Keywords { get; set; }
    public bool? RainOk { get; set; }
    public bool? NoRain { get; set; }
    public List<string>? Seasons { get; set; }
    public DateTimeOffset AddedAt { get; set; }

    public bool IsWatch => Type == GarmentTypes.Watch;
}
