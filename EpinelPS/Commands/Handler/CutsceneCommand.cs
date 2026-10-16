using System.Text;
using EpinelPS.Commands.Binding;
using EpinelPS.Commands.Core;
using EpinelPS.Utils;

namespace EpinelPS.Commands.Handler;

public class CutsceneParameter : ICommandParameters
{
    public static ParameterDescriptor[] Descriptors => [
        Param.String(0, "action", "Action: info, optimize (or opt)", isOptional: true),
    ];

    public string Action { get; init; } = "info";
}

public class CutsceneHandler(IExecutionContext context) : BaseHandler<CutsceneParameter>(context)
{
    public override string Name => "cutscene";
    public override string Description => "Manage cutscene video optimization (info, optimize)";
    public override string[] Alias => ["cs"];

    protected override async Task<HandleResult> ExecuteAsync(CutsceneParameter parameters)
    {
        string action = (parameters.Action ?? "info").ToLowerInvariant();

        if (action == "info" || action == "status")
        {
            bool ffmpegOk = CutsceneOptimizer.IsFfmpegAvailable(out string? ffmpegPath);
            string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");

            int totalMp4 = 0;
            int optimized = 0;
            int unoptimized = 0;

            if (Directory.Exists(cacheDir))
            {
                var files = Directory.EnumerateFiles(cacheDir, "*.mp4", SearchOption.AllDirectories)
                    .Where(f => !f.EndsWith(".tmp.mp4", StringComparison.OrdinalIgnoreCase) && !f.EndsWith(".orig", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                totalMp4 = files.Count;
                optimized = files.Count(f => File.Exists(f + ".optimized"));
                unoptimized = totalMp4 - optimized;
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== Cutscene Video Status ===");
            sb.AppendLine($"FFmpeg Available   : {(ffmpegOk ? "YES" : "NO")}");
            sb.AppendLine($"FFmpeg Binary      : {ffmpegPath ?? "(not found)"}");
            sb.AppendLine($"Cached MP4 Videos  : {totalMp4}");
            sb.AppendLine($"Optimized (Baseline): {optimized}");
            sb.AppendLine($"Pending Optimization: {unoptimized}");

            return new HandleResult(true, sb.ToString().TrimEnd());
        }

        if (action == "optimize" || action == "opt")
        {
            if (!CutsceneOptimizer.IsFfmpegAvailable(out _))
            {
                return new HandleResult(false, "FFmpeg is not available. Please install FFmpeg (e.g. winget install Gyan.FFmpeg) or set FfmpegPath in gameconfig.json.");
            }

            var (opt, skipped, failed) = await CutsceneOptimizer.OptimizeAllCachedVideosAsync();
            return new HandleResult(true, $"Cutscene optimization complete: {opt} optimized, {skipped} already optimized, {failed} failed.");
        }

        return new HandleResult(false, "Usage: cutscene [info|optimize]");
    }
}
