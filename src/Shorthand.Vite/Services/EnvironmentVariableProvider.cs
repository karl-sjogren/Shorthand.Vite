using Shorthand.Vite.Contracts;

namespace Shorthand.Vite.Services;

// The whole idea of this abstraction is to make environment variable
// stuff more testable, this however makes this class almost
// untestable, so we exclude it from code coverage.
[ExcludeFromCodeCoverage]
internal class EnvironmentVariableProvider : IEnvironmentVariableProvider {
    public string? GetEnvironmentVariable(string variable) {
        return Environment.GetEnvironmentVariable(variable);
    }

    public void SetEnvironmentVariable(string variable, string? value) {
        Environment.SetEnvironmentVariable(variable, value);
    }
}
