using ClosetAPI.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCloset(builder.Configuration);

var app = builder.Build();
await app.InitializeClosetDatabaseAsync();
app.UseCloset();
app.Run();

public partial class Program;
