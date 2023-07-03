using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Shorthand.Vite.Contracts;
using Shorthand.Vite.Services;

namespace Shorthand.Vite.Tests.Services;

public class ViteServiceTests {
    [Fact]
    public void CanConstruct() {
        var snapshot = Mock.Of<IOptionsSnapshot<ViteOptions>>();
        var environment = Mock.Of<IWebHostEnvironment>();
        var fileSystemProvider = Mock.Of<IFileSystemProvider>();
        var environmentVariableProvider = new MockEnvironmentVariableProvider();

        var vite = new ViteService(snapshot, environment, fileSystemProvider, environmentVariableProvider, NullLogger<ViteService>.Instance);
        vite.ShouldNotBeNull();
    }

    [Theory]
    [InlineData("test.js", null, null, null, "http://localhost:5173/test.js")]
    [InlineData("/test.js", null, null, null, "http://localhost:5173/test.js")]
    [InlineData("test.js", "192.168.0.1", null, null, "http://192.168.0.1:5173/test.js")]
    [InlineData("test.js", "localhost", 3000, false, "http://localhost:3000/test.js")]
    [InlineData("test.js", "localhost", 3000, true, "https://localhost:3000/test.js")]
    public async Task GetAssetUrlAsync_WhenCalledInDevelopmentModeWithOptions_ReturnsExpectedUrlAsync(string assetPath, string hostname, Int32? port, bool? useHttps, string expected) {
        var snapshot = Mock.Of<IOptionsSnapshot<ViteOptions>>(o => o.Value == new ViteOptions {
            Hostname = hostname,
            Port = port,
            Https = useHttps
        });
        var environment = Mock.Of<IWebHostEnvironment>(e => e.EnvironmentName == "Development");
        var fileSystemProvider = Mock.Of<IFileSystemProvider>();
        var environmentVariableProvider = new MockEnvironmentVariableProvider();

        var vite = new ViteService(snapshot, environment, fileSystemProvider, environmentVariableProvider, NullLogger<ViteService>.Instance);
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
        var snapshot = Mock.Of<IOptionsSnapshot<ViteOptions>>(o => o.Value == new ViteOptions());
        var environment = Mock.Of<IWebHostEnvironment>(e => e.EnvironmentName == "Development");
        var fileSystemProvider = Mock.Of<IFileSystemProvider>();

        var environmentVariableProvider = new MockEnvironmentVariableProvider();
        environmentVariableProvider.SetEnvironmentVariable("VITE_HOSTNAME", hostname);
        environmentVariableProvider.SetEnvironmentVariable("VITE_PORT", port?.ToString());
        environmentVariableProvider.SetEnvironmentVariable("VITE_HTTPS", useHttps?.ToString());

        var vite = new ViteService(snapshot, environment, fileSystemProvider, environmentVariableProvider, NullLogger<ViteService>.Instance);
        var result = await vite.GetAssetUrlAsync(assetPath);

        result.ShouldBe(expected);
    }
}
