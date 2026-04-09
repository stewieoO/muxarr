using System.Net.Http.Json;
using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications.Providers;

public class GotifyProvider : NotificationProviderBase
{
    [Field("Server URL", Type = "url", Placeholder = "https://gotify.example.com")]
    public string Url { get; set; } = "";

    [Field("App Token")]
    public string Token { get; set; } = "";

    protected override async Task SendCoreAsync(HttpClient client, NotificationPayload payload)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(Url, "message"));
        request.Headers.Add("X-Gotify-Key", Token);
        request.Content = JsonContent.Create(new { title = payload.Title, message = payload.Body, priority = 5 });
        await SendRequestAsync(client, request);
    }
}
