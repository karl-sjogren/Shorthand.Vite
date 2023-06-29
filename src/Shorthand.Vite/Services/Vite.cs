using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shorthand.Vite.Contracts;
using Shorthand.Vite.Exceptions;

namespace Shorthand.Vite.Services;

public class Vite : IVite {
    private readonly IOptionsSnapshot<ViteOptions> _options;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileSystemProvider _fileSystemProvider;
    private readonly ILogger<Vite> _logger;

    private static readonly JsonSerializerOptions _manifestJsonOptions = new(JsonSerializerDefaults.Web);

    public Vite(IOptionsSnapshot<ViteOptions> options, IWebHostEnvironment webHostEnvironment, IFileSystemProvider fileSystemProvider, ILogger<Vite> logger) {
        _options = options;
        _webHostEnvironment = webHostEnvironment;
        _fileSystemProvider = fileSystemProvider;
        _logger = logger;
    }

    public async Task<string?> GetAssetUrlAsync(string assetPath, CancellationToken cancellationToken = default) {
        if(_webHostEnvironment.EnvironmentName == "Development") {
            var hostname = GetViteHostname();
            var port = GetVitePort();
            return $"https://{hostname}:{port}/{assetPath}";
        }

        var manifest = await GetAssetManifestAsync(cancellationToken);

        manifest.TryGetValue(assetPath, out var asset);

        var file = asset?.File;
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

        var viteHostName = Environment.GetEnvironmentVariable("VITE_HOSTNAME");
        if(!string.IsNullOrWhiteSpace(viteHostName)) {
            return viteHostName;
        }

        return "127.0.0.1";
    }

    private Int32 GetVitePort() {
        var options = _options.Value;
        if(options.Port.HasValue) {
            return options.Port.Value;
        }

        var vitePortString = Environment.GetEnvironmentVariable("VITE_PORT");
        if(Int32.TryParse(vitePortString, out var vitePort)) {
            return vitePort;
        }

        return 5010;
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

    private class ManifestEntry {
        public string File { get; set; } = string.Empty;
        public string Src { get; set; } = string.Empty;
        public bool IsEntry { get; set; }
    }
}
