using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications.Providers;

public class SlackProvider : NotificationProviderBase
{
    public override string Icon => "bi-slack";

    [Field("Webhook URL", Type = "url", Placeholder = "https://hooks.slack.com/services/...")]
    public string Url { get; set; } = "";

    protected override async Task SendCoreAsync(HttpClient client, NotificationPayload payload)
    {
        await PostJsonAsync(client, Url, new
        {
            text = $"*{payload.Title}*\n{payload.Body}"
        });
    }
}
