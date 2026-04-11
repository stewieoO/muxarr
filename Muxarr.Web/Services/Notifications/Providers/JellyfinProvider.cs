using Muxarr.Web.Components.Shared;

namespace Muxarr.Web.Services.Notifications.Providers;

public class JellyfinSettings
{
    [Field("Server URL", Type = FieldType.Url, Placeholder = "http://192.168.1.10:8096",
        HelpText = "Your Jellyfin server base URL.")]
    public string ServerUrl { get; set; } = "";

    [Field("API Key", Type = FieldType.Password,
        HelpText = "Generate one at Dashboard > API Keys.")]
    public string ApiKey { get; set; } = "";

    [Field("Library Item ID", Placeholder = "leave blank to refresh the entire server",
        HelpText = "VirtualFolder ItemId. Find via /Library/VirtualFolders.")]
    public string LibraryItemId { get; set; } = "";

    [Field("Full metadata refresh", Type = FieldType.Checkbox,
        HelpText = "Re-fetches all metadata and images instead of only what's missing. Slower; only applies when a Library Item ID is set.")]
    public bool FullMetadataRefresh { get; set; }
}

public class JellyfinProvider : NotificationProvider<JellyfinSettings>
{
    public override string Icon => "bi-collection-play";

    protected override async Task SendCoreAsync(HttpClient client, JellyfinSettings s, NotificationPayload payload)
    {
        if (string.IsNullOrWhiteSpace(s.ServerUrl) || string.IsNullOrWhiteSpace(s.ApiKey))
        {
            throw new InvalidOperationException("Jellyfin Server URL and API Key are required.");
        }

        var baseUrl = s.ServerUrl.TrimEnd('/');
        string url;

        if (string.IsNullOrWhiteSpace(s.LibraryItemId))
        {
            url = $"{baseUrl}/Library/Refresh";
        }
        else
        {
            var mode = s.FullMetadataRefresh ? "FullRefresh" : "Default";
            url = $"{baseUrl}/Items/{Uri.EscapeDataString(s.LibraryItemId.Trim())}/Refresh"
                  + $"?metadataRefreshMode={mode}&imageRefreshMode={mode}";
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("X-Emby-Token", s.ApiKey);
        await SendRequestAsync(client, request);
    }
}
