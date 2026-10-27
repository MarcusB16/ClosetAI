using System.Reflection;
using ClosetAPI.Domain.Outfits;
using Microsoft.EntityFrameworkCore;

namespace Closet.Tests.Architecture;

public class LayeringTests
{
    private const BindingFlags AllMembers =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly Type[] ApiTypes = typeof(OutfitStudio).Assembly.GetTypes();

    private static IEnumerable<Type> TypesIn(string namespacePrefix) =>
        ApiTypes.Where(type => type.Namespace?.StartsWith(namespacePrefix, StringComparison.Ordinal) == true);

    [Fact]
    public void Only_repositories_and_infrastructure_depend_on_the_db_context()
    {
        var offenders = TypesIn("ClosetAPI.Application")
            .Concat(TypesIn("ClosetAPI.Domain"))
            .Where(type => ReferencedTypes(type).Any(referenced => typeof(DbContext).IsAssignableFrom(referenced)))
            .Select(type => type.FullName);

        Assert.Empty(offenders);
    }

    [Fact]
    public void Domain_does_not_depend_on_application_or_infrastructure()
    {
        var offenders = TypesIn("ClosetAPI.Domain")
            .SelectMany(type => ReferencedTypes(type)
                .Where(IsOutsideDomainLayer)
                .Select(referenced => $"{type.FullName} -> {referenced.FullName}"));

        Assert.Empty(offenders);
    }

    private static bool IsOutsideDomainLayer(Type type) =>
        type.Namespace?.StartsWith("ClosetAPI.Application", StringComparison.Ordinal) == true
        || type.Namespace?.StartsWith("ClosetAPI.Infrastructure", StringComparison.Ordinal) == true;

    private static IEnumerable<Type> ReferencedTypes(Type type)
    {
        var direct = type.GetConstructors(AllMembers).SelectMany(constructor => constructor.GetParameters()).Select(parameter => parameter.ParameterType)
            .Concat(type.GetFields(AllMembers).Select(field => field.FieldType))
            .Concat(type.GetProperties(AllMembers).Select(property => property.PropertyType))
            .Concat(type.GetMethods(AllMembers).SelectMany(method =>
                method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType)));

        return direct.SelectMany(Unwrap).Distinct();
    }

    private static IEnumerable<Type> Unwrap(Type type)
    {
        if (type.HasElementType) return Unwrap(type.GetElementType()!);
        if (!type.IsGenericType) return [type];
        return type.GetGenericArguments().SelectMany(Unwrap).Prepend(type);
    }
}
