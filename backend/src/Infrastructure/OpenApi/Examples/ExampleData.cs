using ClosetAPI.Application.Dtos.Shared;
using ClosetAPI.Domain.Items;

namespace ClosetAPI.Infrastructure.OpenApi.Examples;

public static class ExampleData
{
    public static readonly DateTimeOffset Moment = new(2026, 10, 26, 8, 30, 0, TimeSpan.FromHours(-4));

    public static ItemResponse Sneakers => new()
    {
        Id = "s1",
        Name = "White leather sneakers",
        Brand = "Common Projects",
        Category = Category.Shoes,
        Slot = Slot.Shoes,
        Type = "sneaker",
        Color = "#F2F1EC",
        ColorName = "White",
        Formality = 1,
        Fabric = "Italian calf leather",
        Size = "10",
        TimesWorn = 46,
        AddedAt = Moment,
        LastWorn = new DateOnly(2026, 10, 14),
        Price = 425,
        Keywords = ["white sneakers", "sneakers"],
    };

    public static OutfitResponse Outfit => new()
    {
        Id = "5f0c2a9e8b7d4c1fa3e6d2b09c4a1e77",
        Title = "Easy casual",
        Prompt = "Casual coffee with a friend",
        Note = "Easy pieces that still go together. It's 57° today, so bring the black bomber.",
        Saved = false,
        CreatedAt = Moment,
        Pieces = new OutfitPiecesResponse
        {
            Outer = "o4",
            Top = "t3",
            Bottom = "b1",
            Shoes = "s1",
            Acc = ["a1", "a6"],
        },
    };
}
