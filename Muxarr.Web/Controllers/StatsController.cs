using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muxarr.Core.Api.Models;
using Muxarr.Core.Config;
using Muxarr.Data;
using Muxarr.Data.Entities;
using Muxarr.Data.Extensions;
using Muxarr.Web.Authentication;

namespace Muxarr.Web.Controllers;

[Authorize(AuthenticationSchemes = AuthSchemes.ApiKey)]
public class StatsController(IDbContextFactory<AppDbContext> contextFactory) : Controller
{
    [HttpGet]
    [Route("~/api/stats")]
    public async Task<IActionResult> Get()
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var stats = await context.Configs.GetAsync<LibraryStatsConfig>();
        var activeConversions = await context.MediaConversions.CountAsync(c => c.State == ConversionState.Processing);
        var queuedConversions = await context.MediaConversions.CountAsync(c => c.State == ConversionState.New);
        var failedConversions = await context.MediaConversions.CountAsync(c => c.State == ConversionState.Failed);

        var lastConversionAt = await context.MediaConversions
            .Where(c => c.State == ConversionState.Completed)
            .OrderByDescending(c => c.CreatedDate)
            .Select(c => (DateTime?)c.CreatedDate)
            .FirstOrDefaultAsync();

        var lastFileAddedAt = await context.MediaFiles
            .OrderByDescending(f => f.CreatedDate)
            .Select(f => (DateTime?)f.CreatedDate)
            .FirstOrDefaultAsync();

        return Ok(new StatsResponse
        {
            TotalFiles = stats?.TotalFiles ?? 0,
            TotalSizeBytes = stats?.TotalSizeBytes ?? 0,
            ActiveConversions = activeConversions,
            QueuedConversions = queuedConversions,
            CompletedConversions = stats?.TotalConversions ?? 0,
            FailedConversions = failedConversions,
            SpaceSavedBytes = stats?.SpaceSavedBytes ?? 0,
            LastConversionAt = lastConversionAt,
            LastFileAddedAt = lastFileAddedAt,
        });
    }
}
