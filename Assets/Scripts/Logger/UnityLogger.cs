using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using UnityEngine;

public sealed class UnityLogger : Microsoft.Extensions.Logging.ILogger
{
    private readonly string _category;

    public UnityLogger(string category)
    {
        _category = category;
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel,
                            EventId eventId,
                            TState state,
                            Exception exception,
                            Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) { return; }

        string message = $"[{eventId.Id}: {logLevel}] {_category} - {formatter(state, exception)}";

        switch (logLevel)
        {
            case LogLevel.Trace:
                break;
            case LogLevel.Debug:
                Debug.Log($"<color=#000080ff>{message}</color>");
                break;
            case LogLevel.Information:
                Debug.Log($"<color=#004000ff>{message}</color>");
                break;
            case LogLevel.Warning:
                Debug.LogWarning($"<color=#bf6500ff>{message}</color>");
                break;
            case LogLevel.Error:
                Debug.LogError($"<color=red>{message}</color>");
                break;
            case LogLevel.Critical:
                Debug.unityLogger.LogException(exception);
                break;
            case LogLevel.None:
                break;
        }
    }
}

public sealed class UnityLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, UnityLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, categoryName => new UnityLogger(categoryName));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public static class UnityLoggerExtensions
{
    public static ILoggingBuilder AddUnityLogger(this ILoggingBuilder builder)
    {
        //builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, UnityLoggerProvider>());
        return builder;
    }
}