using System.Collections.Concurrent;
using System.Diagnostics;

namespace EpinelPS.Utils;

/// <summary>
/// Cutscene Video Optimizer:
/// Automatically detects and transcodes cutscene MP4 videos to H.264 Baseline Profile with BT.709 color matrix,
/// constant frame rate (CFR), and +faststart atom placement.
/// 
/// This completely resolves Unity WindowsVideoMedia playback errors:
/// - "WindowsVideoMedia error unhandled Color Standard: 0 falling back to default"
/// - "Unexpected timestamp values detected. This can occur in H.264 videos not encoded with the baseline profile"
/// 
/// Employs fast-path (.optimized marker), per-file concurrency locks, atomic file replacement with backup (.orig),
/// and non-breaking graceful fallback if FFmpeg is absent.
/// </summary>
public static class CutsceneOptimizer
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new(StringComparer.OrdinalIgnoreCase);
    private static string? _cachedFfmpegPath;
    private static bool _ffmpegChecked;
    private static readonly object PathLock = new();

    /// <summary>
    /// Checks whether FFmpeg executable is available, probing in order:
    /// 1. GameConfig.Root.FfmpegPath
    /// 2. FFMPEG_PATH environment variable
    /// 3. System and User PATH directories
    /// 4. WinGet package installation directory
    /// 5. Local tools/ directory relative to application root
    /// </summary>
    public static bool IsFfmpegAvailable(out string? resolvedPath)
    {
        lock (PathLock)
        {
            if (_ffmpegChecked)
            {
                resolvedPath = _cachedFfmpegPath;
                return !string.IsNullOrEmpty(resolvedPath);
            }

            resolvedPath = ResolveFfmpegPathInternal();
            _cachedFfmpegPath = resolvedPath;
            _ffmpegChecked = true;

            if (!string.IsNullOrEmpty(resolvedPath))
            {
                Logging.WriteLine($"[CutsceneOptimizer] FFmpeg binary discovered at: {resolvedPath}", LogType.Info);
            }
            else
            {
                Logging.WriteLine("[CutsceneOptimizer] FFmpeg binary not found. Cutscene optimization disabled; original MP4 files will be served directly.", LogType.Warning);
            }

            return !string.IsNullOrEmpty(resolvedPath);
        }
    }

    /// <summary>
    /// Resets the cached FFmpeg path, forcing a new discovery probe on next access.
    /// </summary>
    public static void ResetPathCache()
    {
        lock (PathLock)
        {
            _cachedFfmpegPath = null;
            _ffmpegChecked = false;
        }
    }

    private static string? ResolveFfmpegPathInternal()
    {
        // 1. Check GameConfig
        string? configPath = GameConfig.Root.FfmpegPath;
        if (!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath))
        {
            return Path.GetFullPath(configPath);
        }

        // 2. Check FFMPEG_PATH environment variable
        string? envPath = Environment.GetEnvironmentVariable("FFMPEG_PATH");
        if (!string.IsNullOrWhiteSpace(envPath) && File.Exists(envPath))
        {
            return Path.GetFullPath(envPath);
        }

        // 3. Check system and user PATH
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            char sep = Path.PathSeparator;
            string[] dirs = pathEnv.Split(sep, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string dir in dirs)
            {
                try
                {
                    string candidate = Path.Combine(dir, OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg");
                    if (File.Exists(candidate))
                    {
                        return Path.GetFullPath(candidate);
                    }
                }
                catch
                {
                    // Ignore inaccessible PATH entries
                }
            }
        }

        // 4. Check WinGet package directory on Windows
        if (OperatingSystem.IsWindows())
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string wingetPackages = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages");
                if (Directory.Exists(wingetPackages))
                {
                    foreach (string pkgDir in Directory.EnumerateDirectories(wingetPackages, "*ffmpeg*", SearchOption.TopDirectoryOnly))
                    {
                        foreach (string file in Directory.EnumerateFiles(pkgDir, "ffmpeg.exe", SearchOption.AllDirectories))
                        {
                            if (File.Exists(file))
                            {
                                return Path.GetFullPath(file);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore WinGet probe errors
            }
        }

        // 5. Check local tools/ directory
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string[] localCandidates =
        [
            Path.Combine(baseDir, "ffmpeg.exe"),
            Path.Combine(baseDir, "tools", "ffmpeg.exe"),
            Path.Combine(baseDir, "tools", "ffmpeg", "bin", "ffmpeg.exe")
        ];

        foreach (string candidate in localCandidates)
        {
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(candidate);
            }
        }

        return null;
    }

    /// <summary>
    /// Ensures that the specified MP4 video file is transcoded to Unity-compliant H.264 Baseline Profile.
    /// Returns the file path to serve (optimized if successful, original if skipped or failed).
    /// </summary>
    public static async Task<string> OptimizeVideoAsync(string videoPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(videoPath) || !File.Exists(videoPath))
        {
            return videoPath;
        }

        if (!videoPath.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
        {
            return videoPath;
        }

        string markerPath = videoPath + ".optimized";
        if (File.Exists(markerPath))
        {
            // Fast-path: already transcoded, 0 ms overhead
            return videoPath;
        }

        if (!IsFfmpegAvailable(out string? ffmpegExe) || string.IsNullOrEmpty(ffmpegExe))
        {
            // Fallback: serve original directly without blocking
            return videoPath;
        }

        string normalizedPath = Path.GetFullPath(videoPath);
        SemaphoreSlim fileLock = FileLocks.GetOrAdd(normalizedPath, _ => new SemaphoreSlim(1, 1));

        await fileLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check fast-path after acquiring lock
            if (File.Exists(markerPath))
            {
                return videoPath;
            }

            string tempPath = videoPath + ".tmp.mp4";
            string backupPath = videoPath + ".orig";

            // Delete stale temp file if leftover from interrupted process
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { /* ignore */ }
            }

            Logging.WriteLine($"[CutsceneOptimizer] Optimizing cutscene {Path.GetFileName(videoPath)} to H.264 Baseline / BT.709...", LogType.Info);

            // FFmpeg arguments for maximum Unity WindowsVideoMedia compatibility & visual fidelity:
            // -c:v libx264 -profile:v baseline -level 4.1: Native 1080p H.264 Baseline profile
            // -pix_fmt yuv420p: Standard 8-bit YUV 4:2:0
            // -color_primaries bt709 -color_trc bt709 -colorspace bt709: Eliminates "Color Standard: 0" fallback
            // -x264-params colorprim=bt709:transfer=bt709:colormatrix=bt709: Encodes standard BT.709 VUI parameters
            // -fps_mode cfr -r 30: Constant frame rate to eliminate timestamp skew warnings
            // -crf 16 -preset slow: Visually lossless compression (~7.5 Mbps for 1080p) preserving high-frequency anime line art
            // -c:a aac -b:a 192k -ar 48000: High fidelity 48kHz AAC stereo audio
            // -movflags +faststart: Relocate moov atom to start of file for smooth HTTP range-streaming
            string arguments = $"-y -i \"{videoPath}\" -c:v libx264 -profile:v baseline -level 4.1 -pix_fmt yuv420p -color_primaries bt709 -color_trc bt709 -colorspace bt709 -x264-params colorprim=bt709:transfer=bt709:colormatrix=bt709 -fps_mode cfr -r 30 -crf 16 -preset slow -c:a aac -b:a 192k -ar 48000 -movflags +faststart \"{tempPath}\"";

            using Process process = new();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegExe,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            process.Start();

            // Asynchronously read stderr to prevent buffer deadlocks
            Task<string> stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* ignore */ }
                Logging.WriteLine($"[CutsceneOptimizer] Transcode timed out for {Path.GetFileName(videoPath)}.", LogType.Warning);
                if (File.Exists(tempPath)) { try { File.Delete(tempPath); } catch { /* ignore */ } }
                return videoPath;
            }

            string stderr = await stderrTask;

            if (process.ExitCode == 0 && File.Exists(tempPath) && new FileInfo(tempPath).Length > 0)
            {
                long oldSize = new FileInfo(videoPath).Length;
                long newSize = new FileInfo(tempPath).Length;

                // Backup original if not already backed up
                if (!File.Exists(backupPath))
                {
                    File.Move(videoPath, backupPath);
                }
                else
                {
                    File.Delete(videoPath);
                }

                // Atomically place new optimized video
                File.Move(tempPath, videoPath);

                // Create fast-path marker
                await File.WriteAllTextAsync(markerPath, $"optimized={DateTime.UtcNow:O}\norig_size={oldSize}\nnew_size={newSize}\n");

                Logging.WriteLine($"[CutsceneOptimizer] Successfully optimized {Path.GetFileName(videoPath)} ({oldSize} -> {newSize} bytes). Baseline profile & BT.709 active.", LogType.Info);
                return videoPath;
            }
            else
            {
                Logging.WriteLine($"[CutsceneOptimizer] FFmpeg exited with code {process.ExitCode} for {Path.GetFileName(videoPath)}: {stderr}", LogType.Warning);
                if (File.Exists(tempPath)) { try { File.Delete(tempPath); } catch { /* ignore */ } }
                return videoPath;
            }
        }
        catch (Exception ex)
        {
            Logging.WriteLine($"[CutsceneOptimizer] Exception during optimization of {Path.GetFileName(videoPath)}: {ex.Message}", LogType.Error);
            return videoPath;
        }
        finally
        {
            fileLock.Release();
        }
    }

    /// <summary>
    /// Scans a directory (defaulting to the server cache directory) for all unoptimized MP4 files
    /// and transcodes them in batch.
    /// </summary>
    public static async Task<(int optimized, int skipped, int failed)> OptimizeAllCachedVideosAsync(string? targetDir = null, CancellationToken cancellationToken = default)
    {
        string baseDir = string.IsNullOrWhiteSpace(targetDir)
            ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache")
            : targetDir;

        if (!Directory.Exists(baseDir))
        {
            return (0, 0, 0);
        }

        int optimized = 0;
        int skipped = 0;
        int failed = 0;

        var mp4Files = Directory.EnumerateFiles(baseDir, "*.mp4", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".tmp.mp4", StringComparison.OrdinalIgnoreCase) && !f.EndsWith(".orig", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (string file in mp4Files)
        {
            if (cancellationToken.IsCancellationRequested) break;

            string marker = file + ".optimized";
            if (File.Exists(marker))
            {
                skipped++;
                continue;
            }

            string result = await OptimizeVideoAsync(file, cancellationToken);
            if (File.Exists(marker))
            {
                optimized++;
            }
            else
            {
                failed++;
            }
        }

        return (optimized, skipped, failed);
    }
}
