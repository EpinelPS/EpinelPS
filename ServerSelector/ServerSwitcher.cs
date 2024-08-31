using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ServerSelector
{
    public class ServerSwitcher
    {
        private static int GameAssemblySodiumIntegrityFuncHint = 0x5877FFB;
        private static byte[] GameAssemblySodiumIntegrityFuncOrg = [0xE8, 0xF0, 0x9D, 0x43, 0xFB];
        private static byte[] GameAssemblySodiumIntegrityFuncPatch = [0xb0, 0x01, 0x90, 0x90, 0x90];
        private const string HostsStartMarker = "# begin ServerSelector entries";
        private const string HostsEndMarker = "# end ServerSelector entries";

        public static bool IsUsingOfficalServer()
        {
            var hostsFile = File.ReadAllText(OperatingSystem.IsWindows() ? "C:\\Windows\\System32\\drivers\\etc\\hosts" : "/etc/hosts");
            return !hostsFile.Contains("global-lobby.nikke-kr.com");
        }

        public static bool IsOffline()
        {
            var hostsFile = File.ReadAllText(OperatingSystem.IsWindows() ? "C:\\Windows\\System32\\drivers\\etc\\hosts" : "/etc/hosts");
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

            var CAcert = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "myCA.pem");

            string launcherCertList = launcherPath + "/intl_service/cacert.pem";
            string gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/cacert.pem";

            var certList1 = await File.ReadAllTextAsync(launcherCertList);

            int goodSslIndex1 = certList1.IndexOf("Good SSL Ca");
            if (goodSslIndex1 == -1)
                return "Patch missing";

            var certList2 = await File.ReadAllTextAsync(gameCertList);

            int goodSslIndex2 = certList2.IndexOf("Good SSL Ca");
            if (goodSslIndex2 == -1)
                return "Patch missing";


            // TODO: Check sodium lib
            // TODO: Check if gameassembly was patched

            return "OK";
        }

        public static async Task RevertHostsFile(string hostsFilePath)
        {
            var txt = await File.ReadAllTextAsync(hostsFilePath);

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
                        var c = txt[i];
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

                txt = txt.Substring(0, startIdx) + txt.Substring(endIdx);


                await File.WriteAllTextAsync(hostsFilePath, txt);
            }
            catch
            {

            }
        }

        public static async Task SaveCfg(bool useOffical, string gamePath, string launcherPath, string ip, bool offlineMode)
        {
            string sodiumLib = AppDomain.CurrentDomain.BaseDirectory + "sodium.dll";
            string gameSodium = gamePath + "/nikke_Data/Plugins/x86_64/sodium.dll";
            string gameAssembly = gamePath + "/GameAssembly.dll";
            string sodiumBackup = gameSodium + ".bak";
            string hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            var CAcert = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "myCA.pem");

            string launcherCertList = launcherPath + "/intl_service/cacert.pem";
            string gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/cacert.pem";


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

                // remove cert
                if (OperatingSystem.IsWindows())
                {
                    X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadWrite);
                    store.Remove(new X509Certificate2(X509Certificate.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "myCA.pfx")));
                    store.Close();
                }

                // restore sodium
                if (!File.Exists(sodiumBackup))
                {
                    throw new Exception("sodium backup does not exist");
                }
                File.Copy(sodiumBackup, gameSodium, true);

                // revert gameassembly changes
                var gameAssemblyBytes = await File.ReadAllBytesAsync(gameAssembly);
                for (int i = 0x5877FFB; i < gameAssemblyBytes.Length; i++)
                {
                    if (gameAssemblyBytes[i] == GameAssemblySodiumIntegrityFuncOrg[0] &&
           gameAssemblyBytes[i + 1] == GameAssemblySodiumIntegrityFuncOrg[1] &&
           gameAssemblyBytes[i + 2] == GameAssemblySodiumIntegrityFuncOrg[2] &&
           gameAssemblyBytes[i + 3] == GameAssemblySodiumIntegrityFuncOrg[3] &&
           gameAssemblyBytes[i + 4] == GameAssemblySodiumIntegrityFuncOrg[4])
                    {
                        // was not patched
                        break;
                    }

                    if (gameAssemblyBytes[i] == GameAssemblySodiumIntegrityFuncPatch[0] &&
                        gameAssemblyBytes[i + 1] == GameAssemblySodiumIntegrityFuncPatch[1] &&
                        gameAssemblyBytes[i + 2] == GameAssemblySodiumIntegrityFuncPatch[2] &&
                        gameAssemblyBytes[i + 3] == GameAssemblySodiumIntegrityFuncPatch[3] &&
                        gameAssemblyBytes[i + 4] == GameAssemblySodiumIntegrityFuncPatch[4])
                    {
                        gameAssemblyBytes[i] = GameAssemblySodiumIntegrityFuncOrg[0];
                        gameAssemblyBytes[i + 1] = GameAssemblySodiumIntegrityFuncOrg[1];
                        gameAssemblyBytes[i + 2] = GameAssemblySodiumIntegrityFuncOrg[2];
                        gameAssemblyBytes[i + 3] = GameAssemblySodiumIntegrityFuncOrg[3];
                        gameAssemblyBytes[i + 4] = GameAssemblySodiumIntegrityFuncOrg[4];

                        File.WriteAllBytes(gameAssembly, gameAssemblyBytes);
                        break;
                    }
                }

                var certList1 = await File.ReadAllTextAsync(launcherCertList);

                int goodSslIndex1 = certList1.IndexOf("Good SSL Ca");
                if (goodSslIndex1 != -1)
                    await File.WriteAllTextAsync(launcherCertList, certList1.Substring(0, goodSslIndex1));

                var certList2 = await File.ReadAllTextAsync(gameCertList);

                int goodSslIndex2 = certList2.IndexOf("Good SSL Ca");
                if (goodSslIndex2 != -1)
                    await File.WriteAllTextAsync(gameCertList, certList2.Substring(0, goodSslIndex2));
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
                if (!(await File.ReadAllTextAsync(hostsFilePath)).Contains("global-lobby.nikke-kr.com"))
                {
                    using StreamWriter w = File.AppendText(hostsFilePath);
                    w.WriteLine();
                    w.Write(hosts);
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

                // trust CA
                if (OperatingSystem.IsWindows())
                {
                    X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "myCA.pfx")));
                    store.Close();
                }

                // update sodium lib

                if (!File.Exists(gameSodium))
                {
                    throw new Exception("expected sodium library to exist at path " + gameSodium);
                }

                // copy backup if sodium size is correct
                var sod = await File.ReadAllBytesAsync(gameSodium);
                if (sod.Length <= 307200)
                {
                    // orignal file size, copy backup
                    await File.WriteAllBytesAsync(sodiumBackup, sod);
                }

                // write new sodium library
                await File.WriteAllBytesAsync(gameSodium, await File.ReadAllBytesAsync(sodiumLib));

                // patch gameassembly to remove sodium IntegrityUtility Check introduced in v124.6.10
                var gameAssemblyBytes = await File.ReadAllBytesAsync(gameAssembly);
                for (int i = 0x5877FFB; i < gameAssemblyBytes.Length; i++)
                {
                    if (gameAssemblyBytes[i] == GameAssemblySodiumIntegrityFuncOrg[0] &&
                        gameAssemblyBytes[i + 1] == GameAssemblySodiumIntegrityFuncOrg[1] &&
                        gameAssemblyBytes[i + 2] == GameAssemblySodiumIntegrityFuncOrg[2] &&
                        gameAssemblyBytes[i + 3] == GameAssemblySodiumIntegrityFuncOrg[3] &&
                        gameAssemblyBytes[i + 4] == GameAssemblySodiumIntegrityFuncOrg[4])
                    {
                        gameAssemblyBytes[i] = GameAssemblySodiumIntegrityFuncPatch[0]; // MOV ax, 1
                        gameAssemblyBytes[i + 1] = GameAssemblySodiumIntegrityFuncPatch[1];
                        gameAssemblyBytes[i + 2] = GameAssemblySodiumIntegrityFuncPatch[2]; // NOP
                        gameAssemblyBytes[i + 3] = GameAssemblySodiumIntegrityFuncPatch[3]; // NOP
                        gameAssemblyBytes[i + 4] = GameAssemblySodiumIntegrityFuncPatch[4]; // NOP

                        await File.WriteAllBytesAsync(gameAssembly, gameAssemblyBytes);
                        break;
                    }

                    if (gameAssemblyBytes[i] == GameAssemblySodiumIntegrityFuncPatch[0] &&
                        gameAssemblyBytes[i + 1] == GameAssemblySodiumIntegrityFuncPatch[1] &&
                        gameAssemblyBytes[i + 2] == GameAssemblySodiumIntegrityFuncPatch[2] &&
                        gameAssemblyBytes[i + 3] == GameAssemblySodiumIntegrityFuncPatch[3] &&
                        gameAssemblyBytes[i + 4] == GameAssemblySodiumIntegrityFuncPatch[4])
                    {
                        // was already patched
                        break;
                    }
                }



                // update launcher/game ca cert list

                var certList1 = await File.ReadAllTextAsync(launcherCertList);
                certList1 += "\nGood SSL Ca\n===============================\n";
                certList1 += CAcert;
                await File.WriteAllTextAsync(launcherCertList, certList1);

                var certList2 = await File.ReadAllTextAsync(gameCertList);
                certList2 += "\nGood SSL Ca\n===============================\n";
                certList2 += CAcert;
                await File.WriteAllTextAsync(gameCertList, certList2);
            }
        }
    }
}
