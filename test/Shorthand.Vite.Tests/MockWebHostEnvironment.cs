using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Shorthand.Vite.Tests;

internal class MockWebHostEnvironment : IWebHostEnvironment {
    public string WebRootPath { get; set; } = string.Empty;
    public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string EnvironmentName { get; set; } = string.Empty;
}
