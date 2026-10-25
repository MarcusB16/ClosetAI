using ClosetAPI.Application.Dtos.Items.CreateItem;
using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Application.Mapping;

public static class ItemMapping
{
    public static ItemResponse ToResponse(this Item item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Brand = item.Brand,
        Category = item.Category,
        Slot = item.Slot,
        Type = item.Type,
        Color = item.Color,
        ColorName = item.ColorName,
        Formality = item.Formality,
        Fabric = item.Fabric,
        Size = item.Size,
        TimesWorn = item.TimesWorn,
        AddedAt = item.AddedAt,
        ImageUri = item.ImageUri,
        Variant = item.Variant,
        Price = item.Price,
        LastWorn = item.LastWorn,
        Keywords = item.Keywords,
        RainOk = item.RainOk,
        NoRain = item.NoRain,
        Seasons = item.Seasons,
    };

    public static Item ToEntity(this NewItem newItem, string id, DateTimeOffset addedAt) => new()
    {
        Id = id,
        Name = newItem.Name,
        Brand = newItem.Brand,
        Category = newItem.Category,
        Slot = newItem.Slot,
        Type = newItem.Type,
        Color = newItem.Color,
        ColorName = newItem.ColorName,
        Formality = newItem.Formality,
        Fabric = newItem.Fabric,
        Size = newItem.Size,
        ImageUri = newItem.ImageUri,
        Variant = newItem.Variant,
        Price = newItem.Price,
        Keywords = newItem.Keywords,
        RainOk = newItem.RainOk,
        NoRain = newItem.NoRain,
        Seasons = newItem.Seasons,
        TimesWorn = 0,
        AddedAt = addedAt,
    };
}
