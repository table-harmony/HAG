import { ColorFormat, Pixel, SIF } from "@/lib/types";

interface HagHeader {
  width: number;
  height: number;
  format: ColorFormat;
}

interface HagBody {
  data: Uint8Array;
}

export interface Hag {
  header: HagHeader;
  body: HagBody;
}

const HAG_COPY = 0xc0;
const HAG_DELTA = 0x40;
const HAG_BIG_DELTA = 0x80;
const HAG_SET = 0x00;
const HAG_RGB = 0xfe;
const HAG_RGBA = 0xff;

const UNIQUE_PIXELS_SIZE = 63;

export class HagCodec {
  static encode(source: SIF): Hag {
    const result: Hag = {
      header: {
        width: source.header.width,
        height: source.header.height,
        format: source.header.format,
      },
      body: {
        data: new Uint8Array(),
      },
    };

    const bodyData: number[] = [];
    const uniquePixels: (Pixel | null)[] = new Array(UNIQUE_PIXELS_SIZE).fill(
      null
    );
    let prevPixel: Pixel | null = null;
    let copyCount = 0;

    const addPixel = (pixel: Pixel) => {
      if (result.header.format === ColorFormat.RGB) {
        bodyData.push(HAG_RGB);
      }
      if (result.header.format === ColorFormat.RGBA) {
        bodyData.push(HAG_RGBA);
      }

      bodyData.push(pixel.red);
      bodyData.push(pixel.green);
      bodyData.push(pixel.blue);

      if (result.header.format === ColorFormat.RGBA) {
        bodyData.push(pixel.alpha ?? 255);
      }
    };

    const getPixelCode = (pixel: Pixel): number => {
      return (
        (pixel.red * 3 +
          pixel.green * 5 +
          pixel.blue * 7 +
          (pixel.alpha ?? 255) * 11) %
        UNIQUE_PIXELS_SIZE
      );
    };

    for (const currentPixel of source.body.pixels) {
      const currentPixelCode = getPixelCode(currentPixel);

      if (!prevPixel) {
        addPixel(currentPixel);
        prevPixel = currentPixel;
        uniquePixels[currentPixelCode] = currentPixel;
        continue;
      }

      if (this.pixelsEqual(prevPixel, currentPixel)) {
        if (copyCount === 0) {
          bodyData.push(0);
        }

        copyCount++;
        bodyData[bodyData.length - 1] = HAG_COPY | copyCount;

        if (copyCount === 61) {
          copyCount = 0;
        }
      } else {
        copyCount = 0;

        if (
          uniquePixels[currentPixelCode] &&
          this.pixelsEqual(uniquePixels[currentPixelCode]!, currentPixel)
        ) {
          bodyData.push(HAG_SET | currentPixelCode);
          prevPixel = currentPixel;
          continue;
        } else {
          uniquePixels[currentPixelCode] = currentPixel;
        }

        const deltaPixel = this.subtractPixels(currentPixel, prevPixel);

        if (this.isSmallDelta(deltaPixel)) {
          bodyData.push(
            HAG_DELTA |
              ((deltaPixel.red & 0x03) << 4) |
              ((deltaPixel.green & 0x03) << 2) |
              (deltaPixel.blue & 0x03)
          );
        } else if (this.isBigDelta(deltaPixel)) {
          bodyData.push(HAG_BIG_DELTA | (deltaPixel.red & 0x3f));
          bodyData.push(
            ((deltaPixel.green & 0x1f) << 4) | (deltaPixel.blue & 0x1f)
          );
        } else {
          addPixel(currentPixel);
        }
      }

      prevPixel = currentPixel;
    }

    result.body.data = new Uint8Array(bodyData);
    return result;
  }

