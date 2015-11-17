using System;
using System.IO;
using System.Security.Cryptography;

namespace LightRail
{
    public class Oplog : IDisposable
    {
        private readonly string _prefix;

        const int Mb = 1048576;
        const int Size = Mb * 2;

        FileStream _stream = null;

        BinaryWriter _writer = null;
        BinaryReader _reader = null;

        long _position = 0;

        public Oplog(string prefix)
        {
            _prefix = prefix;

            if (_stream == null)
            {
                var file = NamingScheme(_position);
                OpenFileStream(file);
            }
        }

        private void OpenFileStream(string path)
        {
            _stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            _stream.SetLength(Size);

            _writer = new BinaryWriter(_stream);
            _reader = new BinaryReader(_stream);
        }

        private void CloseFileStream()
        {
            _stream.Flush(true);
            _writer.Close();
            _stream.Close();
        }

        public long Append(byte[] payload)
        {
            if (_stream.Position + payload.Length > Size)
            {
                _position = _position + _stream.Position;
                
                CloseFileStream();

                var file = NamingScheme(_position);
                OpenFileStream(file);
            }

            var op = new Op(payload);
            op.WriteTo(_writer);

            _writer.Flush();

            return _stream.Position;
        }

        public Op Read(long pos)
        {
            long fileId = 0;

            do
            {
                fileId = fileId + Size;
            } 
            while (fileId + Size < pos);

            _stream.Position = pos;

            return Op.ReadFrom(_reader);
        }

        public string NamingScheme(long p)
        {
            return string.Format("{0}{1}.sf", _prefix, p.ToString("D12"));
        }

        public void Dispose()
        {
            CloseFileStream();
        }
    }

    public class Op
    {
        public byte[] Payload { get; private set; }
        public byte[] Hash { get; private set; }
        public long Length { get; private set; }
        public long Position { get; private set; }

        public Op(byte[] payload)
        {
            Payload = payload;
            Hash = GetHashFor(payload);
            Length = Payload.Length + Hash.Length + 1;
        }

        public Op(byte[] payload, byte[] hash, long length)
        {
            Payload = payload;
            Hash = hash;
            Length = length;
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Length);
            writer.Write((byte)0);
            writer.Write(Hash);
            writer.Write(Payload);
        }

        public static Op ReadFrom(BinaryReader reader)
        {
            var length = reader.ReadInt64();
            var delivered = reader.ReadByte();
            var hash = reader.ReadBytes(16);

            var payloadLength = length - hash.Length - 1;
            var payload = reader.ReadBytes((int)payloadLength);

            return new Op(payload, hash, length);
        }

        public static byte[] GetHashFor(byte[] payload)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(new MemoryStream(payload));
        }
    }
}