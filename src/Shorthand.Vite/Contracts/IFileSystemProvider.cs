using System.Text;

namespace Shorthand.Vite.Contracts;

public interface IFileSystemProvider {
    void CreateDirectory(string path);
    bool DirectoryExists(string path);
    bool FileExists(string path);
    string ReadAllText(string path, Encoding? encoding);
    Stream OpenRead(string path);
    IReadOnlyCollection<string> GetSubdirectories(string path);
    IReadOnlyCollection<string> GetFiles(string path);
}
