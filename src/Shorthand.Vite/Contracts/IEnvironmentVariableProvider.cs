namespace Shorthand.Vite.Contracts;

public interface IEnvironmentVariableProvider {
    string? GetEnvironmentVariable(string variable);
    void SetEnvironmentVariable(string variable, string? value);
}
