using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications.Providers;

public class DiscordProvider : NotificationProviderBase
{
    public override string Icon => "bi-discord";

    [Field("Webhook URL", Type = "url", Placeholder = "https://discord.com/api/webhooks/...")]
    public string Url { get; set; } = "";

    protected override async Task SendCoreAsync(HttpClient client, NotificationPayload payload)
    {
        var color = payload.EventType switch
        {
            NotificationEventType.Completed => 3066993,  // green
            NotificationEventType.Failed => 15158332,    // red
            _ => 3447003                                 // blue
        };

        await PostJsonAsync(client, Url, new
        {
            embeds = new[]
            {
                new { title = payload.Title, description = payload.Body, color }
            }
        });
    }
}
