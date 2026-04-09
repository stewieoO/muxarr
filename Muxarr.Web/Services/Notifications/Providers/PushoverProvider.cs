using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications.Providers;

public class PushoverProvider : NotificationProviderBase
{
    public override string Icon => "bi-phone";

    [Field("App Token")]
    public string AppToken { get; set; } = "";

    [Field("User Key")]
    public string UserKey { get; set; } = "";

    protected override async Task SendCoreAsync(HttpClient client, NotificationPayload payload)
    {
        await PostJsonAsync(client, "https://api.pushover.net/1/messages.json", new
        {
            token = AppToken,
            user = UserKey,
            title = payload.Title,
            message = payload.Body
        });
    }
}
