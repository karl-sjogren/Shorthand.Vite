using Shorthand.Vite.Contracts;

namespace Shorthand.Vite.Tests;

internal class MockEnvironmentVariableProvider : IEnvironmentVariableProvider {
    private readonly Dictionary<string, string?> _variables = new();

    public string? GetEnvironmentVariable(string variable) {
        _variables.TryGetValue(variable, out var value);

        return value;
    }

    public void SetEnvironmentVariable(string variable, string? value) {
        _variables[variable] = value;
    }
}
