using System.Net;

namespace Closet.Tests.Support;

public sealed class StubServer : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly Task _loop;

    private StubServer(int status, string body)
    {
        var port = FreePort();
        Url = $"http://127.0.0.1:{port}";
        _listener.Prefixes.Add($"{Url}/");
        _listener.Start();
        _loop = Task.Run(async () =>
        {
            while (_listener.IsListening)
            {
                HttpListenerContext context;
                try { context = await _listener.GetContextAsync(); }
                catch (Exception) { return; }

                using var reader = new StreamReader(context.Request.InputStream);
                LastRequestBody = await reader.ReadToEndAsync();
                var bytes = System.Text.Encoding.UTF8.GetBytes(body);
                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }
        });
    }

    public string Url { get; }
    public string LastRequestBody { get; private set; } = "";

    public static StubServer Respond(int status, string body) => new(status, body);

    public void Dispose()
    {
        _listener.Stop();
        _listener.Close();
        _loop.Wait(TimeSpan.FromSeconds(2));
    }

    private static int FreePort()
    {
        var probe = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        var port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        return port;
    }
}
