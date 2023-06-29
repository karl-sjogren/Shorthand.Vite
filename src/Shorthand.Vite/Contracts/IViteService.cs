namespace Shorthand.Vite.Contracts;

public interface IViteService {
    Task<string?> GetAssetUrlAsync(string assetPath, CancellationToken cancellationToken = default);
}
