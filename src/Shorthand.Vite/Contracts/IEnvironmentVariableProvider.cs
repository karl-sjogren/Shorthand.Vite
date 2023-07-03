namespace Shorthand.Vite.Contracts;

internal interface IEnvironmentVariableProvider {
    string? GetEnvironmentVariable(string variable);
    void SetEnvironmentVariable(string variable, string? value);
}
