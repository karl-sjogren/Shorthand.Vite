using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace Shorthand.Vite.Middlewares;

public class ViteDevServerProxyMiddleware {
    private readonly RequestDelegate _next;

    public ViteDevServerProxyMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory, IHttpForwarder forwarder) {
        var modules = await GetModulesAsync(context, httpClientFactory);

        var request = context.Request;

        var isHmrRequest = request.Path.StartsWithSegments("/.vite/hmr", StringComparison.OrdinalIgnoreCase);

        var isModuleRequest = request.Query.Any(query => query.Key == "vite-entry-module" && query.Value == "true")
            || request.Path.StartsWithSegments("/@vite", StringComparison.OrdinalIgnoreCase)
            || modules.Any(module => request.Path.StartsWithSegments(module, StringComparison.OrdinalIgnoreCase));

        if(!isHmrRequest && !isModuleRequest) {
            await _next(context);
            return;
        }

        var options = context.RequestServices.GetRequiredService<IOptions<ViteOptions>>().Value;

        var httpClient = new HttpMessageInvoker(new SocketsHttpHandler() {
            UseProxy = false,
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
            ConnectTimeout = TimeSpan.FromSeconds(15)
        });

        await forwarder.SendAsync(context, options.ViteDevServerUrl, httpClient);
    }

    private static async Task<string[]> GetModulesAsync(HttpContext context, IHttpClientFactory httpClientFactory) {
        var httpClient = httpClientFactory.CreateClient("Shorthand.Vite.HttpClient");

        var responseJson = await httpClient.GetStringAsync(".shorthand-vite/modules", context.RequestAborted);

        var modules = JsonSerializer.Deserialize<string[]>(responseJson) ?? Array.Empty<string>();

        return modules
            .Where(module => !string.IsNullOrWhiteSpace(module) && module.StartsWith("/", StringComparison.Ordinal))
            .ToArray();
    }
}
