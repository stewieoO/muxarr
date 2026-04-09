using System.Net.Http.Headers;
using System.Text;
using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications.Providers;

public class NtfyProvider : NotificationProviderBase
{
    public override string Icon => "bi-megaphone";

    [Field("Server URL", Type = "url", Placeholder = "https://ntfy.sh")]
    public string Url { get; set; } = "";

    [Field("Topic", Placeholder = "muxarr")]
    public string Topic { get; set; } = "";

    [Field("Access Token", HelpText = "Required for self-hosted ntfy instances with authentication.")]
    public string Token { get; set; } = "";

    protected override async Task SendCoreAsync(HttpClient client, NotificationPayload payload)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(Url, Topic));
        request.Headers.Add("Title", payload.Title);
        request.Content = new StringContent(payload.Body, Encoding.UTF8, "text/plain");

        if (!string.IsNullOrEmpty(Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);
        }

        await SendRequestAsync(client, request);
    }
}
