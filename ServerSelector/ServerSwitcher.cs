//#define GameAssemblyNeedsPatch // remove if running on versions before v124 or on v137+
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ServerSelector
{
    public class ServerSwitcher
    {
        private static readonly string[] GameAssemblySodiumIntegrityFuncHint = ["40 53 56 57 41 54 41 55 41 56 41 57 48 81 EC C0 00 00 00 80 3d ?? ?? ?? ?? 00 0f 85 ?? 00 00 00 48"];
        private static readonly string[] GameAssemblySodiumIntegrityFuncPatched = ["b0 01 c3 90 90 54 41 55 41 56 41 57 48 81 EC C0 00 00 00 80 3d ?? ?? ?? ?? 00 0f 85 ?? 00 00 00 48"];
        private static readonly string[] GameAssemblySodiumIntegrityFuncPatch = ["b0 01 c3 90 90"];
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

            string sodiumLib = AppDomain.CurrentDomain.BaseDirectory + "sodium.dll";
            string gameSodium = gamePath + "/nikke_Data/Plugins/x86_64/sodium.dll";
            string gameAssembly = gamePath + "/GameAssembly.dll";
            string sodiumBackup = gameSodium + ".bak";
            string hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");

            if (OperatingSystem.IsLinux())
            {
                // for wine
                hostsFilePath = gamePath + "/../../../windows/system32/drivers/etc/hosts";
            }

            string launcherCertList = launcherPath + "/intl_service/cacert.pem";
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

        public static bool PatchGameAssembly(string dll, bool install)
        {
            // v124 introduced check to ensure that sodium dll is not changed.
#if !GameAssemblyNeedsPatch
            return true;
#else
            string backupPath = dll + ".bak";
            bool backupExists = File.Exists(backupPath);

            /*
                Official->Local (Install):
                    - Check if backup file exists:
                        - Game was updated, delete backup file
                    - Check if game assembly is not patched:
                        - Patch game assembly
                Local->Official (Uninstall):
                    - Check if game assembly is not patched:
                        - Game updated, delete backup and return
                    - Check if backup exists
                        - Restore DLL
            */

            if (install)
            {
                // todo reuse offset value
                if (backupExists && PatchUtility.CanFindOffset(dll, GameAssemblySodiumIntegrityFuncHint))
                {
                    // game was likely updated, delete backup
                    File.Delete(backupPath);
                }

                if (PatchUtility.CanFindOffset(dll, GameAssemblySodiumIntegrityFuncPatched))
                {
                    // already patched
                    return true;
                }

                // patch assembly
                return PatchUtility.SearchAndReplace(dll, GameAssemblySodiumIntegrityFuncHint, GameAssemblySodiumIntegrityFuncPatch);
            }
            else
            {
                if (PatchUtility.CanFindOffset(dll, GameAssemblySodiumIntegrityFuncHint))
                {
                    // game was updated, delete backup
                    if (backupExists) File.Delete(backupPath);
                    return true;
                }

                // restore backup if it exists
                if (backupExists)
                {
                    File.Move(backupPath, dll, true);
                }
            }

            return true;
#endif
        }

        public static async Task<ServerSwitchResult> SaveCfg(bool useOffical, string gamePath, string? launcherPath, string ip, bool offlineMode)
        {
            string sodiumLib = AppDomain.CurrentDomain.BaseDirectory + "sodium.dll";
            string gameSodium = gamePath + "/nikke_Data/Plugins/x86_64/sodium.dll";
            string gameAssembly = gamePath + "/GameAssembly.dll";
            string sodiumBackup = gameSodium + ".bak";
            string hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            string CAcert = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "myCA.pem");

            string launcherCertList = launcherPath + "/intl_service/cacert.pem";
            string gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/intl_cacert.pem";
            if (!File.Exists(gameCertList))
                gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/cacert.pem"; // older INTL sdk versions
            bool supported = true;

            if (OperatingSystem.IsLinux())
            {
                // for wine
                hostsFilePath = gamePath + "/../../../windows/system32/drivers/etc/hosts";
            }


            // TODO: allow changing ip address
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

                if (!PatchGameAssembly(gameAssembly, false))
                {
                    supported = false;
                }

                if (File.Exists(launcherCertList))
                {
                    string certList1 = await File.ReadAllTextAsync(launcherCertList);

                    int goodSslIndex1 = certList1.IndexOf("Good SSL Ca");
                    if (goodSslIndex1 != -1)
                        await File.WriteAllTextAsync(launcherCertList, certList1[..goodSslIndex1]);
                }

                if (File.Exists(gameCertList))
                {
                    string certList2 = await File.ReadAllTextAsync(gameCertList);

                    int goodSslIndex2 = certList2.IndexOf("Good SSL Ca");
                    if (goodSslIndex2 != -1)
                        await File.WriteAllTextAsync(gameCertList, certList2[..goodSslIndex2]);
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
                catch
                {

                }

                // update sodium lib

                if (!File.Exists(gameSodium))
                {
                    throw new Exception("expected sodium library to exist at path " + gameSodium);
                }

                // copy backup if sodium size is correct
                byte[] sod = await File.ReadAllBytesAsync(gameSodium);
                if (sod.Length <= 307200)
                {
                    // orignal file size, copy backup
                    await File.WriteAllBytesAsync(sodiumBackup, sod);
                }

                // write new sodium library
                await File.WriteAllBytesAsync(gameSodium, await File.ReadAllBytesAsync(sodiumLib));

                // patch gameassembly to remove sodium IntegrityUtility Check introduced in v124.6.10
                supported = PatchGameAssembly(gameAssembly, true);

                // update launcher/game ca cert list

                if (launcherPath != null)
                {
                    string certList1 = await File.ReadAllTextAsync(launcherCertList);
                    certList1 += "\nGood SSL Ca\n===============================\n";
                    certList1 += CAcert;
                    await File.WriteAllTextAsync(launcherCertList, certList1);
                }

                string certList2 = await File.ReadAllTextAsync(gameCertList);
                certList2 += "\nGood SSL Ca\n===============================\n";
                certList2 += CAcert;
                await File.WriteAllTextAsync(gameCertList, certList2);
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
