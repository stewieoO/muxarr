using Muxarr.Core.MediaInfo;
using Muxarr.Core.Utilities;
using Muxarr.Data.Entities;
using Muxarr.Data.Extensions;

namespace Muxarr.Tests;

[TestClass]
public class MediaInfoCliTests
{
    private static readonly string SourceFixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "test.mkv");
    private string _mp4Fixture = null!;

    [TestInitialize]
    public async Task Setup()
    {
        Assert.IsTrue(File.Exists(SourceFixture));
        _mp4Fixture = Path.Combine(Path.GetTempPath(), $"muxarr_mi_{Guid.NewGuid():N}.mp4");

        var args =
            $"-y -hide_banner -loglevel error -i \"{SourceFixture}\" -map 0:v -map 0:a -map 0:s " +
            $"-c:v copy -c:a copy -c:s mov_text " +
            $"-metadata:s:0 title=\"Video 1080p\" " +
            $"-metadata:s:1 title=\"Surround 5.1\" " +
            $"-metadata:s:2 title=\"DTS-HD MA 5.1\" " +
            $"-metadata:s:3 title=\"English SDH\" " +
            $"-metadata:s:4 title=\"Nederlands voor doven en slechthorenden\" " +
            $"-movflags +use_metadata_tags -f mp4 \"{_mp4Fixture}\"";
        var gen = await ProcessExecutor.ExecuteProcessAsync("ffmpeg", args, TimeSpan.FromSeconds(30));
        Assert.IsTrue(gen.Success, $"Failed to generate MP4 fixture: {gen.Error}");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_mp4Fixture))
        {
            File.Delete(_mp4Fixture);
        }
    }

    [TestMethod]
    public async Task GetTrackInfo_ReturnsPerTrackTitlesInStreamOrder()
    {
        var mi = await MediaInfoCli.GetTrackInfo(_mp4Fixture);
        Assert.IsTrue(mi.Success, $"mediainfo failed: {mi.Error}");
        Assert.IsNotNull(mi.Result?.Media);

        var byOrder = mi.Result.Media.Tracks
            .Where(t => !string.IsNullOrEmpty(t.StreamOrder))
            .ToDictionary(t => int.Parse(t.StreamOrder!), t => t.Title);

        Assert.AreEqual("Video 1080p", byOrder[0]);
        Assert.AreEqual("Surround 5.1", byOrder[1]);
        Assert.AreEqual("DTS-HD MA 5.1", byOrder[2]);
        Assert.AreEqual("English SDH", byOrder[3]);
        Assert.AreEqual("Nederlands voor doven en slechthorenden", byOrder[4]);
    }

    [TestMethod]
    public void OverlayMediaInfoTitles_FillsNullTracksOnly()
    {
        var file = new MediaFile
        {
            Tracks =
            [
                new MediaTrack { Type = MediaTrackType.Video, TrackNumber = 0, TrackName = null },
                new MediaTrack { Type = MediaTrackType.Audio, TrackNumber = 1, TrackName = "Existing" },
                new MediaTrack { Type = MediaTrackType.Subtitles, TrackNumber = 3, TrackName = null }
            ]
        };

        var mi = new MediaInfoResult
        {
            Media = new MediaInfoMedia
            {
                Tracks =
                [
                    new MediaInfoTrack { StreamOrder = "0", Title = "FromMediaInfo" },
                    new MediaInfoTrack { StreamOrder = "1", Title = "ShouldBeIgnored" },
                    new MediaInfoTrack { StreamOrder = "3", Title = "SubFromMediaInfo" }
                ]
            }
        };

        file.OverlayMediaInfoTitles(mi);

        Assert.AreEqual("FromMediaInfo", file.Tracks.ElementAt(0).TrackName);
        Assert.AreEqual("Existing", file.Tracks.ElementAt(1).TrackName);
        Assert.AreEqual("SubFromMediaInfo", file.Tracks.ElementAt(2).TrackName);
    }

    [TestMethod]
    public void OverlayMediaInfoTitles_CorrelatesByStreamOrderNotPosition()
    {
        var file = new MediaFile
        {
            Tracks =
            [
                new MediaTrack { Type = MediaTrackType.Subtitles, TrackNumber = 3, TrackName = null }
            ]
        };

        var mi = new MediaInfoResult
        {
            Media = new MediaInfoMedia
            {
                Tracks =
                [
                    // File-level (General) track with no StreamOrder — must be skipped.
                    new MediaInfoTrack(),
                    new MediaInfoTrack { StreamOrder = "0", Title = "Ignored" },
                    new MediaInfoTrack { StreamOrder = "3", Title = "Hit" }
                ]
            }
        };

        file.OverlayMediaInfoTitles(mi);
        Assert.AreEqual("Hit", file.Tracks.First().TrackName);
    }

    [TestMethod]
    public void OverlayMediaInfoTitles_NullInput_NoOp()
    {
        var file = new MediaFile
        {
            Tracks = [new MediaTrack { Type = MediaTrackType.Video, TrackNumber = 0, TrackName = null }]
        };

        file.OverlayMediaInfoTitles(new MediaInfoResult());
        Assert.IsNull(file.Tracks.First().TrackName);
    }

    [TestMethod]
    public async Task SetFileDataFromFFprobe_ReadsMp4TitlesOnEveryFFmpegVersion()
    {
        // ffmpeg 8.0+ reads titles directly, older versions go through the
        // mediainfo fallback. Both paths must populate TrackName.
        var file = new MediaFile { Path = _mp4Fixture };
        await file.SetFileDataFromFFprobe();

        var tracks = file.Tracks.OrderBy(t => t.TrackNumber).ToList();
        Assert.AreEqual("Video 1080p", tracks[0].TrackName);
        Assert.AreEqual("Surround 5.1", tracks[1].TrackName);
        Assert.AreEqual("DTS-HD MA 5.1", tracks[2].TrackName);
        Assert.AreEqual("English SDH", tracks[3].TrackName);
        Assert.AreEqual("Nederlands voor doven en slechthorenden", tracks[4].TrackName);
    }
}
