using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace ServerSelector
{
    public class ServerSwitcher
    {
        public static bool IsUsingOfficalServer()
        {
            var hostsFile = File.ReadAllText("C:\\Windows\\System32\\drivers\\etc\\hosts");
            return !hostsFile.Contains("cloud.nikke-kr.com");
        }

        public static void SaveCfg(bool useOffical, string gamePath, string launcherPath, string ip)
        {
            string sodiumLib = AppDomain.CurrentDomain.BaseDirectory + "sodium.dll";
            string gameSodium = gamePath + "/nikke_Data/Plugins/x86_64/sodium.dll";
            string sodiumBackup = gameSodium + ".bak";
            string hostsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts");
            var CAcert = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "myCA.pem");

            string launcherCertList = launcherPath + "/intl_service/cacert.pem";
            string gameCertList = gamePath + "/nikke_Data/Plugins/x86_64/cacert.pem";


            // TODO: allow changing ip address
            if (useOffical)
            {
                var txt = File.ReadAllText(hostsFilePath);

                // remove stuff
                try
                {

                    int startIdx = txt.IndexOf("cloud.nikke-kr.com");

                    // find new line character before start index
                    for (int i = startIdx - 1; i >= 0; i--)
                    {
                        var c = txt[i];
                        if (c == '\n')
                        {
                            startIdx = i + 1;
                            break;
                        }
                    }

                    int endIdx = txt.IndexOf("y.io") + 4;
                    txt = txt.Substring(0, startIdx) + txt.Substring(endIdx);

                    File.WriteAllText(hostsFilePath, txt);
                }
                catch
                {

                }


                // remove cert
                X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Remove(new X509Certificate2(X509Certificate2.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "myCA.pfx")));
                store.Close();

                // restore sodium
                if (!File.Exists(sodiumBackup))
                {
                    throw new Exception("sodium backup does not exist");
                }
                File.Copy(sodiumBackup, gameSodium, true);

                var certList1 = File.ReadAllText(launcherCertList);
                File.WriteAllText(launcherCertList, certList1.Substring(0, certList1.IndexOf("Good SSL Ca")));

                var certList2 = File.ReadAllText(gameCertList);
                File.WriteAllText(gameCertList, certList2.Substring(0, certList2.IndexOf("Good SSL Ca")));
            }
            else
            {
                // add to hosts file
                string hosts = $@"{ip} cloud.nikke-kr.com
{ip} global-lobby.nikke-kr.com
{ip} jp-lobby.nikke-kr.com
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
{ip} data-aws-na.intlgame.com
255.255.221.21 sentry.io";

                if (!File.ReadAllText(hostsFilePath).Contains("global-lobby.nikke-kr.com"))
                {
                    using StreamWriter w = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"));
                    w.WriteLine();
                    w.WriteLine(hosts);
                }


                // trust CA
                X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(new X509Certificate2(X509Certificate2.CreateFromCertFile(AppDomain.CurrentDomain.BaseDirectory + "myCA.pfx")));
                store.Close();

                // update sodium lib

                if (!File.Exists(gameSodium))
                {
                    throw new Exception("expected sodium library to exist at path " + gameSodium);
                }

                // copy backup if sodium size is correct
                var sod = File.ReadAllBytes(gameSodium);
                if (sod.Length <= 307200)
                {
                    // orignal file size, copy backup
                    File.WriteAllBytes(sodiumBackup, sod);
                }

                // write new sodium library
                File.WriteAllBytes(gameSodium, File.ReadAllBytes(sodiumLib));


                // update launcher/game ca cert list

                var certList1 = File.ReadAllText(launcherCertList);
                certList1 += "\nGood SSL Ca\n===============================\n";
                certList1 += CAcert;
                File.WriteAllText(launcherCertList, certList1);

                var certList2 = File.ReadAllText(gameCertList);
                certList2 += "\nGood SSL Ca\n===============================\n";
                certList2 += CAcert;
                File.WriteAllText(gameCertList, certList2);
            }
        }
    }
}
