using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace Shorthand.Vite;

public static class EndpointRouteBuilderExtensions {
    public static IEndpointRouteBuilder MapViteProxy(this IEndpointRouteBuilder endpoints) {
        var requestConfig = new ForwarderRequestConfig();
        var transformer = new ViteRequestTransformer();
        endpoints.MapForwarder(".vite/{**catch-all}", "http://localhost:3000", requestConfig, transformer);
        endpoints.MapForwarder("@vite/{**catch-all}", "http://localhost:3000", requestConfig, transformer);
        endpoints.MapForwarder("node_modules/{**catch-all}", "http://localhost:3000", requestConfig, transformer);
        endpoints.MapForwarder("assets/{**catch-all}", "http://localhost:3000", requestConfig, transformer);
        return endpoints;
    }
}

internal class ViteRequestTransformer : HttpTransformer {
    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix, CancellationToken cancellationToken) {
        // Copy all request headers
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

        var request = httpContext.Request;
        var options = httpContext.RequestServices.GetRequiredService<IOptions<ViteOptions>>().Value;

        var pathPrefix = options.ProxyPrefix;
        var requestPath = httpContext.Request.Path;
        if(requestPath.StartsWithSegments(pathPrefix, StringComparison.OrdinalIgnoreCase)) {
            var pathString = requestPath.Value!;
            var path = pathString[pathPrefix.Length..];

            requestPath = new PathString(path);
        }

        var protocol = options.Https == true ? "https" : "http";
        var targetUri = new Uri($"{protocol}://{options.Hostname}:{options.Port}{requestPath}?{request.QueryString}");
        proxyRequest.RequestUri = targetUri;

        // Assign the custom uri. Be careful about extra slashes when concatenating here. RequestUtilities.MakeDestinationAddress is a safe default.
        //proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(targetUri, requestPath, request.QueryString);

        proxyRequest.Headers.Host = null;
    }

    public override async ValueTask<bool> TransformResponseAsync(HttpContext httpContext, HttpResponseMessage? proxyResponse, CancellationToken cancellationToken) {
        if(proxyResponse == null) {
            return await base.TransformResponseAsync(httpContext, proxyResponse, cancellationToken);
        }

        var options = httpContext.RequestServices.GetRequiredService<IOptions<ViteOptions>>().Value;
        var pathPrefix = options.ProxyPrefix;

        var requestPath = httpContext.Request.Path;
        if(!requestPath.StartsWithSegments(pathPrefix, StringComparison.OrdinalIgnoreCase)) {
            return await base.TransformResponseAsync(httpContext, proxyResponse, cancellationToken);
        }

        var ms = new MemoryStream();
        await proxyResponse.Content.CopyToAsync(ms, cancellationToken);

        var content = ms.ToArray();
        var contentString = Encoding.UTF8.GetString(content);

        contentString = contentString.Replace(options.ProxyRewriteRoot, options.ProxyPrefix + options.ProxyRewriteRoot, StringComparison.OrdinalIgnoreCase);

        var mediaType = proxyResponse.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        proxyResponse.Content = new StringContent(contentString, Encoding.UTF8, mediaType);

        await base.TransformResponseAsync(httpContext, proxyResponse, cancellationToken);

        return true;
    }
}
