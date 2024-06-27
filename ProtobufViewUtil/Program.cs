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

            ResGetStageData s = new ResGetStageData();
            var inn = File.ReadAllBytes(@"C:\Users\Misha\Downloads\getstageinfo2");
            s.MergeFrom(inn);
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
