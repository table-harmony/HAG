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

    for (const currentPixel of source.body.pixels) {
      const currentPixelCode = this.getPixelCode(currentPixel);

      if (!prevPixel) {
        this.addPixel(bodyData, currentPixel, source.header.format);
        prevPixel = currentPixel;
        uniquePixels[currentPixelCode] = currentPixel;
        continue;
      }

      if (this.pixelsEqual(prevPixel, currentPixel)) {
        if (copyCount === 0) {
          bodyData.push(0);
        }
        copyCount++;
        bodyData[bodyData.length - 1] = HAG_COPY | Math.min(copyCount, 61);
        if (copyCount === 61) copyCount = 0;
        continue;
      }

      copyCount = 0;
      const deltaPixel = this.subtractPixels(currentPixel, prevPixel);

      if (
        uniquePixels[currentPixelCode] &&
        this.pixelsEqual(uniquePixels[currentPixelCode]!, currentPixel)
      ) {
        bodyData.push(HAG_SET | currentPixelCode);
      } else if (this.isSmallDelta(deltaPixel)) {
        bodyData.push(
          HAG_DELTA |
            ((deltaPixel.red & 0x03) << 4) |
            ((deltaPixel.green & 0x03) << 2) |
            (deltaPixel.blue & 0x03)
        );
      } else if (this.isBigDelta(deltaPixel)) {
        bodyData.push(HAG_BIG_DELTA | (deltaPixel.red & 0x3f));
        bodyData.push(
          ((deltaPixel.green & 0x0f) << 4) | (deltaPixel.blue & 0x0f)
        );
      } else {
        this.addPixel(bodyData, currentPixel, source.header.format);
      }

      prevPixel = currentPixel;
      uniquePixels[currentPixelCode] = this.storePixel(currentPixel);
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

      if (currentByte === HAG_RGB || currentByte === HAG_RGBA) {
        const isRGBA = currentByte === HAG_RGBA;
        if (i > source.body.data.length - (isRGBA ? 5 : 4)) continue;

        const pixel: Pixel = {
          red: source.body.data[i + 1],
          green: source.body.data[i + 2],
          blue: source.body.data[i + 3],
          alpha: isRGBA ? source.body.data[i + 4] : undefined,
        };

        result.body.pixels.push(pixel);
        i += isRGBA ? 4 : 3;
        prevPixel = pixel;
        uniquePixels[this.getPixelCode(pixel)] = pixel;
        continue;
      }

      if (!prevPixel) continue;

      switch (commandType) {
        case HAG_COPY: {
          const copyCount = currentByte & 0x3f;
          for (let j = 0; j < copyCount; j++) {
            result.body.pixels.push(this.clonePixel(prevPixel));
          }
          break;
        }

        case HAG_DELTA: {
          const pixel: Pixel = {
            red: (prevPixel.red + ((currentByte >> 4) & 0x03)) & 0xff,
            green: (prevPixel.green + ((currentByte >> 2) & 0x03)) & 0xff,
            blue: (prevPixel.blue + (currentByte & 0x03)) & 0xff,
            alpha: prevPixel.alpha,
          };
          result.body.pixels.push(pixel);
          prevPixel = { ...pixel };
          uniquePixels[this.getPixelCode(pixel)] = { ...pixel };
          break;
        }

        case HAG_BIG_DELTA: {
          if (i === source.body.data.length - 1) continue;
          const nextByte = source.body.data[i + 1];

          const pixel: Pixel = {
            red: (prevPixel.red + (currentByte & 0x3f)) & 0xff,
            green: (prevPixel.green + ((nextByte >> 4) & 0x0f)) & 0xff,
            blue: (prevPixel.blue + (nextByte & 0x0f)) & 0xff,
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
            const pixel = { ...storedPixel };
            result.body.pixels.push(pixel);
            prevPixel = pixel;
          }
          break;
        }
      }
    }

    return result;
  }

  private static pixelsEqual(a: Pixel, b: Pixel): boolean {
    const aAlpha = a.alpha === undefined ? 255 : a.alpha;
    const bAlpha = b.alpha === undefined ? 255 : b.alpha;
    return (
      a.red === b.red &&
      a.green === b.green &&
      a.blue === b.blue &&
      aAlpha === bAlpha
    );
  }

  private static subtractPixels(a: Pixel, b: Pixel): Pixel {
    return {
      red: (a.red - b.red) & 0xff,
      green: (a.green - b.green) & 0xff,
      blue: (a.blue - b.blue) & 0xff,
      alpha:
        a.alpha !== undefined ? (a.alpha - (b.alpha ?? 255)) & 0xff : undefined,
    };
  }

  private static isSmallDelta(delta: Pixel): boolean {
    return (
      0 < delta.red &&
      delta.red < 4 &&
      0 < delta.green &&
      delta.green < 4 &&
      0 < delta.blue &&
      delta.blue < 4 &&
      0 < (delta.alpha ?? 0) &&
      (delta.alpha ?? 0) < 4
    );
  }

  private static isBigDelta(delta: Pixel): boolean {
    return (
      0 < delta.red &&
      delta.red < 64 &&
      0 < delta.green &&
      delta.green < 32 &&
      0 < delta.blue &&
      delta.blue < 32 &&
      0 < (delta.alpha ?? 0) &&
      (delta.alpha ?? 0) < 4
    );
  }

  private static getPixelCode(pixel: Pixel): number {
    return (
      (((pixel.red & 0xf0) << 4) |
        (pixel.green & 0xf0) |
        ((pixel.blue & 0xf0) >> 4)) %
      UNIQUE_PIXELS_SIZE
    );
  }

  private static addPixel(
    bodyData: number[],
    pixel: Pixel,
    format: ColorFormat
  ) {
    bodyData.push(format === ColorFormat.RGBA ? HAG_RGBA : HAG_RGB);
    bodyData.push(pixel.red);
    bodyData.push(pixel.green);
    bodyData.push(pixel.blue);
    if (format === ColorFormat.RGBA) {
      bodyData.push(pixel.alpha ?? 255);
    }
  }

  private static clonePixel(pixel: Pixel): Pixel {
    return {
      red: pixel.red & 0xff,
      green: pixel.green & 0xff,
      blue: pixel.blue & 0xff,
      alpha: pixel.alpha !== undefined ? pixel.alpha & 0xff : undefined,
    };
  }

  private static storePixel(pixel: Pixel): Pixel {
    const stored = this.clonePixel(pixel);
    // Ensure values are properly bounded
    stored.red &= 0xff;
    stored.green &= 0xff;
    stored.blue &= 0xff;
    if (stored.alpha !== undefined) {
      stored.alpha &= 0xff;
    }
    return stored;
  }
}
