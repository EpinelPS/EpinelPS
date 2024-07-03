using Google.Protobuf;
using nksrv;
using System.Diagnostics;

namespace ProtobufViewUtil
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            StaticDataPackResponse s = new StaticDataPackResponse();
            var inn = File.ReadAllBytes(@"C:\Users\Misha\Downloads\staticdatanew");
            s.MergeFrom(inn);
            Console.WriteLine("salt1: " + Convert.ToBase64String(s.Salt1.ToArray()));
            Console.WriteLine("salt2: " + Convert.ToBase64String(s.Salt2.ToArray()));
            Console.WriteLine("sha: " + Convert.ToBase64String(s.Sha256Sum.ToArray()));
            Console.WriteLine(s.ToString());
            var outt = s.ToByteArray();
            
            if (inn.SequenceEqual(outt))
            {
                Console.WriteLine("Check OK");
            }
            else
            {
                Console.WriteLine("Check FAIL");
            }
            Debugger.Break();
        }
    }
}
