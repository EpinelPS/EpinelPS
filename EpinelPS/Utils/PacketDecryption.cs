using EpinelPS.LobbyServer;
using Sodium;
using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace EpinelPS.Utils
{
    public class PacketDecryption
    {
        public static async Task<PacketDecryptResponse> DecryptOrReturnContentAsync(HttpContext ctx)
        {
            using MemoryStream buffer = new();

            Stream stream = ctx.Request.Body;

            string? encoding = ctx.Request.Headers.ContentEncoding.FirstOrDefault();

            Stream decryptedStream;
            switch (encoding)
            {
                case "gzip":
                    decryptedStream = new GZipStream(stream, CompressionMode.Decompress);
                    break;
                case "deflate":
                    decryptedStream = new DeflateStream(stream, CompressionMode.Decompress);
                    break;
                case null:
                case "":
                    decryptedStream = stream;
                    break;
                case "gzip,enc":
                    CBorItem responseLen = CBorReadItem(stream); // length of header (not including encrypted data)
                    stream.ReadByte(); // ignore padding
                    stream.ReadByte(); // ignore padding

                    string decryptionToken = CBorReadString(stream);
                    byte[] nonce = CBorReadByteString(stream);

                    MemoryStream encryptedBytes = new();
                    stream.CopyTo(encryptedBytes);

                    byte[] bytes = encryptedBytes.ToArray();

                    GameClientInfo key = LobbyHandler.GetInfo(decryptionToken) ?? throw new BadHttpRequestException("InvalId decryption token");
                    byte[] additionalData = GenerateAdditionalData(decryptionToken, false);

                    byte[] x = SecretAeadXChaCha20Poly1305.Decrypt(bytes, nonce, key.Keys.ReadSharedSecret, [.. additionalData]);

                    MemoryStream ms = new(x);

                    int unkVal1 = ms.ReadByte();
                    int unkVal2 = ms.ReadByte();
                    ulong sequenceNumber = ReadCborInteger(ms);


                    int startPos = (int)ms.Position;

                    byte[] contents = [.. x.Skip(startPos)];
                    if (contents.Length != 0 && contents[0] == 31)
                    {
                        //File.WriteAllBytes("contentsgzip", contents);
                        // gzip compression is used
                        using Stream csStream = new GZipStream(new MemoryStream(contents), CompressionMode.Decompress);
                        using MemoryStream decoded = new();
                        csStream.CopyTo(decoded);

                        contents = decoded.ToArray();
                    }

                    return new PacketDecryptResponse() { Contents = contents, UserId = key.UserId, UsedAuthToken = decryptionToken };
                default:
                    throw new Exception($"Unsupported content encoding \"{encoding}\"");
            }


            await stream.CopyToAsync(buffer, 81920).ConfigureAwait(continueOnCapturedContext: false);
            return new PacketDecryptResponse() { Contents = buffer.ToArray() };
        }

        public static ulong ReadCborInteger(Stream stream)
        {
            // Read the initial byte
            int initialByte = stream.ReadByte();
            if (initialByte == -1)
            {
                throw new EndOfStreamException("Stream ended unexpectedly");
            }

            // Major type is the first 3 bits of the initial byte
            int majorType = (initialByte >> 5) & 0x07;
            // Additional info is the last 5 bits of the initial byte
            int additionalInfo = initialByte & 0x1F;

            if (majorType != 0)
            {
                //throw new InvalidDataException("Not a valId CBOR unsigned integer");
            }

            ulong value;
            if (additionalInfo < 24)
            {
                value = (ulong)additionalInfo;
            }
            else if (additionalInfo == 24)
            {
                value = (ulong)stream.ReadByte();
            }
            else if (additionalInfo == 25)
            {
                Span<byte> buffer = stackalloc byte[2];
                if (stream.Read(buffer) != 2)
                {
                    throw new EndOfStreamException("Stream ended unexpectedly");
                }
                value = BinaryPrimitives.ReadUInt16BigEndian(buffer);
            }
            else if (additionalInfo == 26)
            {
                Span<byte> buffer = stackalloc byte[4];
                if (stream.Read(buffer) != 4)
                {
                    throw new EndOfStreamException("Stream ended unexpectedly");
                }
                value = BinaryPrimitives.ReadUInt32BigEndian(buffer);
            }
            else if (additionalInfo == 27)
            {
                Span<byte> buffer = stackalloc byte[8];
                if (stream.Read(buffer) != 8)
                {
                    throw new EndOfStreamException("Stream ended unexpectedly");
                }
                value = BinaryPrimitives.ReadUInt64BigEndian(buffer);
            }
            else
            {
                throw new InvalidDataException("InvalId additional info for CBOR unsigned integer");
            }

            return value;
        }

        public static byte[] EncryptData(byte[] message, string authToken)
        {
            GameClientInfo key = LobbyHandler.GetInfo(authToken) ?? throw new BadHttpRequestException("InvalId decryption token");
            MemoryStream m = new();

            m.WriteByte(89); // cbor ushort

            // 42bytes of data past header, 3 bytes for auth token bytestring, 2 bytes for nonce prefix, 24 bytes for nonce data
            int headerLen = 2 + 3 + authToken.Length + 2 + 24;
            byte[] headerLenBytes = BitConverter.GetBytes((ushort)headerLen);
            if (BitConverter.IsLittleEndian) headerLenBytes = [.. headerLenBytes.Reverse()];

            m.Write(headerLenBytes, 0, headerLenBytes.Length);

            // write 2 bytes that i am not sure about
            m.WriteByte(131);
            m.WriteByte(1);

            // write auth token len
            m.WriteByte(89); // cbor ushort
            byte[] authLenBytes = BitConverter.GetBytes((ushort)authToken.Length);
            if (BitConverter.IsLittleEndian) authLenBytes = [.. authLenBytes.Reverse()];
            m.Write(authLenBytes, 0, authLenBytes.Length);

            // write actual auth token
            byte[] authBytes = Encoding.UTF8.GetBytes(authToken);
            m.Write(authBytes, 0, authBytes.Length);

            // write nonce
            m.WriteByte(88); // cbor byte
            m.WriteByte(24); // nonce length

            // generate it
            byte[] nonce = new byte[24];
            new Random().NextBytes(nonce);

            // write nonce bytes
            m.Write(nonce, 0, nonce.Length);

            // get additional data
            byte[] additionalData = GenerateAdditionalData(authToken, true);

            // prep payload
            MemoryStream msm = new();
            msm.WriteByte(67);
            msm.WriteByte(129);

            // TODO: write message length

            msm.Write(message);

            byte[] encryptedBytes = SecretAeadXChaCha20Poly1305.Encrypt(msm.ToArray(), nonce, key.Keys.TransferSharedSecret, [.. additionalData]);

            // write encrypted data
            m.Write(encryptedBytes);

            byte[] data = m.ToArray();
            //File.WriteAllBytes("our-encryption", data);

            // we are done
            return data;
        }

        private static byte[] GenerateAdditionalData(string authToken, bool encrypting)
        {
            // Generate "additional data" which consists of auth token and its length using cbor.
            // Not sure what the first bytes are but they are always the same. 89 represents start of UShort
            MemoryStream additionalData = new();
            additionalData.WriteByte((byte)(encrypting ? 129 : 130));
            additionalData.WriteByte(1);

            // write auth token len
            if (!encrypting)
            {
                byte[] authLen = BitConverter.GetBytes((ushort)authToken.Length);
                if (BitConverter.IsLittleEndian)
                    authLen = [.. authLen.Reverse()];
                additionalData.WriteByte(89);
                additionalData.Write(authLen, 0, authLen.Length);

                // write our authentication token
                byte[] authBytes = Encoding.UTF8.GetBytes(authToken);
                additionalData.Write(authBytes, 0, authBytes.Length);
            }

            return additionalData.ToArray();
        }

        private static string CBorReadString(Stream s)
        {
            CBorItem item = CBorReadItem(s);
            if (item.MajorType != 2)
            {
                throw new Exception("invalId string");
            }
            string resp = "";

            int len = item.FullValue;
            for (int i = 0; i < len; i++)
            {
                int b = s.ReadByte();
                if (b == -1) throw new EndOfStreamException();

                resp += (char)b;
            }

            return resp;
        }
        private static byte[] CBorReadByteString(Stream s)
        {
            CBorItem item = CBorReadItem(s);
            if (item.MajorType != 2)
            {
                throw new Exception("invalId string");
            }

            int len = item.FullValue;
            byte[] resp = new byte[len];

            for (int i = 0; i < len; i++)
            {
                int b = s.ReadByte();
                if (b == -1) throw new EndOfStreamException();

                resp[i] = (byte)b;
            }

            return resp;
        }
        private static CBorItem CBorReadItem(Stream s)
        {
            int b = s.ReadByte();
            int type = b & 0x1f;
            CBorItem res = new()
            {
                MajorType = (b >> 5) & 7
            };
            switch (type)
            {
                case 24:
                    // byte
                    res.ByteValue = [(byte)s.ReadByte()];
                    res.type = CBorItemType.Byte;

                    res.FullValue = res.ByteValue[0];
                    break;
                case 25:
                    byte[] arr = new byte[2];
                    ReadToBuff(s, arr);

                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(arr);

                    res.ByteValue = arr;
                    res.type = CBorItemType.UShort;
                    res.UShortValue = BitConverter.ToUInt16(arr, 0);

                    res.FullValue = res.UShortValue;
                    break;
                default:
                    // throw new NotImplementedException();
                    break;
            }

            return res;
        }

        private static void ReadToBuff(Stream s, byte[] buf)
        {
            int i = 0;
            while (i != buf.Length)
            {
                if (i > buf.Length)
                    throw new ArgumentOutOfRangeException(nameof(buf));
                int read = s.Read(buf, i, buf.Length - i);
                if (read == 0)
                    break;
                i += read;
            }

            if (i < buf.Length)
            {
                throw new EndOfStreamException();
            }
        }
    }
    public class PacketDecryptResponse
    {
        public ulong UserId;
        public string UsedAuthToken = "";
        public byte[] Contents = [];
    }
    public class CBorItem
    {
        public CBorItemType type;
        public byte[] ByteValue = [];
        public ushort UShortValue;
        public int MajorType;

        public int FullValue;
    }
    public enum CBorItemType
    {
        Byte,
        UShort,
        ByteString,
    }
}
