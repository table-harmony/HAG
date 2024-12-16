namespace ImageProcessing.Core;

public class Hag {
    public HagHeader Header { get; }
    public IEnumerable<byte> Body { get; }

    public Hag(byte[] data) {
        if (data == null || data.Length < HagHeader.LENGTH) {
            throw new ArgumentException("Data must be at least 8 bytes long", nameof(data));
        }

        Header = new HagHeader(data.Take(HagHeader.LENGTH).ToArray());
        Body = [.. data.Skip(HagHeader.LENGTH)];
    }

    public override string ToString() =>
        $"HAG - {Header}, Encoded Body Size: {Body.Count()} bytes";

    public class HagHeader : ImageHeader {
        public static readonly int LENGTH = 8;

        public HagHeader(byte[] data) {
            if (data == null || data.Length != LENGTH) {
                throw new ArgumentException("Header must be exactly 8 bytes.", nameof(data));
            }

            Width = BitConverter.ToInt32(data.Take(4).Reverse().ToArray(), 0);
            Height = BitConverter.ToInt32(data.Skip(4).Take(4).Reverse().ToArray(), 0);
        }
    }
}