using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClosetAPI.Infrastructure.Persistence;

public static class JsonPropertyExtensions
{
    public static PropertyBuilder<T?> HasJsonConversion<T>(this PropertyBuilder<T?> property) where T : class
    {
        property.HasConversion((ValueConverter)Converter<T>(), (ValueComparer)Comparer<T>());
        return property;
    }

    public static ComplexTypePropertyBuilder<T> HasJsonConversion<T>(this ComplexTypePropertyBuilder<T> property) where T : class
    {
        property.HasConversion(Converter<T>(), Comparer<T>());
        return property;
    }

    private static ValueConverter<T, string> Converter<T>() where T : class => new(
        value => Serialize(value),
        json => JsonSerializer.Deserialize<T>(json, (JsonSerializerOptions?)null)!);

    private static ValueComparer<T> Comparer<T>() where T : class => new(
        (left, right) => Serialize(left) == Serialize(right),
        value => Serialize(value).GetHashCode(),
        value => Clone(value));

    private static string Serialize<T>(T? value) => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null);

    private static T Clone<T>(T value) where T : class =>
        JsonSerializer.Deserialize<T>(Serialize(value), (JsonSerializerOptions?)null)!;
}
