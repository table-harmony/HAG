using System.Net.NetworkInformation;

namespace ImageProcessing.Serializers;

public class SerializerFactory {
    public static ISerializer Create(SupportedImageFormats format) {
        return format switch {
            SupportedImageFormats.Png => new PngSerializer(),
            SupportedImageFormats.Hag => new HagSerializer(),
            SupportedImageFormats.Jpeg => new JpegSerializer(),
            SupportedImageFormats.Bmp => new BmpSerializer(),
            SupportedImageFormats.Webp => new WebpSerializer(),
            SupportedImageFormats.Qoi => new QoiSerializer(),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }
}

public enum SupportedImageFormats {
    Hag,
    Png,
    Jpeg,
    Bmp,
    Webp,
    Qoi
}