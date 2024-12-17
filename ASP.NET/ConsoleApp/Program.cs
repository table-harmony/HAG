using ImageProcessing.Services;

if (args.Length < 2) {
    Console.WriteLine("Usage: program <input-file> <output-format>");
    Console.WriteLine("Example: program image.png jpg");
    return;
}

string inputPath = args[0];
string targetFormat = args[1].ToLowerInvariant();

if (!File.Exists(inputPath)) {
    Console.WriteLine($"Input file not found: {inputPath}");
    return;
}

try {
    var outputFormat = ImageConverter.GetFormatFromExtension($"dummy.{targetFormat}");

    string outputPath = ImageConverter.Convert(
        inputPath,
        outputFormat
    );

    Console.WriteLine($"Successfully converted {inputPath} to {outputPath}");
} catch (Exception ex) {
    Console.WriteLine($"Error: {ex.Message}");
}