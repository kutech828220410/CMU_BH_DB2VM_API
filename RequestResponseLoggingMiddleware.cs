using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Log connection info
        var connectionInfo = $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";
        _logger.LogInformation($"Connection Info: {connectionInfo}");

        // Log headers
        foreach (var header in context.Request.Headers)
        {
            _logger.LogInformation($"Header: {header.Key} = {header.Value}");
        }

        // Log the request body
        context.Request.EnableBuffering();
        string requestBody = string.Empty;
        try
        {
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }
        }
        catch (DecoderFallbackException ex)
        {
            _logger.LogError(ex, "Request body contains invalid characters and could not be decoded.");
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Invalid request encoding.");
            return;
        }

        _logger.LogInformation($"Incoming Request: {context.Request.Method} {context.Request.Path} {requestBody}");

        await _next(context);
    }
}
