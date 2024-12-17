using ImageProcessing.Serializers;

namespace ImageProcessing.Services;

public class ImageConverter {
    private readonly SerializerFactory serializerFactory = new();

    public void Convert(string inputPath, string outputPath, SupportedImageFormats inputFormat, SupportedImageFormats outputFormat) {
        var inputSerializer = serializerFactory.Create(inputFormat);
        var outputSerializer = serializerFactory.Create(outputFormat);

        using var inputStream = File.OpenRead(inputPath);
        var sif = inputSerializer.Serialize(inputStream);

        using var outputStream = outputSerializer.Deserialize(sif);
        using var fileStream = File.Create(outputPath);

        outputStream.CopyTo(fileStream);
    }
}