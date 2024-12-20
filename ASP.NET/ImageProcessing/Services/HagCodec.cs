using ImageProcessing.Core;
using SixLabors.ImageSharp.PixelFormats;
using static ImageProcessing.Core.Hag;

namespace ImageProcessing.Services;

/// <summary>
/// Handles encoding and decoding of images to and from the HAG format.
/// The HAG format uses various compression techniques including:
/// - Run-length encoding for repeated pixels
/// - Delta encoding for similar pixels
/// - Lookup table for frequently used pixels
/// </summary>
/// 
public class HagCodec {
    private const byte HAG_COPY = 0xc0;
    private const byte HAG_DELTA = 0x40;
    private const byte HAG_BIG_DELTA = 0x80;
    private const byte HAG_SET = 0x00;
    private const byte HAG_RGB = 0xfe;
    private const byte HAG_RGBA = 0xff;

    private const int UNIQUE_PIXELS_SIZE = 63;

    /// <summary>
    /// Encodes a SIF (Serialized Image Format) into HAG format
    /// </summary>
    /// <param name="source">The source SIF image to encode</param>
    /// <returns>A HAG formatted image with compressed data</returns>
    public static Hag Encode(Sif source) {
        var result = new Hag {
            Header = new HagHeader() {
                Width = source.Header.Width,
                Height = source.Header.Height,
                Format = source.Header.Format,
            },
            Body = new HagBody()
        };

        var bodyData = new List<byte>();
        var uniquePixels = new Pixel[UNIQUE_PIXELS_SIZE];
        Pixel? prevPixel = null;
        int copyCount = 0;

        foreach (var currentPixel in source.Body.Pixels) {
            int currentPixelCode = currentPixel.GetCode() % UNIQUE_PIXELS_SIZE;

            if (prevPixel == null) {
                AddPixel(currentPixel);
                prevPixel = currentPixel;
                uniquePixels[currentPixelCode] = currentPixel;
                continue;
            }

            if (prevPixel.Equals(currentPixel)) {
                if (copyCount == 0) {
                    bodyData.Add(new byte());
                }
                copyCount++;
                bodyData[^1] = (byte)(HAG_COPY | Math.Min(copyCount, 61));
                if (copyCount == 61) copyCount = 0;
                continue;
            }

            copyCount = 0;
            var deltaPixel = currentPixel - prevPixel;

            if (uniquePixels[currentPixelCode]?.Equals(currentPixel) == true) {
                bodyData.Add((byte)(HAG_SET | currentPixelCode));
            }

            else if (0 < deltaPixel.Red && deltaPixel.Red < 4 &&
                     0 < deltaPixel.Green && deltaPixel.Green < 4 &&
                     0 < deltaPixel.Blue && deltaPixel.Blue < 4 && 
                     0 < deltaPixel.Alpha && deltaPixel.Alpha < 4) {
                bodyData.Add((byte)(
                    HAG_DELTA |
                    ((deltaPixel.Red & 0x03) << 4) |
                    ((deltaPixel.Green & 0x03) << 2) |
                    (deltaPixel.Blue & 0x03)
                ));
            }

            else if (0 < deltaPixel.Red && deltaPixel.Red < 64 &&
                     0 < deltaPixel.Green && deltaPixel.Green < 32 &&
                     0 < deltaPixel.Blue && deltaPixel.Blue < 32 &&
                     0 < deltaPixel.Alpha && deltaPixel.Alpha < 4) {
                bodyData.Add((byte)(HAG_BIG_DELTA | (deltaPixel.Red & 0x3F)));
                bodyData.Add((byte)(
                    ((deltaPixel.Green & 0x0F) << 4) |
                    (deltaPixel.Blue & 0x0F)
                ));
            } else {
                AddPixel(currentPixel);
            }

            prevPixel = currentPixel;
            uniquePixels[currentPixelCode] = currentPixel;
        }

        result.Body.Data = [.. bodyData];
        return result;

        void AddPixel(Pixel pixel) {
            bodyData.Add(source.Header.Format == ColorFormat.RGBA ? HAG_RGBA : HAG_RGB);
            bodyData.Add((byte)pixel.Red);
            bodyData.Add((byte)pixel.Green);
            bodyData.Add((byte)pixel.Blue);
            if (source.Header.Format == ColorFormat.RGBA)
                bodyData.Add((byte)pixel.Alpha);
        }
    }

