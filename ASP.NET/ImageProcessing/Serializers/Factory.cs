using System.Net.NetworkInformation;

namespace ImageProcessing.Serializers;

public class SerializerFactory {
    public static ISerializer Create(SupportedImageFormats format) {
        return format switch {
            SupportedImageFormats.Png => new PngSerializer(),
            SupportedImageFormats.Hag => new HagSerializer(),
            SupportedImageFormats.Jpeg => new JpegSerializer(),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }
}

public enum SupportedImageFormats {
    Hag,
    Png,
    Jpeg,
}