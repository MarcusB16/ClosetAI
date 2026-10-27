using System.ComponentModel.DataAnnotations;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Application.Dtos.Items.CreateItem;

public record NewItem
{
    [MinLength(1)]
    public required string Name { get; init; }
    public required string Brand { get; init; }
    public required Category Category { get; init; }
    public required Slot Slot { get; init; }
    [GarmentType]
    public required string Type { get; init; }
    [MinLength(1)]
    public required string Color { get; init; }
    [MinLength(1)]
    public required string ColorName { get; init; }
    [Range(Domain.Items.Formality.Casual, Domain.Items.Formality.Formal)]
    public required int Formality { get; init; }
    public required string Fabric { get; init; }
    public required string Size { get; init; }
    public string? ImageUri { get; init; }
    public Dictionary<string, string>? Variant { get; init; }
    public decimal? Price { get; init; }
    public List<string>? Keywords { get; init; }
    public bool? RainOk { get; init; }
    public bool? NoRain { get; init; }
    public List<string>? Seasons { get; init; }
}
