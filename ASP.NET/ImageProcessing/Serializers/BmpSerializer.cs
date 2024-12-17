using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ImageProcessing.Core;

namespace ImageProcessing.Serializers;

public class BmpSerializer : ISerializer {
    public Sif Serialize(Stream source) {
        using var image = Image.Load<Rgb24>(source);
        var result = new Sif {
            Header = new ImageHeader {
                Width = image.Width,
                Height = image.Height,
                Format = ColorFormat.RGB,
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
                });
            }
        }

        result.Body.Pixels = pixels;
        return result;
    }

    public Stream Deserialize(Sif source) {
        using var image = new Image<Rgb24>(source.Header.Width, source.Header.Height);
        var pixelArray = source.Body.Pixels.ToArray();
        int index = 0;

        for (int y = 0; y < image.Height; y++) {
            for (int x = 0; x < image.Width; x++) {
                if (index == pixelArray.Length - 1)
                    break;

                var pixel = pixelArray[index++];
                image[x, y] = new Rgb24(
                    (byte)pixel.Red,
                    (byte)pixel.Green,
                    (byte)pixel.Blue
                );
            }
        }

        var stream = new MemoryStream();
        image.SaveAsBmp(stream);
        stream.Position = 0;

        return stream;
    }
}