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

        var vite = new ViteService(snapshot, environment, fileSystemProvider, NullLogger<ViteService>.Instance);
        vite.ShouldNotBeNull();
    }
}
