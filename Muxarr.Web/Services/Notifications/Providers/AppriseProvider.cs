using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications.Providers;

public class AppriseProvider : NotificationProviderBase
{
    public override string Icon => "bi-collection";

    [Field("Apprise API URL", Type = "url", Placeholder = "http://apprise:8000")]
    public string Url { get; set; } = "";

    [Field("Tag", HelpText = "Only notify URLs tagged with this value in your Apprise config. Leave empty to notify all.")]
    public string Tag { get; set; } = "";

    protected override async Task SendCoreAsync(HttpClient client, NotificationPayload payload)
    {
        var body = new Dictionary<string, string>
        {
            ["title"] = payload.Title,
            ["body"] = payload.Body,
            ["type"] = payload.EventType switch
            {
                NotificationEventType.Failed => "failure",
                NotificationEventType.Completed => "success",
                _ => "info"
            }
        };

        if (!string.IsNullOrEmpty(Tag))
        {
            body["tag"] = Tag;
        }

        await PostJsonAsync(client, BuildUrl(Url, "notify"), body);
    }
}
