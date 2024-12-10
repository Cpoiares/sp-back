using Microsoft.Extensions.Logging;

namespace sp_back_api.Loging;

public class FileLogger : ILogger
{
    private readonly string _path;
    private readonly object _lock = new object();

    public FileLogger(string path)
    {
        _path = path;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        var logEntry = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{logLevel}] {message}";
        if (exception != null)
        {
            logEntry += $"\nException: {exception}\nStackTrace: {exception.StackTrace}";
        }

        lock (_lock)
        {
            File.AppendAllText(_path, logEntry + Environment.NewLine);
        }
    }
}