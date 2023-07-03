using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shorthand.Vite.Contracts;
using Shorthand.Vite.Exceptions;

namespace Shorthand.Vite.Services;

public class ViteService : IViteService {
    private readonly IOptionsSnapshot<ViteOptions> _options;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly IEnvironmentVariableProvider _environmentVariableProvider;
    private readonly ILogger<ViteService> _logger;

    private static readonly JsonSerializerOptions _manifestJsonOptions = new(JsonSerializerDefaults.Web);

    internal ViteService(
            IOptionsSnapshot<ViteOptions> options,
            IWebHostEnvironment webHostEnvironment,
            IFileSystemProvider fileSystemProvider,
            IEnvironmentVariableProvider environmentVariableProvider,
            ILogger<ViteService> logger) {
        _options = options;
        _webHostEnvironment = webHostEnvironment;
        _fileSystemProvider = fileSystemProvider;
        _environmentVariableProvider = environmentVariableProvider;
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

        var manifest = await GetAssetManifestAsync(cancellationToken);

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

    private async Task<Dictionary<string, ManifestEntry>> GetAssetManifestAsync(CancellationToken cancellationToken) {
        try {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var manifestPath = Path.Combine(webRootPath, "manifest.json");

            using var fileStream = _fileSystemProvider.OpenRead(manifestPath);
            var manifest = await JsonSerializer.DeserializeAsync<Dictionary<string, ManifestEntry>>(fileStream, _manifestJsonOptions, cancellationToken);

            return new Dictionary<string, ManifestEntry>(manifest ?? new Dictionary<string, ManifestEntry>(), StringComparer.Ordinal);
        } catch(Exception e) {
            throw new ViteException("Failed to read asset manifest from disk.", e);
        }
    }

    private record ManifestEntry {
        public string File { get; init; } = string.Empty;
        public string Src { get; init; } = string.Empty;
        public bool IsEntry { get; init; }
    }
}