  static decode(source: Hag): SIF {
    const result: SIF = {
      header: source.header,
      body: {
        pixels: [],
      },
    };

    const uniquePixels: (Pixel | null)[] = new Array(UNIQUE_PIXELS_SIZE).fill(
      null
    );
    let prevPixel: Pixel | null = null;

    for (let i = 0; i < source.body.data.length; i++) {
      const currentByte = source.body.data[i];
      const commandType = currentByte & 0xc0;

      if (currentByte === HAG_RGB) {
        if (i > source.body.data.length - 4) continue;

        const pixel: Pixel = {
          red: source.body.data[i + 1],
          green: source.body.data[i + 2],
          blue: source.body.data[i + 3],
        };

        result.body.pixels.push(pixel);
        i += 3;

        prevPixel = pixel;
        uniquePixels[this.getPixelCode(pixel)] = pixel;
        continue;
      }

      if (currentByte === HAG_RGBA) {
        if (i > source.body.data.length - 5) continue;

        const pixel: Pixel = {
          red: source.body.data[i + 1],
          green: source.body.data[i + 2],
          blue: source.body.data[i + 3],
          alpha: source.body.data[i + 4],
        };

        result.body.pixels.push(pixel);
        i += 4;

        prevPixel = pixel;
        uniquePixels[this.getPixelCode(pixel)] = pixel;
        continue;
      }

      if (!prevPixel) continue;

      switch (commandType) {
        case HAG_COPY: {
          const copyCount = currentByte & 0x3f;
          for (let j = 0; j < copyCount; j++) {
            result.body.pixels.push({ ...prevPixel });
          }
          break;
        }

        case HAG_DELTA: {
          const pixel: Pixel = {
            red: prevPixel.red + ((currentByte >> 4) & 0x03),
            green: prevPixel.green + ((currentByte >> 2) & 0x03),
            blue: prevPixel.blue + (currentByte & 0x03),
            alpha: prevPixel.alpha,
          };
          result.body.pixels.push(pixel);
          prevPixel = pixel;
          uniquePixels[this.getPixelCode(pixel)] = pixel;
          break;
        }

        case HAG_BIG_DELTA: {
          if (i === source.body.data.length - 1) continue;
          const nextByte = source.body.data[i + 1];

          const pixel: Pixel = {
            red: prevPixel.red + (currentByte & 0x3f),
            green: prevPixel.green + ((nextByte >> 4) & 0x0f),
            blue: prevPixel.blue + (nextByte & 0x0f),
            alpha: prevPixel.alpha,
          };
          result.body.pixels.push(pixel);
          i++;
          prevPixel = pixel;
          uniquePixels[this.getPixelCode(pixel)] = pixel;
          break;
        }

        case HAG_SET: {
          const currentPixelCode = currentByte & 0x3f;
          const storedPixel = uniquePixels[currentPixelCode];
          if (storedPixel) {
            result.body.pixels.push({ ...storedPixel });
            prevPixel = storedPixel;
          }
          break;
        }
      }
    }

    return result;
  }

  private static pixelsEqual(a: Pixel, b: Pixel): boolean {
    return (
      a.red === b.red &&
      a.green === b.green &&
      a.blue === b.blue &&
      (a.alpha ?? 255) === (b.alpha ?? 255)
    );
  }

  private static subtractPixels(a: Pixel, b: Pixel): Pixel {
    return {
      red: a.red - b.red,
      green: a.green - b.green,
      blue: a.blue - b.blue,
      alpha: (a.alpha ?? 255) - (b.alpha ?? 255),
    };
  }

  private static isSmallDelta(delta: Pixel): boolean {
    return (
      delta.red < 4 &&
      delta.red > 0 &&
      delta.green < 4 &&
      delta.green > 0 &&
      delta.blue < 4 &&
      delta.blue > 0 &&
      (delta.alpha ?? 0) < 16 &&
      (delta.alpha ?? 0) > 0
    );
  }

  private static isBigDelta(delta: Pixel): boolean {
    return (
      delta.red < 64 &&
      delta.red > 0 &&
      delta.green < 32 &&
      delta.green > 0 &&
      delta.blue < 32 &&
      delta.blue > 0 &&
      (delta.alpha ?? 0) < 16 &&
      (delta.alpha ?? 0) > 0
    );
  }

  private static getPixelCode(pixel: Pixel): number {
    return (
      (pixel.red * 3 +
        pixel.green * 5 +
        pixel.blue * 7 +
        (pixel.alpha ?? 255) * 11) %
      UNIQUE_PIXELS_SIZE
    );
  }
}
