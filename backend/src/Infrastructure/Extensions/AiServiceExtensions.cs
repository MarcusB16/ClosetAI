using Anthropic;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Domain.TryOns;
using ClosetAPI.Infrastructure.Ai;

namespace ClosetAPI.Infrastructure.Extensions;

public static class AiServiceExtensions
{
    public static IServiceCollection AddClosetAi(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(AnthropicOptions.SectionName).Get<AnthropicOptions>() ?? new AnthropicOptions();
        services.Configure<AnthropicOptions>(configuration.GetSection(AnthropicOptions.SectionName));

        if (options.HasApiKey)
        {
            services.AddSingleton(new AnthropicClient { ApiKey = options.ApiKey });
            services.AddSingleton<IStructuredLlm, AnthropicStructuredLlm>();
            services.AddSingleton<IOutfitStylist, ClaudeOutfitStylist>();
            services.AddSingleton<IItemTagger, ClaudeItemTagger>();
        }
        else
        {
            services.AddSingleton<IOutfitStylist, RulesOutfitStylist>();
            services.AddSingleton<IItemTagger, FakeItemTagger>();
        }

        services.AddSingleton<IBackgroundRemover, PassThroughBackgroundRemover>();
        services.AddSingleton<ITryOnRenderer, PlaceholderTryOnRenderer>();
        return services;
    }
}
