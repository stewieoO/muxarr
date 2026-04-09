using System.Net.Http.Json;
using System.Reflection;
using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications;

public interface INotificationProvider
{
    string Type { get; }
    string Icon { get; }
    IReadOnlyList<FieldDefinition> Fields { get; }
    Task SendAsync(HttpClient client, NotificationConfig config, NotificationPayload payload);
}

[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute(string label) : Attribute
{
    public string Label { get; } = label;
    public string Placeholder { get; set; } = "";
    public string HelpText { get; set; } = "";
    public string Type { get; set; } = "text";
}

public class FieldDefinition
{
    public required string Name { get; init; }
    public required string Label { get; init; }
    public string Placeholder { get; init; } = "";
    public string HelpText { get; init; } = "";
    public string InputType { get; init; } = "text";
}

public abstract class NotificationProviderBase : INotificationProvider
{
    private (PropertyInfo Property, FieldAttribute Attribute)[]? _fieldCache;

    private (PropertyInfo Property, FieldAttribute Attribute)[] GetFieldProperties()
    {
        return _fieldCache ??= GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<FieldAttribute>()!))
            .Where(x => x.Attribute != null)
            .ToArray();
    }

    public string Type => GetType().Name.Replace("Provider", "");
    public virtual string Icon => "bi-bell";

    public IReadOnlyList<FieldDefinition> Fields => GetFieldProperties()
        .Select(x => new FieldDefinition
        {
            Name = x.Property.Name,
            Label = x.Attribute.Label,
            Placeholder = x.Attribute.Placeholder,
            HelpText = x.Attribute.HelpText,
            InputType = x.Attribute.Type
        })
        .ToArray();

    public Task SendAsync(HttpClient client, NotificationConfig config, NotificationPayload payload)
    {
        foreach (var (prop, _) in GetFieldProperties())
        {
            prop.SetValue(this, config.Settings.GetValueOrDefault(prop.Name, ""));
        }

        return SendCoreAsync(client, payload);
    }

    protected abstract Task SendCoreAsync(HttpClient client, NotificationPayload payload);

    protected static async Task PostJsonAsync(HttpClient client, string url, object body)
    {
        using var response = await client.PostAsJsonAsync(url, body);
        await response.EnsureSuccess();
    }

    protected static async Task SendRequestAsync(HttpClient client, HttpRequestMessage request)
    {
        using var response = await client.SendAsync(request);
        await response.EnsureSuccess();
    }

    protected static string BuildUrl(string baseUrl, params string[] segments)
    {
        var result = baseUrl.TrimEnd('/');
        foreach (var segment in segments)
        {
            if (!string.IsNullOrEmpty(segment))
            {
                result += "/" + segment.Trim('/');
            }
        }

        return result;
    }
}

public class NotificationPayload
{
    public required string Title { get; init; }
    public required string Body { get; init; }
    public NotificationEventType? EventType { get; init; }
    public string? FileName { get; init; }
    public long? SizeBefore { get; init; }
    public long? SizeAfter { get; init; }
    public long? SizeSaved { get; init; }
    public string? Error { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

internal static class HttpResponseExtensions
{
    public static async Task EnsureSuccess(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        if (body.Length > 500)
        {
            body = body[..500];
        }

        var message = string.IsNullOrWhiteSpace(body)
            ? $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"
            : $"HTTP {(int)response.StatusCode}: {body}";

        throw new HttpRequestException(message);
    }
}
