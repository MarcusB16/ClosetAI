namespace Closet.Core.Domain;

// These mirror the app's src/domain/types.ts. The API serializes enums in kebab-case
// ("outer", "needs-photo") and property names in camelCase, so the JSON matches the app.

public enum Slot { Outer, Top, Bottom, Shoes, Acc }

public enum Category { Shirts, Pants, Jackets, Shoes, Accessories, Formal }

public class Item
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public Category Category { get; set; }
    public Slot Slot { get; set; }
    /// <summary>Cutout photo. When missing, the app draws the garment from Type and Color.</summary>
    public string? ImageUri { get; set; }
    /// <summary>Which drawing the app uses without a photo: tee, shirt, sneaker, watch, ...</summary>
    public string Type { get; set; } = "";
    public Dictionary<string, string>? Variant { get; set; }
    public string Color { get; set; } = "";
    public string ColorName { get; set; } = "";
    /// <summary>1 = casual, 2 = smart, 3 = formal.</summary>
    public double Formality { get; set; }
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
}

/// <summary>What the app sends to create an item. Wear stats are set by the server.</summary>
public record NewItem(
    string Name,
    string Brand,
    Category Category,
    Slot Slot,
    string Type,
    string Color,
    string ColorName,
    double Formality,
    string Fabric,
    string Size,
    string? ImageUri = null,
    Dictionary<string, string>? Variant = null,
    decimal? Price = null,
    List<string>? Keywords = null,
    bool? RainOk = null,
    bool? NoRain = null,
    List<string>? Seasons = null);

public record OutfitPieces(string? Outer, string Top, string Bottom, string Shoes, List<string> Acc)
{
    public IEnumerable<string> AllIds() =>
        new[] { Outer, Top, Bottom, Shoes }.Concat(Acc).Where(id => !string.IsNullOrEmpty(id))!;
}

public class Outfit
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Prompt { get; set; }
    public string? Note { get; set; }
    public OutfitPieces Pieces { get; set; } = new(null, "", "", "", []);
    public bool Saved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class WearEntry
{
    public DateOnly Date { get; set; }
    public string OutfitId { get; set; } = "";
}

public record WornOutfit(DateOnly Date, string OutfitId, Outfit Outfit);

public record StyleContext(DateOnly Date, string Weekday, int TempF, string Forecast, bool Rain);

public record StyleRequest(string Prompt, string? LockItemId = null, bool? AvoidRecent = null);

/// <summary>What the tagging call returns for a new photo. The user confirms it before saving.</summary>
public record ItemSuggestion(
    string Name,
    string? Brand,
    Category Category,
    string ColorName,
    string Color,
    double Formality,
    List<string> Seasons,
    string? Fabric,
    string CutoutUri,
    Slot? Slot = null,
    string? Type = null);

/// <summary>Set once during first-launch setup; used for every try-on.</summary>
public record Profile(bool Onboarded, string? PersonPhotoUri);

public enum TryOnStatus { Ready, NeedsPhoto, Failed }

public record TryOnResult(TryOnStatus Status, string? ImageUri = null, bool? Placeholder = null, string? Message = null);

/// <summary>Per-user settings and bookkeeping that aren't items, outfits or wears.</summary>
public class UserState
{
    public bool Onboarded { get; set; }
    public string? PersonPhotoUri { get; set; }
    public DateOnly? TodayOutfitDate { get; set; }
    public string? TodayOutfitId { get; set; }

    public Profile ToProfile() => new(Onboarded, PersonPhotoUri);
}

/// <summary>A saved try-on image. Reused until the outfit's pieces or the person photo change.</summary>
public class TryOnRender
{
    public string OutfitId { get; set; } = "";
    public string PiecesKey { get; set; } = "";
    public string PersonPhotoUri { get; set; } = "";
    public string ImageUri { get; set; } = "";
    public bool Placeholder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
