namespace ClosetAPI.Domain.Errors;

public class RuleViolationException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
