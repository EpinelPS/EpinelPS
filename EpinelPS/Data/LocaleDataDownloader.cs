using System.Text;
using EpinelPS.Utils;

namespace EpinelPS.Data;

public static class LocaleDataDownloader
{
    private const string Platform = "StandaloneWindows64";
    private const string VersionFileName = ".saus-version";
    private static readonly string[] RequiredFiles =
    [
        "Locale_Bgm.lsc",
        "Locale_Character.lsc",
        "Locale_CharacterCostume.lsc",
        "Locale_Item.lsc"
    ];
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(60) };

    public static async Task DownloadAsync(CancellationToken cancellationToken)
    {
        var dataPackVersion = GameConfig.Root.ResourceDataPackVersion;
        if (string.IsNullOrWhiteSpace(dataPackVersion))
            throw new InvalidOperationException("ResourceDataPackVersion is not configured");

        var platformRoot = GameConfig.Root.ResourceBaseURL
            .Replace("{Platform}", Platform, StringComparison.OrdinalIgnoreCase)
            .TrimEnd('/');
        if (string.IsNullOrWhiteSpace(platformRoot))
            throw new InvalidOperationException("ResourceBaseURL is not configured");

        var packageRoot = $"{platformRoot}/pck/";
        var latest = await Client.GetStringAsync($"{packageRoot}latest-{dataPackVersion}.txt", cancellationToken);
        var saus = ParseSausEntry(latest);
        var localeDirectory = Path.Combine(AppContext.BaseDirectory, "cache", "local-locale");
        Directory.CreateDirectory(localeDirectory);

        var versionPath = Path.Combine(localeDirectory, VersionFileName);
        if (File.Exists(versionPath)
            && string.Equals(await File.ReadAllTextAsync(versionPath, cancellationToken), saus.Tag, StringComparison.Ordinal)
            && RequiredFiles.All(file => File.Exists(Path.Combine(localeDirectory, file))))
            return;

        var remoteRoot = $"{packageRoot}saus/{saus.Revision}/lss/";
        foreach (var fileName in RequiredFiles)
            await DownloadAtomicallyAsync(new Uri(remoteRoot + fileName), Path.Combine(localeDirectory, fileName), cancellationToken);

        await File.WriteAllTextAsync(versionPath, saus.Tag, Encoding.UTF8, cancellationToken);
    }

    private static SausEntry ParseSausEntry(string latest)
    {
        foreach (var rawLine in latest.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!rawLine.StartsWith("saus:", StringComparison.OrdinalIgnoreCase)) continue;
            var values = rawLine[5..].Split(',', 2, StringSplitOptions.TrimEntries);
            if (values.Length == 2 && values.All(value => !string.IsNullOrWhiteSpace(value)))
                return new SausEntry(values[0], values[1]);
        }
        throw new InvalidDataException("The resource version file does not contain a SAUS entry");
    }

    private static async Task DownloadAtomicallyAsync(Uri source, string target, CancellationToken cancellationToken)
    {
        var temporary = target + ".download";
        try
        {
            using var response = await Client.GetAsync(source, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using (var input = await response.Content.ReadAsStreamAsync(cancellationToken))
            await using (var output = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
                await input.CopyToAsync(output, cancellationToken);

            await using (var downloaded = File.OpenRead(temporary))
            {
                Span<byte> magic = stackalloc byte[4];
                if (downloaded.Read(magic) != magic.Length || !magic.SequenceEqual("NKDB"u8))
                    throw new InvalidDataException($"Downloaded {Path.GetFileName(target)} is not an NKDB file");
            }
            File.Move(temporary, target, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private sealed record SausEntry(string Revision, string Tag);
}
