using ImageProcessing.Core;
using ImageProcessing.Services;

namespace ImageProcessing.Serializers;

public class HagSerializer : ISerializer {
    private readonly HagCodec codec = new();

    public Sif Serialize(Stream source) {
        var buffer = new byte[source.Length];
        source.Read(buffer, 0, (int)source.Length);
        var hag = new Hag(buffer);

        return codec.Decode(hag);
    }

    public Stream Deserialize(Sif source) {
        var hag = codec.Encode(source);
        var stream = new MemoryStream();

        var widthBytes = BitConverter.GetBytes(hag.Header.Width).Reverse().ToArray();
        var heightBytes = BitConverter.GetBytes(hag.Header.Height).Reverse().ToArray();

        stream.Write(widthBytes, 0, 4);
        stream.Write(heightBytes, 0, 4);

        if (hag.Body.Data != null && hag.Body.Data.Length > 0) {
            stream.Write(hag.Body.Data, 0, hag.Body.Data.Length);
        }

        stream.Position = 0;
        return stream;
    }
}