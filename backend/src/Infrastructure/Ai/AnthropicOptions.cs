namespace ClosetAPI.Infrastructure.Ai;

public class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public string? ApiKey { get; set; }
    public string Model { get; set; } = "claude-sonnet-5-5";

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}
