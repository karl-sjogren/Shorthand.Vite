using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shorthand.Vite.Contracts;
using Shorthand.Vite.Exceptions;

using ManifestFileType = System.Collections.Generic.Dictionary<string, Shorthand.Vite.Services.ViteManifestEntry>;

namespace Shorthand.Vite.Services;

public class ViteService : IViteService {
    private readonly IOptionsSnapshot<ViteOptions> _options;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IEnvironmentVariableProvider _environmentVariableProvider;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ViteService> _logger;

    public ViteService(
            IOptionsSnapshot<ViteOptions> options,
            IWebHostEnvironment webHostEnvironment,
            IEnvironmentVariableProvider environmentVariableProvider,
            IMemoryCache memoryCache,
            ILogger<ViteService> logger) {
        _options = options;
        _webHostEnvironment = webHostEnvironment;
        _environmentVariableProvider = environmentVariableProvider;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<string?> GetAssetUrlAsync(string assetPath, CancellationToken cancellationToken = default) {
        if(_webHostEnvironment.EnvironmentName == "Development") {
            var hostname = GetViteHostname();
            var port = GetVitePort();
            var protocol = GetViteProtocol();

            if(assetPath.StartsWith("/", StringComparison.Ordinal)) {
                assetPath = assetPath[1..];
            }

            return $"{protocol}://{hostname}:{port}/{assetPath}";
        }

        var manifest = await GetCachedManifestAsync(cancellationToken);

        manifest.TryGetValue(assetPath, out var asset);

        if(asset == null) {
            _logger.LogWarning("Could not find asset {AssetPath} in manifest.", assetPath);
            return null;
        }

        var file = asset.File;
        if(file?.StartsWith("/", StringComparison.Ordinal) == false) {
            file = "/" + file;
        }

        return file;
    }

    internal string GetViteHostname() {
        var options = _options.Value;
        if(!string.IsNullOrWhiteSpace(options.Hostname)) {
            return options.Hostname;
        }

        var viteHostName = _environmentVariableProvider.GetEnvironmentVariable("VITE_HOSTNAME");
        if(!string.IsNullOrWhiteSpace(viteHostName)) {
            return viteHostName;
        }

        return "localhost";
    }

    internal Int32 GetVitePort() {
        var options = _options.Value;
        if(options.Port.HasValue) {
            return options.Port.Value;
        }

        var vitePortString = _environmentVariableProvider.GetEnvironmentVariable("VITE_PORT");
        if(Int32.TryParse(vitePortString, out var vitePort)) {
            return vitePort;
        }

        return 5173;
    }

    internal string GetViteProtocol() {
        var options = _options.Value;
        if(options.Https == true) {
            return "https";
        }

        var viteHttps = _environmentVariableProvider.GetEnvironmentVariable("VITE_HTTPS");
        if(viteHttps?.Equals("true", StringComparison.OrdinalIgnoreCase) == true) {
            return "https";
        }

        return "http";
    }

    private async Task<ManifestFileType> GetCachedManifestAsync(CancellationToken cancellationToken) {
        var cacheKey = "Shorthand.Vite.ViteManifest";
        if(_memoryCache.TryGetValue(cacheKey, out ManifestFileType? manifest) && manifest != null) {
            return manifest;
        }

        manifest = await GetAssetManifestAsync(cancellationToken);
        _memoryCache.Set(cacheKey, manifest);

        return manifest;
    }

    private async Task<ManifestFileType> GetAssetManifestAsync(CancellationToken cancellationToken) {
        try {
            var options = _options.Value;
            var manifestPath = options.ManifestFileName;

            var fileProvider = _webHostEnvironment.WebRootFileProvider;
            using var fileStream = fileProvider.GetFileInfo(manifestPath).CreateReadStream();
            var manifest = await JsonSerializer.DeserializeAsync(fileStream, typeof(ManifestFileType), ViteManifestContext.Default, cancellationToken) as ManifestFileType;

            return new ManifestFileType(manifest ?? new ManifestFileType(), StringComparer.Ordinal);
        } catch(Exception e) {
            throw new ViteException("Failed to read asset manifest from disk.", e);
        }
    }
}

internal record ViteManifestEntry {
    public string File { get; set; } = string.Empty;
    public string Src { get; set; } = string.Empty;
    public bool IsEntry { get; set; }
}

[JsonSerializable(typeof(ManifestFileType))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class ViteManifestContext : JsonSerializerContext {
}
