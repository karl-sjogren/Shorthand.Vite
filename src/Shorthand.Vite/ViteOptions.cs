namespace Shorthand.Vite;

public class ViteOptions {
    public string? Hostname { get; set; }
    public Int32? Port { get; set; }
    public bool? Https { get; set; }
    public string ManifestFileName { get; set; } = "manifest.json";

    public string ProxyPrefix { get; set; } = "/.vite";
    public string ProxyRewriteRoot { get; set; } = "/wwwroot";
}
