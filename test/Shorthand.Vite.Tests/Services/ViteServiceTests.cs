using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Shorthand.Vite.Contracts;
using Shorthand.Vite.Services;

namespace Shorthand.Vite.Tests.Services;

public class ViteServiceTests {
    [Fact]
    public void CanConstruct() {
        var snapshot = A.Fake<IOptionsSnapshot<ViteOptions>>();
        var environment = A.Fake<IWebHostEnvironment>();
        var environmentVariableProvider = new MockEnvironmentVariableProvider();

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var vite = new ViteService(snapshot, environment, environmentVariableProvider, memoryCache, NullLogger<ViteService>.Instance);
        vite.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("test.js", null, null, null, "http://localhost:5173/test.js")]
    [InlineData("/test.js", null, null, null, "http://localhost:5173/test.js")]
    [InlineData("test.js", "192.168.0.1", null, null, "http://192.168.0.1:5173/test.js")]
    [InlineData("test.js", "localhost", 3000, false, "http://localhost:3000/test.js")]
    [InlineData("test.js", "localhost", 3000, true, "https://localhost:3000/test.js")]
    public async Task GetAssetUrlAsync_WhenCalledInDevelopmentModeWithOptions_ReturnsExpectedUrlAsync(string assetPath, string hostname, Int32? port, bool? useHttps, string expected) {
        var snapshot = A.Fake<IOptionsSnapshot<ViteOptions>>();
        A.CallTo(() => snapshot.Value).Returns(new ViteOptions {
            Hostname = hostname,
            Port = port,
            Https = useHttps
        });

        var fileSystemProvider = A.Fake<IFileSystemProvider>();

        var environment = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => environment.EnvironmentName).Returns("Development");

        var environmentVariableProvider = new MockEnvironmentVariableProvider();

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var vite = new ViteService(snapshot, environment, environmentVariableProvider, memoryCache, NullLogger<ViteService>.Instance);
        var result = await vite.GetAssetUrlAsync(assetPath);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("test.js", null, null, null, "http://localhost:5173/test.js")]
    [InlineData("/test.js", null, null, null, "http://localhost:5173/test.js")]
    [InlineData("test.js", "192.168.0.1", null, null, "http://192.168.0.1:5173/test.js")]
    [InlineData("test.js", "localhost", 3000, false, "http://localhost:3000/test.js")]
    [InlineData("test.js", "localhost", 3000, true, "https://localhost:3000/test.js")]
    public async Task GetAssetUrlAsync_WhenCalledInDevelopmentModeWithEnvironmentVariables_ReturnsExpectedUrlAsync(string assetPath, string hostname, Int32? port, bool? useHttps, string expected) {
        var snapshot = A.Fake<IOptionsSnapshot<ViteOptions>>();

        var fileSystemProvider = A.Fake<IFileSystemProvider>();

        var environment = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => environment.EnvironmentName).Returns("Development");

        var environmentVariableProvider = new MockEnvironmentVariableProvider();
        environmentVariableProvider.SetEnvironmentVariable("VITE_HOSTNAME", hostname);
        environmentVariableProvider.SetEnvironmentVariable("VITE_PORT", port?.ToString());
        environmentVariableProvider.SetEnvironmentVariable("VITE_HTTPS", useHttps?.ToString());

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var vite = new ViteService(snapshot, environment, environmentVariableProvider, memoryCache, NullLogger<ViteService>.Instance);
        var result = await vite.GetAssetUrlAsync(assetPath);

        result.ShouldBe(expected);
    }

    [Fact]
    public async Task GetAssetUrlAsync_WhenCalledInProductionModeWithRealAsset_ReturnsExpectedUrlAsync() {
        var snapshot = A.Fake<IOptionsSnapshot<ViteOptions>>();

        var fileSystemProvider = new InMemoryFileSystemProvider();
        fileSystemProvider.WriteAllText("/manifest.json", """
{
  "site.js": {
    "file": "site-b5760e8e.js",
    "isEntry": true,
    "src": "site.js"
  }
}
""");

        var environment = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => environment.EnvironmentName).Returns("Production");
        A.CallTo(() => environment.WebRootFileProvider).Returns(fileSystemProvider);

        var environmentVariableProvider = new MockEnvironmentVariableProvider();

        var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        var vite = new ViteService(snapshot, environment, environmentVariableProvider, memoryCache, NullLogger<ViteService>.Instance);
        var result = await vite.GetAssetUrlAsync("site.js");

        result.ShouldBe("/site-b5760e8e.js");
    }
}
