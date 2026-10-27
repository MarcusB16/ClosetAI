using ClosetAPI.Infrastructure.Media;
using Microsoft.Extensions.FileProviders;

namespace ClosetAPI.Infrastructure.Extensions;

public static class ClosetPipelineExtensions
{
    public static WebApplication UseCloset(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();

        var mediaStore = (LocalMediaStore)app.Services.GetRequiredService<ClosetAPI.Domain.Media.IMediaStore>();
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(mediaStore.Folder),
            RequestPath = MediaOptions.RequestPath,
        });

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "ClosetAI API v1");
                options.RoutePrefix = "swagger";
            });
        }

        app.MapControllers();
        return app;
    }
}
