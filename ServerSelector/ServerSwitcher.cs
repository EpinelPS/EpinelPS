//#define GameAssemblyNeedsPatch // remove if running on versions before v124 or on v137+
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ServerSelector;

public class ServerSwitcher
{
    private const string HostsStartMarker = "# begin ServerSelector entries";
    private const string HostsEndMarker = "# end ServerSelector entries";

    private static PathUtil util = new();

    public static bool IsUsingLocalServer()
    {
        return File.ReadAllText(util.SystemHostsFile).Contains("global-lobby.nikke-kr.com");
    }

    public static bool IsOffline()
    {
        return File.ReadAllText(util.SystemHostsFile).Contains("cloud.nikke-kr.com");
    }

    public static (bool, string?) SetBasePath(string basePath)
    {
        return util.SetBasePath(basePath);
    }

    public static async Task<string> CheckIntegrity()
    {
        if (!IsUsingLocalServer())
            return "Official server";

        if (File.Exists(util.LauncherCertificatePath))
        {
            string certList1 = await File.ReadAllTextAsync(util.LauncherCertificatePath);

            if (!certList1.Contains("Good SSL Ca"))
                return "SSL Cert Patch missing Launcher";
        }

        if (File.Exists(util.GameCertificatePath))
        {
            string certList2 = await File.ReadAllTextAsync(util.GameCertificatePath);

            if (!certList2.Contains("Good SSL Ca"))
                return "SSL Cert Patch missing Game";
        }

        // TODO: Check sodium lib
        // TODO: check hosts file

        return "OK";
    }

    public static async Task RevertHostsFile(string hostsFilePath)
    {
        string txt = await File.ReadAllTextAsync(hostsFilePath);

        // remove stuff
        try
        {

            int startIdx = txt.IndexOf(HostsStartMarker);
            int endIdx;
            if (startIdx == -1)
            {
                startIdx = txt.IndexOf("cloud.nikke-kr.com");
            }

            string endIndexStr = HostsEndMarker;
            if (!txt.Contains(endIndexStr))
            {
                // old code, find new line character before start index
                for (int i = startIdx - 1; i >= 0; i--)
                {
                    char c = txt[i];
                    if (c == '\n')
                    {
                        startIdx = i + 1;
                        break;
                    }
                }

                endIndexStr = "y.io";
                endIdx = txt.IndexOf(endIndexStr) + endIndexStr.Length;
            }
            else
            {
                // add/subtract 2 to take into account newline
                startIdx = txt.IndexOf(HostsStartMarker) - 2;
                endIdx = txt.IndexOf(endIndexStr) + endIndexStr.Length;
            }

            txt = string.Concat(txt.AsSpan(0, startIdx), txt.AsSpan(endIdx));


            await File.WriteAllTextAsync(hostsFilePath, txt);
        }
        catch
        {

        }
    }

    public static async Task<ServerSwitchResult> SaveCfg(bool useOffical, string ip, bool offlineMode)
    {
        string CAcert = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "myCA.pem");
        string sodiumLib = AppDomain.CurrentDomain.BaseDirectory + "sodium.dll";

        bool supported = true;
        if (useOffical)
        {
            await RevertHostsFile(util.SystemHostsFile);
            if (OperatingSystem.IsLinux())
            {
                await RevertHostsFile(util.WineHostsFile);
            }

            try
            {
                // remove cert
                if (OperatingSystem.IsWindows())
                {
                    X509Store store = new(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadWrite);
                    store.Remove(new X509Certificate2(X509Certificate.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "myCA.pfx")));
                    store.Close();
                }
            }
            catch
            {
                // may not be installed
            }

            // restore sodium
            if (!File.Exists(util.GameSodiumBackupPath))
            {
                throw new Exception("sodium backup does not exist. Repair the game in the launcher and switch to local server and back to official.");
            }
            File.Copy(util.GameSodiumBackupPath, util.GameSodiumPath, true);

            if (util.LauncherCertificatePath != null && File.Exists(util.LauncherCertificatePath))
            {
                string certList = await File.ReadAllTextAsync(util.LauncherCertificatePath);

                int goodSslIndex1 = certList.IndexOf("Good SSL Ca");
                if (goodSslIndex1 != -1)
                    await File.WriteAllTextAsync(util.LauncherCertificatePath, certList[..goodSslIndex1]);
            }