    /// <summary>
    /// Decodes a HAG formatted image back into SIF format
    /// </summary>
    /// <param name="source">The HAG formatted image to decode</param>
    /// <returns>A SIF image containing the uncompressed pixel data</returns>
    public static Sif Decode(Hag source) {
        var result = new Sif {
            Header = source.Header,
            Body = new ImageBody()
        };

        var pixels = new List<Pixel>();
        var uniquePixels = new Pixel[UNIQUE_PIXELS_SIZE];
        Pixel? prevPixel = null;

        for (int i = 0; i < source.Body.Data.Length; i++) {
            byte currentByte = source.Body.Data[i];
            byte commandType = (byte)(currentByte & 0xC0);

            if (currentByte == HAG_RGB) {
                if (i > source.Body.Data.Length - 4)
                    continue;

                pixels.Add(new Pixel() {
                    Red = source.Body.Data[i + 1],
                    Green = source.Body.Data[i + 2],
                    Blue = source.Body.Data[i + 3],
                });

                i += 3;

                prevPixel = pixels.Last();
                uniquePixels[prevPixel.GetCode() % UNIQUE_PIXELS_SIZE] = prevPixel;

                continue;
            }

            if (currentByte == HAG_RGBA) {
                if (i > source.Body.Data.Length - 5)
                    continue;

                pixels.Add(new Pixel() {
                    Red = source.Body.Data[i + 1],
                    Green = source.Body.Data[i + 2],
                    Blue = source.Body.Data[i + 3],
                    Alpha = source.Body.Data[i + 4]
                });

                i += 4;

                prevPixel = pixels.Last();
                uniquePixels[prevPixel.GetCode() % UNIQUE_PIXELS_SIZE] = prevPixel;

                continue;
            }

            switch (commandType) {
                case HAG_COPY:
                    if (prevPixel == null)
                        continue;

                    int copyCount = currentByte & 0x3F;

                    for (int j = 0; j < copyCount; j++) {
                        pixels.Add(new Pixel {
                            Red = prevPixel.Red,
                            Green = prevPixel.Green,
                            Blue = prevPixel.Blue,
                            Alpha = prevPixel.Alpha
                        });
                    }
                    break;

                case HAG_DELTA:
                    if (prevPixel == null)
                        continue;

                    pixels.Add(new Pixel() {
                        Red = prevPixel.Red + ((currentByte >> 4) & 0x03),
                        Green = prevPixel.Green + ((currentByte >> 2) & 0x03),
                        Blue = prevPixel.Blue + (currentByte & 0x03),
                        Alpha = prevPixel.Alpha
                    });
                    break;

                case HAG_BIG_DELTA:
                    if (prevPixel == null || i == source.Body.Data.Length - 1)
                        continue;

                    byte nextByte = source.Body.Data[i + 1];

                    pixels.Add(new Pixel() {
                        Red = prevPixel.Red + (currentByte & 0x3F),
                        Green = prevPixel.Green + ((nextByte >> 4) & 0x0F),
                        Blue = prevPixel.Blue + (nextByte & 0x0F),
                        Alpha = prevPixel.Alpha
                    });

                    i++;
                    break;

                case HAG_SET:
                    int currentPixelCode = currentByte & 0x3F;
                    var storedPixel = uniquePixels[currentPixelCode % UNIQUE_PIXELS_SIZE];

                    if (storedPixel != null) {
                        pixels.Add(new Pixel {
                            Red = storedPixel.Red,
                            Green = storedPixel.Green,
                            Blue = storedPixel.Blue,
                            Alpha = storedPixel.Alpha
                        });
                    }
                    break;

                default:
                    continue;
            }

            prevPixel = pixels.LastOrDefault();

            if (prevPixel == null)
                continue;

            uniquePixels[prevPixel.GetCode() % UNIQUE_PIXELS_SIZE] = prevPixel;
        }

        result.Body.Pixels = pixels;
        return result;
    }
}