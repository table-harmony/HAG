using ImageProcessing.Core;
using static ImageProcessing.Core.Hag;

namespace ImageProcessing.Services;

public class HagCodec {
    private const byte HAG_COPY = 0xc0;
    private const byte HAG_DELTA = 0x40;
    private const byte HAG_BIG_DELTA = 0x80;
    private const byte HAG_SET = 0x00;
    private const byte HAG_RGB = 0xfe;
    private const byte HAG_RGBA = 0xff;

    private const int UNIQUE_PIXELS_SIZE = 64;

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

        void AddPixel(Pixel pixel) {
            if (pixel == null) {
                return;
            }

            if (result.Header.Format == ColorFormat.RGB)
                bodyData.Add(HAG_RGB);
            if (result.Header.Format == ColorFormat.RGBA)
                bodyData.Add(HAG_RGBA);

            bodyData.Add((byte)pixel.Red);
            bodyData.Add((byte)pixel.Green);
            bodyData.Add((byte)pixel.Blue);

            if (result.Header.Format == ColorFormat.RGBA)
                bodyData.Add((byte)pixel.Alpha);
        }

        foreach (var currentPixel in source.Body.Pixels) {
            if (prevPixel == null) {
                AddPixel(currentPixel);
                prevPixel = currentPixel;
                continue;
            }

            if (prevPixel == currentPixel) {
                if (copyCount == 0) {
                    bodyData.Add(new byte());
                }

                copyCount++;

                bodyData[^1] = (byte)(HAG_COPY | copyCount);

                if (copyCount == 61) {
                    copyCount = 0;
                }
            }
            else {
                copyCount = 0;

                //int currentPixelCode = currentPixel.GetCode() % UNIQUE_PIXELS_SIZE;
                //if (uniquePixels[currentPixelCode] == currentPixel) {
                //    bodyData.Add((byte)(HAG_SET | currentPixelCode));
                //    prevPixel = currentPixel;
                //    continue;
                //} else {
                //    uniquePixels[currentPixelCode] = currentPixel;
                //}

                var deltaPixel = currentPixel - prevPixel;

                if (IsSmallDelta(deltaPixel)) {
                    bodyData.Add((byte)(
                        HAG_DELTA |
                        (deltaPixel.Red & 0x03) << 4 |
                        (deltaPixel.Green & 0x03) << 2 |
                        (deltaPixel.Blue & 0x03)
                    ));
                } else if (IsBigDelta(deltaPixel)) {
                    bodyData.Add((byte)(
                        HAG_BIG_DELTA |
                        (deltaPixel.Red & 0x3F)
                    ));
                    bodyData.Add((byte)(
                        (deltaPixel.Green & 0x1F) << 4 |
                        (deltaPixel.Blue & 0x1F)
                    ));
                } else {
                    AddPixel(currentPixel);
                }
            }

            prevPixel = currentPixel;
        }

        result.Body.Data = [.. bodyData];
        return result;
    }

    private static bool IsSmallDelta(Pixel delta) =>
        delta.Red < 4 && 0 < delta.Red &&
        delta.Green < 4 && 0 < delta.Green &&
        delta.Blue < 4 && 0 < delta.Blue &&
        delta.Alpha< 4 && 0 < delta.Alpha;

    private static bool IsBigDelta(Pixel delta) =>
        delta.Red < 32 && 0 < delta.Red &&
        delta.Green < 16 && 0 < delta.Green &&
        delta.Blue < 16 && 0 < delta.Blue &&
        delta.Alpha < 32 && 0 < delta.Alpha;

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
            byte commandType = (byte)(currentByte & 0b11000000);

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

                //case HAG_SET:
                //    int currentPixelCode = (byte)(currentByte & 0x3F);
                //    if (uniquePixels[currentPixelCode] != null) {
                //        pixels.Add(uniquePixels[currentPixelCode]);
                //    }
                //    break;

                default:
                    continue;
            }

            prevPixel = pixels.Last();

            if (prevPixel == null)
                continue;

            //int prevPixelCode = prevPixel.GetCode() % UNIQUE_PIXELS_SIZE;
            //if (uniquePixels[prevPixelCode] == null)
            //    uniquePixels[prevPixelCode] = prevPixel;
        }

        result.Body.Pixels = pixels;
        return result;
    }
}