            if (File.Exists(util.GameCertificatePath))
            {
                string certList = await File.ReadAllTextAsync(util.GameCertificatePath);

                int newCertIndex = certList.IndexOf("Good SSL Ca");
                if (newCertIndex != -1)
                    await File.WriteAllTextAsync(util.GameCertificatePath, certList[..newCertIndex]);
            }
        }
        else
        {
            // add to hosts file
            string hosts = $@"{HostsStartMarker}
{ip} global-lobby.nikke-kr.com
";
            if (offlineMode)
            {
                hosts += $"{ip} cloud.nikke-kr.com" + Environment.NewLine;
            }

            hosts += $@"{ip} jp-lobby.nikke-kr.com
{ip} us-lobby.nikke-kr.com
{ip} kr-lobby.nikke-kr.com
{ip} sea-lobby.nikke-kr.com
{ip} hmt-lobby.nikke-kr.com
{ip} aws-na-dr.intlgame.com
{ip} sg-vas.intlgame.com
{ip} aws-na.intlgame.com
{ip} na-community.playerinfinite.com
{ip} common-web.intlgame.com
{ip} li-sg.intlgame.com
255.255.221.21 na.fleetlogd.com
{ip} www.jupiterlauncher.com
{ip} data-aws-na.intlgame.com
255.255.221.21 sentry.io
{HostsEndMarker}";

            await RevertHostsFile(util.SystemHostsFile);

            try
            {
                FileInfo fi = new(util.SystemHostsFile);
                if (fi.IsReadOnly)
                {
                    // try to remove readonly flag
                    fi.IsReadOnly = false;
                }

                if (!(await File.ReadAllTextAsync(util.SystemHostsFile)).Contains("global-lobby.nikke-kr.com"))
                {
                    using StreamWriter w = File.AppendText(util.SystemHostsFile);
                    w.WriteLine();
                    w.Write(hosts);
                }
            }
            catch
            {
                throw new Exception($"cannot modify \"{util.SystemHostsFile}\" file to redirect to server, check your antivirus software");
            }

            // Also change hosts file in wineprefix if running on linux
            if (OperatingSystem.IsLinux())
            {
                await RevertHostsFile(util.WineHostsFile);
                if (!(await File.ReadAllTextAsync(util.WineHostsFile)).Contains("global-lobby.nikke-kr.com"))
                {
                    using StreamWriter w = File.AppendText(util.WineHostsFile);
                    w.WriteLine();
                    w.Write(hosts);
                }
            }

            // trust CA. TODO is this needed?
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    X509Store store = new(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "myCA.pfx")));
                    store.Close();
                }
            }
            catch { }

            if (!File.Exists(util.GameSodiumPath))
            {
                throw new Exception("expected sodium library to exist at path " + util.GameSodiumPath);
            }

            // copy backup if sodium size is correct
            byte[] sod = await File.ReadAllBytesAsync(util.GameSodiumPath);
            if (sod.Length <= 307200) // TODO this is awful
            {
                // orignal file size, copy backup
                await File.WriteAllBytesAsync(util.GameSodiumBackupPath, sod);
            }

            // write new sodium library
            await File.WriteAllBytesAsync(util.GameSodiumPath, await File.ReadAllBytesAsync(sodiumLib));

            // Add generated CA certificate to launcher/game curl certificate list
            if (util.LauncherCertificatePath != null)
            {
                await File.WriteAllTextAsync(util.LauncherCertificatePath,
                    await File.ReadAllTextAsync(util.LauncherCertificatePath)
                    + "\nGood SSL Ca\n===============================\n"
                    + CAcert);
            }

            await File.WriteAllTextAsync(util.GameCertificatePath,
                await File.ReadAllTextAsync(util.GameCertificatePath)
                + "\nGood SSL Ca\n===============================\n"
                + CAcert);
        }

        return new ServerSwitchResult(true, null, supported);
    }
}

public class ServerSwitchResult(bool ok, Exception? exception, bool isSupported)
{
    public bool Ok { get; set; } = ok;
    public Exception? Exception { get; set; } = exception;
    public bool IsSupported { get; set; } = isSupported;
}
