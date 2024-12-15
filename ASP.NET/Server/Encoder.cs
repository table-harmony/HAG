using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server {
    public class Pixel {
        public Int32 Red { get; set; } = 0;
        public Int32 Green { get; set; } = 0;
        public Int32 Blue { get; set; } = 0;

        // Overriding Equals to check if all attributes are the same
        public override bool Equals(object? obj) {
            if (obj is not Pixel other)
                return false;

            return this.Red == other.Red &&
                   this.Green == other.Green &&
                   this.Blue == other.Blue;
        }

        public static bool operator ==(Pixel? a, Pixel? b) {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Pixel? a, Pixel? b) {
            return !(a == b);
        }

        public int GetCode() {
            return Red * 3 + Green * 5 + Blue * 7;
        }

        public static Pixel operator -(Pixel? a, Pixel? b) {
            if (a is null && b is null)
                return new Pixel();

            if (a is null && b is not null)
                return b;

            if (b is null && a is not null)
                return a;

            return new Pixel {
                Red = Math.Abs(a.Red - b.Red),
                Green = Math.Abs(a.Green - b.Green),
                Blue = Math.Abs(a.Blue - b.Blue)
            };
        }

        public override string ToString() {
            return $"Red: {Red}, Green: {Green}, Blue: {Blue}";
        }
    }

    public class Header {
        public int Width { get; set; }
        public int Height { get; set; }

        public override string ToString() {
            return $"Width = {Width}, Height = {Height}";
        }
    }

    public class Encoder {
        private const int HEADER_SIZE = 8;

        private static Header GetHeader(byte[] data) {
            byte[] headerBytes = data
                .Take(HEADER_SIZE)
                .Reverse()
                .ToArray();

            int width = BitConverter.ToInt32(headerBytes, 0);
            int height = BitConverter.ToInt32(headerBytes, 4);

            Header header = new() {
                Width = width,
                Height = height
            };

            return header;    
        }

        public static byte[] Main(byte[] data) {
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

            for (int byteIndex = HEADER_SIZE; byteIndex < data.Length; byteIndex += 3) {

                currentPixel = new Pixel() {
                    Red = data[byteIndex],
                    Green = data[byteIndex + 1],
                    Blue = data[byteIndex + 2],
                };


                // COPY CASE
                if (prevPixel == currentPixel) {
                    if (copyCount == 0) {
                        resultBytes.Add(new byte());
                    }

                    copyCount++;

                    resultBytes[^1] = (byte)(HAG_COPY | copyCount);
                    
                    if (copyCount == 62) {
                        copyCount = 0;
                    }
                } else {
                    if (copyCount != 0)
                        copyCount = 0;
                    
                    // SET CASE
                    int currentPixelCode = currentPixel.GetCode() % 64;
                    if (uniquePixels[currentPixelCode] != null) {
                        resultBytes.Add((byte)(HAG_SET | currentPixelCode));
                        prevPixel = currentPixel;
                        continue;
                    } else {
                        uniquePixels[currentPixelCode] = currentPixel;
                    }

                    Pixel deltaPixel = currentPixel - prevPixel;

                    // DELTA CASE
                    if (Math.Abs(deltaPixel.Red) < 4 &&
                        Math.Abs(deltaPixel.Green) < 4 &&
                        Math.Abs(deltaPixel.Blue) < 4) {
                        resultBytes.Add((byte)(
                            (HAG_DELTA << 6) |
                            (deltaPixel.Red << 4) |
                            (deltaPixel.Green << 2) |
                            deltaPixel.Blue
                        ));
                    } 
                    // BIG DELTA CASE
                    else if (Math.Abs(deltaPixel.Red) < 64 &&
                        Math.Abs(deltaPixel.Green) < 32 &&
                        Math.Abs(deltaPixel.Blue) < 32) {
                        resultBytes.Add((byte)(
                            (HAG_BIG_DELTA << 6) | 
                            deltaPixel.Red
                        ));
                        resultBytes.Add((byte)(
                            (deltaPixel.Green << 4)
                            | deltaPixel.Blue
                        ));
                    } 
                    else {
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
}
