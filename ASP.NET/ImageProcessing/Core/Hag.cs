namespace ImageProcessing.Core;

public class Hag {
    public HagHeader Header { get; set; }
    public HagBody Body { get; set; }

    public Hag() {
    }

    public Hag(byte[] data) {
        if (data == null || data.Length < HagHeader.LENGTH) {
            throw new ArgumentException("Data must be at least 8 bytes long", nameof(data));
        }

        byte[] headerBytes = data.Take(HagHeader.LENGTH).ToArray();
        Header = new HagHeader(headerBytes);

        byte[] bodyBytes = data.Skip(HagHeader.LENGTH).ToArray();
        Body = new HagBody(bodyBytes);
    }

    public override string ToString() =>
        $"HAG - {Header}, Encoded Body Size: {Body} bytes";

    public class HagHeader : ImageHeader {
        public static readonly int LENGTH = 8;

        public HagHeader() {

        }

        public HagHeader(byte[] data) {
            if (data == null || data.Length != LENGTH) {
                throw new ArgumentException("Header must be exactly 8 bytes.", nameof(data));
            }

            Width = BitConverter.ToInt32(data.Take(4).Reverse().ToArray(), 0);
            Height = BitConverter.ToInt32(data.Skip(4).Take(4).Reverse().ToArray(), 0);
        }
    }

    public class HagBody {
        public byte[] Data { get; set; }

        public HagBody() {
            Data = [];
        }
        
        public HagBody(byte[] data) {
            Data = data;
        }

        public override string ToString() {
            return $"Data: {Data}";
        }
    }
}