using System.ComponentModel.DataAnnotations;

namespace Muxarr.Data.Entities;

/// <summary>
/// Track flags that can have per-flag template overrides.
/// Checked in enum order - first matching flag wins.
/// </summary>
public enum TrackFlag
{
    [Display(Name = "SDH")]
    HearingImpaired,

    [Display(Name = "Forced")]
    Forced,

    [Display(Name = "Commentary")]
    Commentary,

    [Display(Name = "AD")]
    VisualImpaired
}

public static class TrackFlagExtensions
{
    public static readonly TrackFlag[] All = Enum.GetValues<TrackFlag>();

    public static bool Matches(this TrackFlag flag, IMediaTrack track) => flag switch
    {
        TrackFlag.HearingImpaired => track.IsHearingImpaired,
        TrackFlag.Forced => track.IsForced,
        TrackFlag.Commentary => track.IsCommentary,
        TrackFlag.VisualImpaired => track.IsVisualImpaired,
        _ => false
    };
}
