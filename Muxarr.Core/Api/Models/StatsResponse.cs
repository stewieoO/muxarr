using Muxarr.Core.Extensions;

namespace Muxarr.Core.Api.Models;

public class StatsResponse
{
    public int TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public string TotalSize => TotalSizeBytes.DisplayFileSize();
    public int ActiveConversions { get; set; }
    public int QueuedConversions { get; set; }
    public int CompletedConversions { get; set; }
    public int FailedConversions { get; set; }
    public long SpaceSavedBytes { get; set; }
    public string SpaceSaved => SpaceSavedBytes.DisplayFileSize();
    public DateTime? LastConversionAt { get; set; }
    public DateTime? LastFileAddedAt { get; set; }

    public static StatsResponse Example => new()
    {
        TotalFiles = 1234,
        TotalSizeBytes = 5497558138880,
        ActiveConversions = 1,
        QueuedConversions = 3,
        CompletedConversions = 456,
        FailedConversions = 2,
        SpaceSavedBytes = 10737418240,
        LastConversionAt = new DateTime(2026, 4, 2, 10, 30, 0, DateTimeKind.Utc),
        LastFileAddedAt = new DateTime(2026, 4, 2, 12, 15, 0, DateTimeKind.Utc),
    };
}
