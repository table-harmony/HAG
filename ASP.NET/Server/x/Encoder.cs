using Server.x.Entities;

namespace Server.x;

public class Encoder
{
    private const int HEADER_SIZE = 8;

    public static byte[] Main(byte[] data)
    {
        List<byte> resultBytes = [];
        Pixel[] uniquePixels = new Pixel[64];

        resultBytes.AddRange(data.Take(HEADER_SIZE));

        Pixel? prevPixel = null;
        Pixel currentPixel;

        const byte HAG_COPY = 0xc0;
        const byte HAG_DELTA = 0x40;
        const byte HAG_BIG_DELTA = 0x80;
        const byte HAG_SET = 0x00;
        const byte HAG_RGB = 0xfe;

        int copyCount = 0;

        for (int byteIndex = HEADER_SIZE; byteIndex < data.Length; byteIndex += 3)
        {
            currentPixel = new Pixel()
            {
                Red = data[byteIndex],
                Green = data[byteIndex + 1],
                Blue = data[byteIndex + 2],
            };


            // COPY CASE
            if (prevPixel == currentPixel)
            {
                if (copyCount == 0)
                {
                    resultBytes.Add(new byte());
                }

                copyCount++;

                resultBytes[^1] = (byte)(HAG_COPY | copyCount);

                if (copyCount == 62)
                {
                    copyCount = 0;
                }
            }
            else
            {
                if (copyCount != 0)
                    copyCount = 0;

                // SET CASE
                int currentPixelCode = currentPixel.GetCode() % 64;
                if (uniquePixels[currentPixelCode] != null)
                {
                    resultBytes.Add((byte)(HAG_SET | currentPixelCode));
                    prevPixel = currentPixel;
                    continue;
                }
                else
                {
                    uniquePixels[currentPixelCode] = currentPixel;
                }

                Pixel deltaPixel = currentPixel - prevPixel;

                // DELTA CASE
                if (Math.Abs(deltaPixel.Red) < 4 &&
                    Math.Abs(deltaPixel.Green) < 4 &&
                    Math.Abs(deltaPixel.Blue) < 4)
                {
                    resultBytes.Add((byte)(
                        HAG_DELTA << 6 |
                        deltaPixel.Red << 4 |
                        deltaPixel.Green << 2 |
                        deltaPixel.Blue
                    ));
                }
                // BIG DELTA CASE
                else if (Math.Abs(deltaPixel.Red) < 64 &&
                    Math.Abs(deltaPixel.Green) < 32 &&
                    Math.Abs(deltaPixel.Blue) < 32)
                {
                    resultBytes.Add((byte)(
                        HAG_BIG_DELTA << 6 |
                        deltaPixel.Red
                    ));
                    resultBytes.Add((byte)(
                        deltaPixel.Green << 4
                        | deltaPixel.Blue
                    ));
                }
                else
                {
                    resultBytes.Add(HAG_RGB);
                    resultBytes.Add(data[byteIndex]);
                    resultBytes.Add(data[byteIndex + 1]);
                    resultBytes.Add(data[byteIndex + 2]);
                }
            }

            prevPixel = currentPixel;
        }

        return [.. resultBytes];
    }
}
