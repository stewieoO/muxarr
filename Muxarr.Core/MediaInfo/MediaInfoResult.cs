using System.Text.Json.Serialization;

namespace Muxarr.Core.MediaInfo;

public class MediaInfoResult
{
    [JsonPropertyName("media")]
    public MediaInfoMedia? Media { get; set; }
}

public class MediaInfoMedia
{
    [JsonPropertyName("track")]
    public List<MediaInfoTrack> Tracks { get; set; } = [];
}

public class MediaInfoTrack
{
    [JsonPropertyName("@type")]
    public string? Type { get; set; }

    /// <summary>
    /// 0-based absolute stream index. Matches ffprobe's stream.index and
    /// MediaTrack.TrackNumber. File-level (General) tracks don't have one.
    /// </summary>
    [JsonPropertyName("StreamOrder")]
    public string? StreamOrder { get; set; }

    [JsonPropertyName("Title")]
    public string? Title { get; set; }
}
