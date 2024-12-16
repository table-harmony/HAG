using System;
using System.Collections.Immutable;

namespace Server.x.Entities;

public class HagImage
{
    public const int HEADER_LENGTH = 8;

    public ImmutableArray<byte> RawData { get; }
    public HagHeader Header { get; }
    public ImmutableArray<byte> EncodedBody { get; }

    public HagImage(byte[] data)
    {
        if (data == null || data.Length < HEADER_LENGTH)
        {
            throw new ArgumentException("Data must be at least 8 bytes long", nameof(data));
        }

        RawData = [.. data];
        Header = new HagHeader(RawData.Take(HEADER_LENGTH).ToArray());
        EncodedBody = [.. RawData.Skip(HEADER_LENGTH)];
    }

    public static HagImage Load(string filePath)
    {
        return new HagImage(File.ReadAllBytes(filePath));
    }

    public void Save(string filePath)
    {
        File.WriteAllBytes(filePath, [.. RawData]);
    }

    public byte[] Decode()
    {
        // TODO: Implement HAG image decoding
        throw new NotImplementedException("Decoding not yet implemented");
    }

    public override string ToString() =>
        $"HAG Image - {Header}, Encoded Body Size: {EncodedBody.Length} bytes";

    public class HagHeader
    {
        public int Width { get; }
        public int Height { get; }
        public ColorFormat ColorFormat { get; }

        public HagHeader(byte[] data)
        {
            if (data == null || data.Length != HEADER_LENGTH)
            {
                throw new ArgumentException("Header must be exactly 8 bytes.", nameof(data));
            }

            Width = BitConverter.ToInt32(data.Take(4).Reverse().ToArray(), 0);
            Height = BitConverter.ToInt32(data.Skip(4).Take(4).Reverse().ToArray(), 0);

            ColorFormat = ColorFormat.RGB;
        }

        public override string ToString() =>
            $"Width: {Width}, Height: {Height}, Format: {ColorFormat}";
    }

    public enum ColorFormat
    {
        RGB,
        RGBA,
        GRAYSCALE
    }
}