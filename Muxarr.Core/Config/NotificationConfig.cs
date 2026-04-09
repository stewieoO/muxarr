namespace Muxarr.Core.Config;

public class NotificationConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = "";
    public string Provider { get; set; } = "";
    public bool Enabled { get; set; } = true;

    public bool OnStarted { get; set; }
    public bool OnCompleted { get; set; } = true;
    public bool OnFailed { get; set; } = true;

    public Dictionary<string, string> Settings { get; set; } = new();

    public string Get(string key) => Settings.GetValueOrDefault(key, "");

    public bool HasTrigger(NotificationEventType type) => type switch
    {
        NotificationEventType.Started => OnStarted,
        NotificationEventType.Completed => OnCompleted,
        NotificationEventType.Failed => OnFailed,
        _ => false
    };
}

public enum NotificationEventType
{
    Test,
    Started,
    Completed,
    Failed
}
