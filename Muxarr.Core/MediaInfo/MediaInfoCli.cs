using Muxarr.Core.Utilities;

namespace Muxarr.Core.MediaInfo;

/// <summary>
/// Wrapper around the mediainfo CLI. Scanner uses it to read per-track MP4
/// titles that libavformat &lt; 8.0 doesn't expose through ffprobe.
/// </summary>
public static class MediaInfoCli
{
    internal const string MediaInfoExecutable = "mediainfo";

    public static async Task<ProcessJsonResult<MediaInfoResult>> GetTrackInfo(string file)
    {
        var result = await ProcessExecutor.ExecuteProcessAsync(
            MediaInfoExecutable,
            $"--Output=JSON \"{file}\"",
            TimeSpan.FromSeconds(30));

        var json = new ProcessJsonResult<MediaInfoResult>(result);

        if (!result.Success || string.IsNullOrEmpty(result.Output))
        {
            return json;
        }

        try
        {
            json.Result = JsonHelper.Deserialize<MediaInfoResult>(result.Output);
        }
        catch (Exception e)
        {
            result.Error = e.ToString();
        }

        return json;
    }
}
