namespace ClosetAPI.Domain.Persistence;

public static class EntityId
{
    public static string New() => Guid.NewGuid().ToString("N");
}
