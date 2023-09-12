using Microsoft.AspNetCore.Builder;
using Shorthand.Vite.Middlewares;

namespace Shorthand.Vite;

public static class IApplicationBuilderExtensions {
    public static void UseViteDevServerProxy(this IApplicationBuilder app) {
        app.UseMiddleware<ViteDevServerProxyMiddleware>();
    }
}
