using Microsoft.Extensions.DependencyInjection;
using Shorthand.Vite.Contracts;
using Shorthand.Vite.Services;

namespace Shorthand.Vite;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddVite(this IServiceCollection services, Action<ViteOptions>? configureOptions = null) {
        services.AddOptions<ViteOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddScoped<IViteService, ViteService>();
        services.AddSingleton<IFileSystemProvider, FileSystemProvider>();

        return services;
    }
}
