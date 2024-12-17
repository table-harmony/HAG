using ImageProcessing.Serializers;

namespace ImageProcessing.Services;

/// <summary>
/// Handles conversion between different image formats using the SIF intermediate format.
/// Supports conversion between PNG, JPEG, HAG, BMP, WebP and QOI formats.
/// </summary>
public class ImageConverter {
    /// <summary>
    /// Converts an image file from one format to another
    /// </summary>
    /// <param name="inputPath">Path to the source image file</param>
    /// <param name="outputFormat">Desired output format</param>
    /// <returns>Path to the converted image file</returns>
    public static string Convert(string inputPath, SupportedImageFormats outputFormat) {
        var inputFormat = GetFormatFromExtension(inputPath);

        var outputPath = Path.Combine(
            Path.GetDirectoryName(inputPath)!,
            $"harmony-output.{outputFormat}"
        );

        Convert(inputPath, outputPath, inputFormat, outputFormat);

        return outputPath;
    }

    private static void Convert(string inputPath, string outputPath, SupportedImageFormats inputFormat, SupportedImageFormats outputFormat) {
        var inputSerializer = SerializerFactory.Create(inputFormat);
        var outputSerializer = SerializerFactory.Create(outputFormat);

        using var inputStream = File.OpenRead(inputPath);
        var sif = inputSerializer.Serialize(inputStream);

        using var outputStream = outputSerializer.Deserialize(sif);
        using var fileStream = File.Create(outputPath);

        outputStream.CopyTo(fileStream);
    }

    /// <summary>
    /// Determines the image format from a file extension
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <returns>The detected image format</returns>
    /// <exception cref="ArgumentException">Thrown when file extension is not supported</exception>
    public static SupportedImageFormats GetFormatFromExtension(string filePath) {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch {
            ".png" => SupportedImageFormats.Png,
            ".jpg" or ".jpeg" => SupportedImageFormats.Jpeg,
            ".hag" => SupportedImageFormats.Hag,
            ".bmp" => SupportedImageFormats.Bmp,
            ".webp" => SupportedImageFormats.Webp,
            ".qoi" => SupportedImageFormats.Qoi,
            _ => throw new ArgumentException($"Unsupported file extension: {extension}")
        };
    }
}