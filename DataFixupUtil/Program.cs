using System.IO.Compression;

namespace DataFixupUtil
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            foreach (var arg in Directory.GetFiles("C:\\NIKKE\\NIKKE\\Game"))
            {
                var fileName = Path.GetFileName(arg);
                if (fileName.StartsWith("input"))
                {
                    byte[] FileContents = File.ReadAllBytes(arg);
                    using MemoryStream ms = new MemoryStream(FileContents);
                    File.WriteAllBytes("fullPkt-decr", ms.ToArray());

                    var unkVal1 = ms.ReadByte();
                    var pktLen = ms.ReadByte() & 0x1f;


                    var seqNumB = ms.ReadByte();
                    var seqNum = seqNumB;
                    if (seqNumB >= 24)
                    {
                        var b = ms.ReadByte();

                        seqNum = BitConverter.ToUInt16(new byte[] { (byte)b, (byte)seqNumB }, 0);

                        // todo support uint32
                    }

                    var startPos = (int)ms.Position;

                    var contents = FileContents.Skip(startPos).ToArray();
                    if (contents.Length > 2 && contents[0] == 0x1f && contents[1] == 0x8b)
                    {
                        // gzip compression is used
                        using Stream csStream = new GZipStream(new MemoryStream(contents), CompressionMode.Decompress);
                        using MemoryStream decoded = new MemoryStream();
                        csStream.CopyTo(decoded);

                        contents = decoded.ToArray();
                        File.WriteAllBytes(arg, contents);
                    }
                    else
                    {
                        File.WriteAllBytes(arg, contents);
                    }
                }
            }
        }
    }
}
