using ClosetAPI.Domain.Context;
using ClosetAPI.Domain.Media;
using ClosetAPI.Infrastructure.Context;
using ClosetAPI.Infrastructure.Http;
using ClosetAPI.Infrastructure.Media;
using ClosetAPI.Infrastructure.OpenApi;
using ClosetAPI.Infrastructure.Styling;

namespace ClosetAPI.Infrastructure.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddClosetApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers().AddJsonOptions(json => ApiJson.Configure(json.JsonSerializerOptions));
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddHttpContextAccessor();
        services.AddSingleton(TimeProvider.System);

        services.Configure<MediaOptions>(configuration.GetSection(MediaOptions.SectionName));
        services.AddSingleton<IMediaStore, LocalMediaStore>();
        services.AddScoped<MediaUrlResolver>();

        services.AddSingleton<IStyleContextProvider, SeededStyleContextProvider>();
        services.AddScoped<OutfitStudio>();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<ClosetDocumentTransformer>();
            options.AddOperationTransformer<ExampleOperationTransformer>();
        });
        return services;
    }
}
