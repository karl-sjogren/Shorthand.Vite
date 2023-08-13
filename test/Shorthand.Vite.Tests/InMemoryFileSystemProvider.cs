using System.Collections;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Shorthand.Vite.Contracts;

namespace Shorthand.Vite.Tests;

public class InMemoryFileSystemProvider : IFileSystemProvider, IFileProvider {
    private readonly InMemoryDirectory _root;

    public InMemoryFileSystemProvider() {
        _root = new InMemoryDirectory(RootPath, true);
    }

    public static string RootPath => Path.DirectorySeparatorChar.ToString();

    public void CreateDirectory(string path) {
        var parentDirectory = GetParentDirectory(path);

        var directory = new InMemoryDirectory(path);
        parentDirectory.AddDirectory(directory);
    }

    public bool DirectoryExists(string path) {
        try {
            var directory = GetDirectoryFromPath(path);

            return directory != null;
        } catch(DirectoryNotFoundException) {
            return false;
        } catch(FileNotFoundException) {
            return false;
        }
    }

    public bool FileExists(string path) {
        try {
            var file = GetFileFromPath(path);

            return file != null;
        } catch(DirectoryNotFoundException) {
            return false;
        } catch(FileNotFoundException) {
            return false;
        }
    }

    public IReadOnlyCollection<string> GetFiles(string path) {
        var directory = GetDirectoryFromPath(path);

        return directory.Files.Select(f => f.FullPath).ToList();
    }

    public IReadOnlyCollection<string> GetSubdirectories(string path) {
        var directory = GetDirectoryFromPath(path);

        return directory.Directories.Select(d => d.FullPath).ToList();
    }

    public string ReadAllText(string path, Encoding? encoding = null) {
        var file = GetFileFromPath(path);

        file.ContentStream.Position = 0;
        using var sr = new StreamReader(file.ContentStream, encoding ?? Encoding.UTF8, leaveOpen: true);

        return sr.ReadToEnd();
    }

    private InMemoryDirectory GetParentDirectory(string path) {
        var parts = path.Split(Path.DirectorySeparatorChar).SkipLast(1);
        var parentPath = string.Join(Path.DirectorySeparatorChar, parts);
        return GetDirectoryFromPath(parentPath);
    }

    private InMemoryDirectory GetDirectoryFromPath(string path) {
        var parts = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var current = _root;
        foreach(var part in parts) {
            var next = current.Directories.FirstOrDefault(d => d.Name == part);
            if(next != null) {
                current = next;
                continue;
            }

            var file = current.Files.FirstOrDefault(f => f.Name == part);
            if(file != null) {
                throw new DirectoryNotFoundException($"Path {path} is not a directory.");
            }

            throw new DirectoryNotFoundException($"Directory {part} not found in {current.Name}.");
        }

        return current;
    }

    private InMemoryFile GetFileFromPath(string path) {
        var parentDirectory = GetParentDirectory(path);
        var fileName = Path.GetFileName(path);

        var file = parentDirectory.Files.FirstOrDefault(f => f.Name == fileName);
        if(file == null) {
            throw new FileNotFoundException($"File {fileName} not found in {parentDirectory.Name}.");
        }

        return file;
    }

    public void WriteAllBytes(string path, byte[] bytes) {
        var file = GetOrCreateFileFromPath(path);
        file.ContentStream.Write(bytes);
    }

    public void WriteAllText(string path, string contents, Encoding? encoding = null) {
        var file = GetOrCreateFileFromPath(path);
        using var sw = new StreamWriter(file.ContentStream, encoding ?? Encoding.UTF8, leaveOpen: true);

        sw.Write(contents);
    }

    private InMemoryFile GetOrCreateFileFromPath(string path) {
        InMemoryFile file;
        try {
            file = GetFileFromPath(path);
        } catch(FileNotFoundException) {
            var parentDirectory = GetParentDirectory(path);

            file = new InMemoryFile(path);
            parentDirectory.AddFile(file);
        }

        return file;
    }

    public Stream OpenRead(string path) {
        var file = GetFileFromPath(path);

        file.ContentStream.Position = 0;
        return file.ContentStream;
    }

    public IDirectoryContents GetDirectoryContents(string subpath) {
        var directory = GetDirectoryFromPath(subpath);
        return new InMemoryDirectoryInfo(directory);
    }

    public IFileInfo GetFileInfo(string subpath) {
        var file = GetFileFromPath(subpath);
        return new InMemoryFileInfo(file);
    }

    public IChangeToken Watch(string filter) {
        throw new NotImplementedException();
    }
}

public class InMemoryDirectory {
    private readonly List<InMemoryDirectory> _directories = new();
    private readonly List<InMemoryFile> _files = new();

    public InMemoryDirectory(string path, bool isRoot = false) {
        FullPath = path;
        Name = Path.GetFileName(Path.TrimEndingDirectorySeparator(path)) ?? string.Empty;
        IsRoot = isRoot;

        if(!IsRoot && Name.Length == 0) {
            throw new ArgumentException("Path is not a directory.", nameof(path));
        }
    }

    public string Name { get; set; }
    public string FullPath { get; set; }
    public bool IsRoot { get; set; }
    public InMemoryDirectory? ParentDirectory { get; private set; }

    public IReadOnlyCollection<InMemoryDirectory> Directories => _directories;
    public IReadOnlyCollection<InMemoryFile> Files => _files;

    public void AddDirectory(InMemoryDirectory directory) {
        directory.ParentDirectory = this;
        _directories.Add(directory);
    }

    public void AddFile(InMemoryFile file) {
        file.Directory = this;
        _files.Add(file);
    }

    public void RemoveDirectory(InMemoryDirectory directory) {
        directory.ParentDirectory = null;
        _directories.Remove(directory);
    }

    public void RemoveFile(InMemoryFile file) {
        file.Directory = null;
        _files.Remove(file);
    }
}

public class InMemoryFile {
    public InMemoryFile(string path, Stream? content = null) {
        FullPath = path;
        Name = Path.GetFileName(path) ?? throw new ArgumentException("Path is not a file.", nameof(path));

        ContentStream = content ?? new MemoryStream();
    }

    public string Name { get; set; }
    public string FullPath { get; set; }
    public Stream ContentStream { get; set; }
    public InMemoryDirectory? Directory { get; set; }
}

public class InMemoryDirectoryInfo : IDirectoryContents {
    private readonly InMemoryDirectory _directory;

    public InMemoryDirectoryInfo(InMemoryDirectory directory) {
        _directory = directory;
    }

    public bool Exists => true;

    public IEnumerator<IFileInfo> GetEnumerator() {
        return _directory.Files.Select(f => new InMemoryFileInfo(f)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}

public class InMemoryFileInfo : IFileInfo {
    private readonly InMemoryFile _file;

    public InMemoryFileInfo(InMemoryFile file) {
        _file = file;
    }

    public bool Exists => true;

    public bool IsDirectory => false;

    public DateTimeOffset LastModified => DateTimeOffset.MinValue;

    public Int64 Length => _file.ContentStream.Length;

    public string Name => _file.Name;

    public string? PhysicalPath => _file.FullPath;

    public Stream CreateReadStream() {
        _file.ContentStream.Position = 0;
        return _file.ContentStream;
    }
}
