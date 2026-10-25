using Closet.Tests.Support;
using ClosetAPI.Domain.Items;
using ClosetAPI.Domain.Outfits;
using ClosetAPI.Infrastructure.Ai;
using ClosetAPI.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Closet.Tests.Api;

public class StartupConfigurationTests
{
    [Fact]
    public void Without_an_api_key_the_rules_stylist_and_fake_tagger_are_used()
    {
        using var factory = new ApiFactory();

        Assert.IsType<RulesOutfitStylist>(factory.Services.GetRequiredService<IOutfitStylist>());
        Assert.IsType<FakeItemTagger>(factory.Services.GetRequiredService<IItemTagger>());
    }

    [Fact]
    public void With_an_api_key_the_claude_implementations_are_used()
    {
        using var factory = new ApiFactory { Settings = { ["Anthropic:ApiKey"] = "sk-ant-test" } };

        Assert.IsType<ClaudeOutfitStylist>(factory.Services.GetRequiredService<IOutfitStylist>());
        Assert.IsType<ClaudeItemTagger>(factory.Services.GetRequiredService<IItemTagger>());
        Assert.IsType<AnthropicStructuredLlm>(factory.Services.GetRequiredService<IStructuredLlm>());
    }

    [Fact]
    public void A_blank_api_key_counts_as_no_key()
    {
        using var factory = new ApiFactory { Settings = { ["Anthropic:ApiKey"] = "  " } };

        Assert.IsType<RulesOutfitStylist>(factory.Services.GetRequiredService<IOutfitStylist>());
    }

    [Fact]
    public void Sqlite_provider_uses_the_sqlite_context()
    {
        using var factory = new ApiFactory();
        using var scope = factory.Services.CreateScope();

        Assert.IsType<SqliteClosetDbContext>(scope.ServiceProvider.GetRequiredService<ClosetDbContext>());
    }

    [Fact]
    public void An_unknown_provider_fails_fast()
    {
        using var factory = new ApiFactory { Settings = { ["Database:Provider"] = "Oracle" } };

        var exception = Assert.Throws<InvalidOperationException>(() => factory.CreateClient());

        Assert.Contains("Oracle", exception.Message);
    }

    [Fact]
    public void Sql_server_provider_selects_the_sql_server_context_without_connecting()
    {
        using var factory = new ApiFactory
        {
            EnvironmentName = "Production",
            Settings =
            {
                ["Database:Provider"] = "SqlServer",
                ["Database:MigrateOnStartup"] = "false",
                ["ConnectionStrings:Closet"] = "Server=localhost;Database=x;User Id=sa;Password=x;TrustServerCertificate=True",
            },
        };
        using var scope = factory.Services.CreateScope();

        Assert.IsType<SqlServerClosetDbContext>(scope.ServiceProvider.GetRequiredService<ClosetDbContext>());
    }
}
