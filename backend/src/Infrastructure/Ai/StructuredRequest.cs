using System.Text.Json.Nodes;
using ClosetAPI.Domain.Media;

namespace ClosetAPI.Infrastructure.Ai;

public record StructuredRequest(string SystemPrompt, string UserText, JsonObject Schema, PhotoContent? Image = null);
