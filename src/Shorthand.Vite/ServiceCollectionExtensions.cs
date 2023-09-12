using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shorthand.Vite.Contracts;
using Shorthand.Vite.Services;

namespace Shorthand.Vite;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddVite(this IServiceCollection services, Action<ViteOptions>? configureOptions = null) {
        services.AddOptions<ViteOptions>()
            .Configure(configureOptions ?? (_ => { }));

        services.AddScoped<IViteService, ViteService>();
        services.AddSingleton<IEnvironmentVariableProvider, EnvironmentVariableProvider>();

        services.AddHttpForwarder();
        services.AddHttpClient("Shorthand.Vite.HttpClient", (serviceProvider, client) => {
            var options = serviceProvider.GetRequiredService<IOptions<ViteOptions>>().Value;
            client.BaseAddress = new Uri(options.ViteDevServerUrl);
        });

        return services;
    }
}
