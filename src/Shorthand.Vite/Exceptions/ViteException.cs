namespace Shorthand.Vite.Exceptions;

public class ViteException : Exception {
    public ViteException() {
    }

    public ViteException(string message) : base(message) {
    }

    public ViteException(string? message, Exception? innerException) : base(message, innerException) {
    }
}
