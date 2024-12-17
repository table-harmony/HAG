namespace ImageProcessing.Serializers;

public class SerializerFactory {
    public ISerializer Create(SupportedImageFormats format) {
        return format switch {
            SupportedImageFormats.Png => new PngSerializer(),
            SupportedImageFormats.Hag => new HagSerializer(),
            SupportedImageFormats.Jpeg => new PngSerializer(),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }
}

public enum SupportedImageFormats {
    Hag,
    Png,
    Jpeg,
}