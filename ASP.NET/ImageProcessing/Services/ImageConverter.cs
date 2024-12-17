using ImageProcessing.Serializers;

namespace ImageProcessing.Services;

public class ImageConverter {
    public static string Convert(string inputPath, SupportedImageFormats outputFormat) {
        var inputFormat = GetFormatFromExtension(inputPath);

        var outputPath = Path.Combine(
            Path.GetDirectoryName(inputPath)!,
            $"harmony-output.{outputFormat}"
        );

        Convert(inputPath, outputPath, inputFormat, outputFormat);

        return outputPath;
    }

    public static void Convert(string inputPath, string outputPath, SupportedImageFormats inputFormat, SupportedImageFormats outputFormat) {
        var inputSerializer = SerializerFactory.Create(inputFormat);
        var outputSerializer = SerializerFactory.Create(outputFormat);

        using var inputStream = File.OpenRead(inputPath);
        var sif = inputSerializer.Serialize(inputStream);

        using var outputStream = outputSerializer.Deserialize(sif);
        using var fileStream = File.Create(outputPath);

        outputStream.CopyTo(fileStream);
    }

    public static SupportedImageFormats GetFormatFromExtension(string filePath) {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch {
            ".png" => SupportedImageFormats.Png,
            ".jpg" or ".jpeg" => SupportedImageFormats.Jpeg,
            ".hag" => SupportedImageFormats.Hag,
            _ => throw new ArgumentException($"Unsupported file extension: {extension}")
        };
    }
}