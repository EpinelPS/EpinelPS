using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ServerSelector
{
    public class ServerSwitcher
    {
        public static bool IsUsingOfficalServer()
        {
            var hostsFile = File.ReadAllText("C:\\Windows\\System32\\drivers\\etc\\hosts");
            return !hostsFile.Contains("cloud.nikke-kr.com");
        }

        public static void SaveCfg(bool useOffical, string gamePath, string launcherPath)
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

                    int startIdx = txt.IndexOf("127.0.0.1 cloud.nikke-kr.com");
                    int endIdx = txt.IndexOf("y.io") + 3;
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
                string hosts = @"127.0.0.1 cloud.nikke-kr.com
127.0.0.1 global-lobby.nikke-kr.com
127.0.0.1 jp-lobby.nikke-kr.com
127.0.0.1 us-lobby.nikke-kr.com
127.0.0.1 kr-lobby.nikke-kr.com
127.0.0.1 sea-lobby.nikke-kr.com
127.0.0.1 hmt-lobby.nikke-kr.com
127.0.0.1 aws-na-dr.intlgame.com
127.0.0.1 sg-vas.intlgame.com
127.0.0.1 aws-na.intlgame.com
127.0.0.1 na-community.playerinfinite.com
127.0.0.1 common-web.intlgame.com
127.0.0.1 li-sg.intlgame.com
127.0.0.1 data-aws-na.intlgame.com
255.255.221.21 sentry.io";

                if (!File.ReadAllText(hostsFilePath).Contains("global-lobby.nikke-kr.com"))
                {
                    using StreamWriter w = File.AppendText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers/etc/hosts"));
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
