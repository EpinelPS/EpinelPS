// AES-128 OFB stream - ported from NikkeTools (https://github.com/Hiro420/NikkeTools)
// Original author: Hiro420
// License: GPL-3.0

using System.Security.Cryptography;

namespace EpinelPS.Data;

public class OfbStream : Stream
{
    private const int Blocks = 16;
    private readonly Stream _parent;
    private readonly CryptoStream _cbcStream;
    private readonly CryptoStreamMode _mode;
    private readonly byte[] _keyStreamBuffer;
    private int _keyStreamBufferOffset;
    private readonly byte[] _readWriteBuffer;

    public override bool CanRead => _mode == CryptoStreamMode.Read;
    public override bool CanSeek => false;
    public override bool CanWrite => _mode == CryptoStreamMode.Write;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public OfbStream(Stream parent, SymmetricAlgorithm algo, CryptoStreamMode mode)
    {
        algo.Mode = CipherMode.CBC;
        algo.Padding = PaddingMode.None;
        _parent = parent;
        _cbcStream = new CryptoStream(new ZeroStream(), algo.CreateEncryptor(), CryptoStreamMode.Read);
        _mode = mode;
        _keyStreamBuffer = new byte[algo.BlockSize * Blocks];
        _readWriteBuffer = new byte[_keyStreamBuffer.Length];
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int toRead = Math.Min(count, _readWriteBuffer.Length);
        int read = _parent.Read(_readWriteBuffer, 0, toRead);
        if (read == 0) return 0;

        for (int i = 0; i < read; i++)
        {
            if (_keyStreamBufferOffset % _keyStreamBuffer.Length == 0)
            {
                FillKeyStreamBuffer();
                _keyStreamBufferOffset = 0;
            }
            buffer[offset + i] = (byte)(_readWriteBuffer[i] ^ _keyStreamBuffer[_keyStreamBufferOffset++]);
        }

        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        int rwOffset = 0;
        for (int i = 0; i < count; i++)
        {
            if (_keyStreamBufferOffset % _keyStreamBuffer.Length == 0)
            {
                FillKeyStreamBuffer();
                _keyStreamBufferOffset = 0;
            }

            if (rwOffset % _readWriteBuffer.Length == 0)
            {
                _parent.Write(_readWriteBuffer, 0, rwOffset);
                rwOffset = 0;
            }

            _readWriteBuffer[rwOffset++] = (byte)(buffer[offset + i] ^ _keyStreamBuffer[_keyStreamBufferOffset++]);
        }
        _parent.Write(_readWriteBuffer, 0, rwOffset);
    }

    private void FillKeyStreamBuffer()
    {
        int read = _cbcStream.Read(_keyStreamBuffer, 0, _keyStreamBuffer.Length);
        if (read != _keyStreamBuffer.Length)
            throw new InvalidOperationException("Failed to fill key stream buffer");
    }

    public override void Flush() { }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}
