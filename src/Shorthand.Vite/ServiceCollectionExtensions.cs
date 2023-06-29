using Microsoft.Extensions.DependencyInjection;
using Shorthand.Vite.Contracts;

namespace Shorthand.Vite;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddVite(this IServiceCollection services, Action<ViteOptions>? configureOptions = null) {
        services.AddOptions<ViteOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddSingleton<IVite, Services.Vite>();

        return services;
    }
}
