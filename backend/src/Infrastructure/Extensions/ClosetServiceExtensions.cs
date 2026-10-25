namespace ClosetAPI.Infrastructure.Extensions;

public static class ClosetServiceExtensions
{
    public static IServiceCollection AddCloset(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddClosetApi(configuration)
            .AddClosetPersistence(configuration)
            .AddClosetAi(configuration);
}
