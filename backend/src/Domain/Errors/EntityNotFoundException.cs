namespace ClosetAPI.Domain.Errors;

public class EntityNotFoundException(string entityName, string id)
    : Exception($"{entityName} '{id}' was not found.");
