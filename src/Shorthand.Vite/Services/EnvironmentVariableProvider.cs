using Shorthand.Vite.Contracts;

namespace Shorthand.Vite.Services;

internal class EnvironmentVariableProvider : IEnvironmentVariableProvider {
    public string? GetEnvironmentVariable(string variable) {
        return Environment.GetEnvironmentVariable(variable);
    }

    public void SetEnvironmentVariable(string variable, string? value) {
        Environment.SetEnvironmentVariable(variable, value);
    }
}
