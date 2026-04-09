using Muxarr.Core.Config;

namespace Muxarr.Web.Services.Notifications.Providers;

public class TelegramProvider : NotificationProviderBase
{
    public override string Icon => "bi-telegram";

    [Field("Bot Token", HelpText = "Create a bot via @BotFather to get this token.")]
    public string BotToken { get; set; } = "";

    [Field("Chat ID", HelpText = "User, group, or channel ID to send messages to.")]
    public string ChatId { get; set; } = "";

    protected override async Task SendCoreAsync(HttpClient client, NotificationPayload payload)
    {
        var url = $"https://api.telegram.org/bot{BotToken}/sendMessage";
        await PostJsonAsync(client, url, new
        {
            chat_id = ChatId,
            text = $"<b>{payload.Title}</b>\n{payload.Body}",
            parse_mode = "HTML"
        });
    }
}
