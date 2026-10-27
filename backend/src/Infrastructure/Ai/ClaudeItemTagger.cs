using System.Text.Json;
using System.Text.RegularExpressions;
using ClosetAPI.Domain.Errors;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Media;

namespace ClosetAPI.Infrastructure.Ai;

public partial class ClaudeItemTagger(IStructuredLlm llm) : IItemTagger
{
    private const string SystemPrompt =
        "You tag clothing photos for a wardrobe app. Describe the single garment or accessory in the photo. " +
        "Give a short product-style name, the brand only if it is visibly readable (otherwise an empty string), " +
        "the dominant colour as a name and a #RRGGBB hex value, a formality of 1 (casual), 2 (smart) or 3 (formal), " +
        "the seasons it suits, and the likely fabric (empty string if unknown).";

    public async Task<ItemTags> TagPhotoAsync(PhotoContent photo, CancellationToken cancellationToken)
    {
        var request = new StructuredRequest(
            SystemPrompt,
            "Tag the item in this photo.",
            ItemSuggestionSchema.Build(),
            photo);
        var json = await llm.CompleteWithJsonSchemaAsync(request, cancellationToken);
        return ReadTags(json);
    }

    private static ItemTags ReadTags(JsonElement json)
    {
        var color = json.RequiredString("color");
        if (!HexColorPattern().IsMatch(color))
            throw new AiProviderException($"The AI response used an invalid colour '{color}'.");

        var type = json.RequiredString("type");
        if (!GarmentTypes.IsKnown(type))
            throw new AiProviderException($"The AI response used an unknown garment type '{type}'.");

        var formality = json.TryGetProperty("formality", out var value) && value.TryGetInt32(out var number) ? number : 0;
        if (!Formality.IsValid(formality))
            throw new AiProviderException("The AI response used an invalid formality.");

        return new ItemTags(
            Name: json.RequiredString("name"),
            Brand: NullIfEmpty(json.OptionalString("brand")),
            Category: ReadEnum<Category>(json, "category"),
            Slot: ReadEnum<Slot>(json, "slot"),
            Type: type,
            ColorName: json.RequiredString("colorName"),
            Color: color,
            Formality: formality,
            Seasons: ReadSeasons(json),
            Fabric: NullIfEmpty(json.OptionalString("fabric")));
    }

    private static TEnum ReadEnum<TEnum>(JsonElement json, string property) where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(json.RequiredString(property), ignoreCase: true, out var parsed)
            ? parsed
            : throw new AiProviderException($"The AI response used an invalid '{property}'.");

    private static List<string> ReadSeasons(JsonElement json) =>
        json.TryGetProperty("seasons", out var array) && array.ValueKind == JsonValueKind.Array
            ? array.EnumerateArray().Select(season => season.GetString()).OfType<string>().ToList()
            : [];

    private static string? NullIfEmpty(string? text) => string.IsNullOrWhiteSpace(text) ? null : text;

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexColorPattern();
}
