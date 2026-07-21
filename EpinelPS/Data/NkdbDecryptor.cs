// NKDB decryption - ported from NikkeTools (https://github.com/Hiro420/NikkeTools)
// Original author: Hiro420
// License: GPL-3.0
// Adapted for NKDB-encrypted .lsc files used by Goddess of Victory: NIKKE

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace EpinelPS.Data;

public static class NkdbDecryptor
{
    private const uint NkdbMagic = 0x42444B4E;

    public static byte[] Decrypt(byte[] input)
    {
        using var ms = new MemoryStream(input);
        using var reader = new BinaryReader(ms);

        var magic = reader.ReadUInt32();
        if (magic != NkdbMagic)
            throw new InvalidDataException("Invalid NKDB magic");

        var version = ReadUInt32BigEndian(reader);
        if (version != 1)
            throw new InvalidDataException($"Unsupported NKDB version: {version}");

        var aesKey = reader.ReadBytes(16);
        var segmentSize = ReadUInt32BigEndian(reader);
        var segmentCount = ReadUInt32BigEndian(reader);

        int lengthByteCount = segmentSize * segmentCount > 0xFFFFFFFF ? 5 : 4;

        long ReadOffset()
        {
            var offsetBytes = reader.ReadBytes(lengthByteCount);
            long result = 0;
            foreach (var b in offsetBytes)
                result = (result << 8) | b;
            return result;
        }

        var currentOffset = ReadOffset();
        var segments = new (long Offset, long Length, int Index)[segmentCount];

        for (int i = 0; i < segmentCount; i++)
        {
            var nextOffset = ReadOffset();
            segments[i] = (currentOffset, nextOffset - currentOffset, i);
            currentOffset = nextOffset;
        }

        using var output = new MemoryStream();

        foreach (var (offset, length, index) in segments)
        {
            ms.Seek(offset, SeekOrigin.Begin);
            var segment = reader.ReadBytes((int)length);

            var iv = new byte[16];
            BitConverter.GetBytes(index).CopyTo(iv, 0);
            BitConverter.GetBytes((int)offset).CopyTo(iv, 4);

            var decrypted = DecryptAesOfb(aesKey, iv, segment);

            using var decompressMs = new MemoryStream(decrypted);
            using var zlib = new ZLibStream(decompressMs, CompressionMode.Decompress);
            zlib.CopyTo(output);
        }

        return output.ToArray();
    }

    public static byte[] DecryptFile(string path)
    {
        return Decrypt(File.ReadAllBytes(path));
    }

    private static byte[] DecryptAesOfb(byte[] key, byte[] iv, byte[] input)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var inputStream = new MemoryStream(input);
        using var ofbStream = new OfbStream(inputStream, aes, CryptoStreamMode.Read);
        using var outputStream = new MemoryStream();
        ofbStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }

    private static uint ReadUInt32BigEndian(BinaryReader reader)
    {
        var bytes = reader.ReadBytes(4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToUInt32(bytes, 0);
    }
}
