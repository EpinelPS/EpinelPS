using System.Security.Cryptography;

namespace GameDownloader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var key = "f8c65f692a6a021a688507a6e441786a";


            var bytes = File.ReadAllBytes(@"C:\Users\Misha\Desktop\nikke-server\nksrv\bin\Debug\net8.0\win-x64\cache\PC\prod\rid.48-r.02587\manifestv2\48_5937488248518493556_0.manifest");


            var x = Aes.Create();
            x.KeySize = 128;
            x.Key = StrToByteArray(key);
            x.Mode = CipherMode.CFB;
            x.Padding = PaddingMode.None;
            x.IV = new byte[16];
            var abc = x.CreateDecryptor();

            var str = new CryptoStream(new MemoryStream(bytes), abc, CryptoStreamMode.Read);

            var decr = new MemoryStream();

            str.CopyTo(decr);

            var res = decr.ToArray();

            File.WriteAllBytes("test", res);


            //var b2 = x.DecryptEcb(bytes, PaddingMode.None);
        }
        public static byte[] StrToByteArray(string str)
        {
            str = str.ToUpper();
            Dictionary<string, byte> hexindex = new Dictionary<string, byte>();
            for (int i = 0; i <= 255; i++)
                hexindex.Add(i.ToString("X2"), (byte)i);

            List<byte> hexres = new List<byte>();
            for (int i = 0; i < str.Length; i += 2)
                hexres.Add(hexindex[str.Substring(i, 2)]);

            return hexres.ToArray();
        }
    }
}