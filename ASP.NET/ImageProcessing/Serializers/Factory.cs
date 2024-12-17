using System.Net.NetworkInformation;

namespace ImageProcessing.Serializers;

/// <summary>
/// Factory for creating image format-specific serializers.
/// Provides a centralized way to instantiate the appropriate serializer for each supported image format.
/// </summary>
public class SerializerFactory {
    /// <summary>
    /// Creates a serializer instance for the specified image format
    /// </summary>
    /// <param name="format">The image format to create a serializer for</param>
    /// <returns>A serializer instance that can handle the specified format</returns>
    /// <exception cref="ArgumentException">Thrown when format is not supported</exception>
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

/// <summary>
/// Enumeration of all supported image formats
/// </summary>
public enum SupportedImageFormats {
    Hag,
    Png,
    Jpeg,
    Bmp,
    Webp,
    Qoi
}