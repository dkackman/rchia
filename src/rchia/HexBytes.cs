using System;
using System.Linq;
using System.Security.Cryptography;

//
// adapted from https://github.com/Playwo/ChiaRPC.Net/blob/master/ChiaRPC.Net/Utils/Bech32M.cs
//
namespace rchia
{
    public struct HexBytes
    {
        public string Hex { get; init; }
        public byte[] Bytes { get; init; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Hex);

        public HexBytes(string hex, byte[] bytes)
        {
            Hex = hex ?? string.Empty;
            Bytes = bytes ?? Array.Empty<byte>();
        }

        public HexBytes Sha256()
        {
            var hash = SHA256.HashData(Bytes);
            var hexHash = HexMate.Convert.ToHexString(hash);
            return new HexBytes(hexHash, hash);
        }

        public static HexBytes operator +(HexBytes a, HexBytes b)
        {
            var concatHex = string.Concat(a.Hex, b.Hex);
            var concatBytes = a.Bytes.Concat(b.Bytes).ToArray();

            return new HexBytes(concatHex, concatBytes);
        }

        public static HexBytes operator +(HexBytes a, byte[] b)
        {
            var bs = HexMate.Convert.ToHexString(b);
            var concatHex = string.Concat(a.Hex, bs);
            var concatBytes = a.Bytes.Concat(b).ToArray();

            return new HexBytes(concatHex, concatBytes);
        }

        public static HexBytes operator +(HexBytes a, string b)
        {
            var bb = HexMate.Convert.FromHexString(b);
            var concatHex = string.Concat(a.Hex, b);
            var concatBytes = a.Bytes.Concat(bb).ToArray();

            return new HexBytes(concatHex, concatBytes);
        }

        public static bool operator ==(HexBytes a, HexBytes b)
        {
            return a.Hex.ToUpperInvariant() == b.Hex.ToUpperInvariant();
        }

        public static bool operator !=(HexBytes a, HexBytes b)
        {
            return a.Hex.ToUpperInvariant() != b.Hex.ToUpperInvariant();
        }

        public static HexBytes FromHex(string hex)
        {
            return string.IsNullOrWhiteSpace(hex)
                           ? Empty
                           : hex.StartsWith("0x")
                               ? new HexBytes(hex[2..], HexMate.Convert.FromHexString(hex.AsSpan()[2..]))
                               : new HexBytes(hex, HexMate.Convert.FromHexString(hex.AsSpan()));
        }

        public static bool TryFromHex(string hex, out HexBytes result)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                result = Empty;
                return true;
            }


            var bytes = new byte[hex.Length / 2].AsSpan();
            var s = hex.StartsWith("0x") ? hex.AsSpan()[2..] : hex.AsSpan();
            if (!HexMate.Convert.TryFromHexChars(s, bytes, out int written))
            {
                result = Empty;
                return false;
            }

            result = new HexBytes(s.ToString(), bytes.Slice(0, written).ToArray());
            return true;
        }

        public static HexBytes FromBytes(byte[] bytes)
        {
            return bytes == null
                           ? Empty
                           : new HexBytes(
                               HexMate.Convert.ToHexString(bytes),
                               bytes);
        }

        public override bool Equals(object obj)
        {
            return obj is HexBytes other && Hex.ToUpperInvariant() == other.Hex.ToUpperInvariant();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Hex);
        }

        public override string ToString()
        {
            return Hex.ToUpperInvariant();
        }

        public static HexBytes Empty => new("", Array.Empty<byte>());
    }
}
