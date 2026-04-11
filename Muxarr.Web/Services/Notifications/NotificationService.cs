using System.Collections.Concurrent;
using Muxarr.Core.Config;
using Muxarr.Core.Extensions;
using Muxarr.Data;
using Muxarr.Data.Entities;
using Muxarr.Data.Extensions;
using Muxarr.Web.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Muxarr.Web.Services.Notifications;

public static class NotificationRegistration
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddImplementations<NotificationProvider>();
        services.AddSingleton<NotificationService>();
        return services;
    }
}

public record NotificationTestResult(bool Success, string Message)
{
    public static NotificationTestResult Ok(string message = "Test notification sent!") => new(true, message);
    public static NotificationTestResult Fail(string message) => new(false, message);
}

public class NotificationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, NotificationProvider> _providers;
    private readonly ILogger<NotificationService> _logger;
    // Tracks conversions that already fired a "Started" notification, so converter retries
    // don't spam. Terminal states clear the entry. Used as a concurrent set.
    private readonly ConcurrentDictionary<int, byte> _startedFired = new();

    public IReadOnlyCollection<NotificationProvider> Providers => _providers.Values;

    public NotificationService(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        IEnumerable<NotificationProvider> providers,
        MediaConverterService converter,
        ILogger<NotificationService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClient = httpClientFactory.CreateClient();
        _providers = providers.ToDictionary(p => p.Type);
        _logger = logger;

        converter.ConverterStateChanged += OnConverterStateChanged;
    }

    private void OnConverterStateChanged(ConverterProgressEvent e)
    {
        var conversion = e.Conversion;
        var eventType = conversion.State switch
        {
            ConversionState.Processing => NotificationEventType.Started,
            ConversionState.Completed => NotificationEventType.Completed,
            ConversionState.Failed => NotificationEventType.Failed,
            _ => (NotificationEventType?)null
        };

        if (eventType is null)
        {
            return;
        }

        if (eventType == NotificationEventType.Started)
        {
            if (!_startedFired.TryAdd(conversion.Id, 0))
            {
                return;
            }
        }
        else
        {
            _startedFired.TryRemove(conversion.Id, out _);
        }

        _ = SendAsync(eventType.Value, conversion);
    }

    public async Task SendAsync(NotificationEventType eventType, MediaConversion conversion)
    {
        List<NotificationConfig> configs;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            await using var context = await scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>()
                .CreateDbContextAsync();
            configs = context.Configs.GetOrDefault<List<NotificationConfig>>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load notification configs");
            return;
        }

        var payload = BuildPayload(eventType, conversion);

        foreach (var config in configs.Where(c => c.Enabled && c.HasTrigger(eventType)))
        {
            if (!_providers.TryGetValue(config.Provider, out var provider))
            {
                continue;
            }

            try
            {
                await provider.SendAsync(_httpClient, config, payload);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Notification '{Name}' ({Provider}) failed", config.Name, config.Provider);
            }
        }
    }

    public async Task<NotificationTestResult> SendTestAsync(NotificationConfig config)
    {
        if (!_providers.TryGetValue(config.Provider, out var provider))
        {
            return NotificationTestResult.Fail($"Unknown provider: {config.Provider}");
        }

        try
        {
            var payload = new NotificationPayload
            {
                Title = "Muxarr Test",
                Body = "If you see this, notifications are working!",
                EventType = NotificationEventType.Test
            };
            await provider.SendAsync(_httpClient, config, payload);
            return NotificationTestResult.Ok();
        }
        catch (Exception ex)
        {
            return NotificationTestResult.Fail(ex.Message);
        }
    }

    private static NotificationPayload BuildPayload(NotificationEventType type, MediaConversion conversion)
    {
        var (title, body) = type switch
        {
            NotificationEventType.Started =>
                ("Conversion Started", $"{conversion.Name} - {conversion.SizeBefore.DisplayFileSize()}"),
            NotificationEventType.Completed =>
                ("Conversion Completed",
                    $"{conversion.Name} - saved {conversion.SizeDifference.DisplayFileSize()} ({conversion.GetSizeChangePercentage()})"),
            NotificationEventType.Failed =>
                ("Conversion Failed", $"{conversion.Name} - {GetLastError(conversion)}"),
            _ => ("Muxarr", conversion.Name)
        };

        return new NotificationPayload
        {
            Title = title,
            Body = body,
            EventType = type,
            FileName = conversion.Name,
            SizeBefore = conversion.SizeBefore,
            SizeAfter = type == NotificationEventType.Completed ? conversion.SizeAfter : null,
            SizeSaved = type == NotificationEventType.Completed ? conversion.SizeDifference : null,
            Error = type == NotificationEventType.Failed ? GetLastError(conversion) : null
        };
    }

    private static string GetLastError(MediaConversion conversion)
    {
        if (string.IsNullOrEmpty(conversion.Log))
        {
            return "Unknown error";
        }

        var lines = conversion.Log.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            return "Unknown error";
        }

        var lastLine = lines[^1];
        var bracketEnd = lastLine.IndexOf(']');
        if (bracketEnd >= 0 && bracketEnd + 1 < lastLine.Length)
        {
            return lastLine[(bracketEnd + 1)..].Trim();
        }

        return lastLine.Trim();
    }
}
