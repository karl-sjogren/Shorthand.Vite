using System.Text;
using Microsoft.Extensions.Logging;
using Shorthand.Vite.Contracts;

namespace Shorthand.Vite.Services;

// The whole idea of this abstraction is to make file system
// stuff more testable, this however makes this class almost
// untestable, so we exclude it from code coverage.
[ExcludeFromCodeCoverage]
internal class FileSystemProvider : IFileSystemProvider {
    private readonly ILogger<FileSystemProvider> _logger;

    public FileSystemProvider(ILogger<FileSystemProvider> logger) {
        _logger = logger;
    }

    public void CreateDirectory(string path) {
        Directory.CreateDirectory(path);
    }

    public bool DirectoryExists(string path) {
        return Directory.Exists(path);
    }

    public bool FileExists(string path) {
        return File.Exists(path);
    }

    public IReadOnlyCollection<string> GetSubdirectories(string path) {
        return Directory.GetDirectories(path);
    }

    public IReadOnlyCollection<string> GetFiles(string path) {
        return Directory.GetFiles(path);
    }

    public string ReadAllText(string path, Encoding? encoding = null) {
        return File.ReadAllText(path, encoding ?? Encoding.UTF8);
    }

    public Stream OpenRead(string path) {
        return File.OpenRead(path);
    }
}
