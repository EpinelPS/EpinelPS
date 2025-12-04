//#define GameAssemblyNeedsPatch // remove if running on versions before v124 or on v137+
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ServerSelector
{
    public class ServerSwitcher
    {
        private const string HostsStartMarker = "# begin ServerSelector entries";
        private const string HostsEndMarker = "# end ServerSelector entries";

        public static bool IsUsingOfficalServer()
        {
            string hostsFile = File.ReadAllText(OperatingSystem.IsWindows() ? "C:\\Windows\\System32\\drivers\\etc\\hosts" : "/etc/hosts");
            return !hostsFile.Contains("global-lobby.nikke-kr.com");
        }

        public static bool IsOffline()
        {
            string hostsFile = File.ReadAllText(OperatingSystem.IsWindows() ? "C:\\Windows\\System32\\drivers\\etc\\hosts" : "/etc/hosts");
            return hostsFile.Contains("cloud.nikke-kr.com");
        }

        public static async Task<string> CheckIntegrity(string gamePath, string launcherPath)
        {
            if (IsUsingOfficalServer())
                return "Official server";

            if (!Directory.Exists(gamePath))
            {
                return "Game path does not exist";
            }

            if (!Directory.Exists(launcherPath))
            {
                return "Launcher path does not exist";
            }

            if (!File.Exists(Path.Combine(launcherPath, "nikke_launcher.exe")))
            {
                return "Launcher path is invalid. Make sure that the game executable exists in the launcher folder";
            }


            // TODO fix this mess
            string launcherCertList = launcherPath + "/intl_service/intl_cacert.pem";
            if (!File.Exists(launcherCertList))
                launcherCertList = launcherPath + "/intl_service/cacert.pem"; // older INTL sdk versions
            string gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/intl_cacert.pem";
            if (!File.Exists(gameCertList))
                gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/cacert.pem"; // older INTL sdk versions

            if (File.Exists(launcherCertList))
            {
                string certList1 = await File.ReadAllTextAsync(launcherCertList);

                if (!certList1.Contains("Good SSL Ca"))
                    return "Patch missing";
            }

            if (File.Exists(gameCertList))
            {
                string certList2 = await File.ReadAllTextAsync(gameCertList);

                if (!certList2.Contains("Good SSL Ca"))
                    return "Patch missing";
            }

            // TODO: Check sodium lib
            // TODO: Check if gameassembly was patched
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

        public static async Task<ServerSwitchResult> SaveCfg(bool useOffical, string gamePath, string? launcherPath, string ip, bool offlineMode)
        {
            string sodiumLib = AppDomain.CurrentDomain.BaseDirectory + "sodium.dll";
            string gameSodium = gamePath + "/nikke_Data/Plugins/x86_64/sodium.dll";
            string gameAssembly = gamePath + "/GameAssembly.dll";
            string sodiumBackup = gameSodium + ".bak";
            string hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            string CAcert = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "myCA.pem");

            string launcherCertList = launcherPath + "/intl_service/intl_cacert.pem";
             if (!File.Exists(launcherCertList))
                launcherCertList = launcherPath + "/intl_service/cacert.pem"; // older INTL sdk versions
            string gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/intl_cacert.pem";
            if (!File.Exists(gameCertList))
                gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/cacert.pem"; // older INTL sdk versions
            bool supported = true;

            if (OperatingSystem.IsLinux())
            {
                // for wine
                hostsFilePath = gamePath + "/../../../windows/system32/drivers/etc/hosts";
            }

            if (useOffical)
            {
                await RevertHostsFile(hostsFilePath);
                if (OperatingSystem.IsLinux())
                {
                    await RevertHostsFile("/etc/hosts");
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
                if (!File.Exists(sodiumBackup))
                {
                    throw new Exception("sodium backup does not exist. Repair the game in the launcher and switch to local server and back to official.");
                }
                File.Copy(sodiumBackup, gameSodium, true);

                if (File.Exists(launcherCertList))
                {
                    string certList = await File.ReadAllTextAsync(launcherCertList);

                    int goodSslIndex1 = certList.IndexOf("Good SSL Ca");
                    if (goodSslIndex1 != -1)
                        await File.WriteAllTextAsync(launcherCertList, certList[..goodSslIndex1]);
                }

                if (File.Exists(gameCertList))
                {
                    string certList = await File.ReadAllTextAsync(gameCertList);

                    int newCertIndex = certList.IndexOf("Good SSL Ca");
                    if (newCertIndex != -1)
                        await File.WriteAllTextAsync(gameCertList, certList[..newCertIndex]);
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

                await RevertHostsFile(hostsFilePath);

                try
                {
                    FileInfo fi = new(hostsFilePath);
                    if (fi.IsReadOnly)
                    {
                        // try to remove readonly flag
                        fi.IsReadOnly = false;
                    }

                    if (!(await File.ReadAllTextAsync(hostsFilePath)).Contains("global-lobby.nikke-kr.com"))
                    {
                        using StreamWriter w = File.AppendText(hostsFilePath);
                        w.WriteLine();
                        w.Write(hosts);
                    }
                }
                catch
                {
                    throw new Exception("cannot modify C:\\Windows\\System32\\drivers\\etc\\hosts file to redirect to server, check your antivirus software");
                }

                // Also change /etc/hosts if running on linux
                if (OperatingSystem.IsLinux())
                {
                    hostsFilePath = "/etc/hosts";
                    await RevertHostsFile(hostsFilePath);
                    if (!(await File.ReadAllTextAsync(hostsFilePath)).Contains("global-lobby.nikke-kr.com"))
                    {
                        using StreamWriter w = File.AppendText(hostsFilePath);
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

                if (!File.Exists(gameSodium))
                {
                    throw new Exception("expected sodium library to exist at path " + gameSodium);
                }

                // copy backup if sodium size is correct
                byte[] sod = await File.ReadAllBytesAsync(gameSodium);
                if (sod.Length <= 307200) // TODO this is awful
                {
                    // orignal file size, copy backup
                    await File.WriteAllBytesAsync(sodiumBackup, sod);
                }

                // write new sodium library
                await File.WriteAllBytesAsync(gameSodium, await File.ReadAllBytesAsync(sodiumLib));

                // Add generated CA certificate to launcher/game curl certificate list
                if (launcherPath != null)
                {
                    await File.WriteAllTextAsync(launcherCertList,
                        await File.ReadAllTextAsync(launcherCertList)
                        + "\nGood SSL Ca\n===============================\n"
                        + CAcert);
                }

                await File.WriteAllTextAsync(gameCertList,
                    await File.ReadAllTextAsync(gameCertList)
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
}
