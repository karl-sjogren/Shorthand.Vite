namespace Shorthand.Vite.Contracts;

public interface IVite {
    Task<string?> GetAssetUrlAsync(string assetPath, CancellationToken cancellationToken = default);
}
