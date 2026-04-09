using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications.Providers;

public class WebhookProvider : NotificationProviderBase
{
    public override string Icon => "bi-globe";

    [Field("URL", Type = "url", Placeholder = "https://...")]
    public string Url { get; set; } = "";

    protected override async Task SendCoreAsync(HttpClient client, NotificationPayload payload)
    {
        await PostJsonAsync(client, Url, new
        {
            @event = payload.EventType?.ToString(),
            title = payload.Title,
            body = payload.Body,
            fileName = payload.FileName,
            sizeBefore = payload.SizeBefore,
            sizeAfter = payload.SizeAfter,
            sizeSaved = payload.SizeSaved,
            error = payload.Error,
            timestamp = payload.Timestamp
        });
    }
}
