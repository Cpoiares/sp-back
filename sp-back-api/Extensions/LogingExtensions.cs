using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using sp_back_api.Loging;

namespace sp_back_api.Extensions;

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, Action<FileLoggerOptions> configure)
    {
        var options = new FileLoggerOptions();
        configure(options);
        builder.Services.AddSingleton(new FileLogger(options.FilePath));
        return builder;
    }
}

public class FileLoggerOptions
{
    public string FilePath { get; set; } = "app.log";
}


