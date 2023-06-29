namespace Shorthand.Vite;

public record ViteOptions {
    public string? Hostname { get; init; }
    public Int32? Port { get; init; }
    public bool? Https { get; init; }
}
