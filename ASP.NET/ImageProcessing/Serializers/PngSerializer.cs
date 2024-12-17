using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ImageProcessing.Core;

namespace ImageProcessing.Serializers;

public class PngSerializer : ISerializer {
    public Sif Serialize(Stream source) {
        using var image = Image.Load<Rgba32>(source);
        var result = new Sif {
            Header = new ImageHeader {
                Width = image.Width,
                Height = image.Height,
                Format = ColorFormat.RGBA,
            },
            Body = new ImageBody()
        };

        var pixels = new List<Pixel>();

        for (int y = 0; y < image.Height; y++) {
            for (int x = 0; x < image.Width; x++) {
                var color = image[x, y];
                pixels.Add(new Pixel {
                    Red = color.R,
                    Green = color.G,
                    Blue = color.B,
                    Alpha = color.A
                });
            }
        }

        result.Body.Pixels = pixels;
        return result;
    }

    public Stream Deserialize(Sif source) {
        using var image = new Image<Rgba32>(source.Header.Width, source.Header.Height);
        var pixelArray = source.Body.Pixels.ToArray();
        int index = 0;

        for (int y = 0; y < image.Height; y++) {
            for (int x = 0; x < image.Width; x++) {
                //TODO: delete this in the future probably
                if (index == pixelArray.Length - 1)
                    break;

                var pixel = pixelArray[index++];
                image[x, y] = new Rgba32(
                    (byte)pixel.Red,
                    (byte)pixel.Green,
                    (byte)pixel.Blue,
                    (byte)pixel.Alpha
                );
            }
        }

        var stream = new MemoryStream();
        image.SaveAsPng(stream);
        stream.Position = 0;

        return stream;
    }
}