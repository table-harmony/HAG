import sharp from "sharp";
import { ISerializer, SIF, ColorFormat } from "../types";

export class PngSerializer implements ISerializer {
  async serialize(source: Blob): Promise<SIF> {
    const buffer = Buffer.from(await source.arrayBuffer());
    const image = sharp(buffer);
    const metadata = await image.metadata();
    const { data, info } = await image
      .raw()
      .toBuffer({ resolveWithObject: true });

    const result: SIF = {
      header: {
        width: metadata.width!,
        height: metadata.height!,
        format: metadata.channels === 3 ? ColorFormat.RGB : ColorFormat.RGBA,
      },
      body: {
        pixels: [],
      },
    };

    for (let i = 0; i < data.length; i += info.channels) {
      result.body.pixels.push({
        red: data[i],
        green: data[i + 1],
        blue: data[i + 2],
        alpha: info.channels === 4 ? data[i + 3] : 255,
      });
    }

    return result;
  }

  async deserialize(source: SIF): Promise<Blob> {
    const pixelCount = source.header.width * source.header.height;
    const channels = source.header.format === ColorFormat.RGB ? 3 : 4;
    const dataSize = pixelCount * channels;

    const data = new Uint8Array(dataSize);

    source.body.pixels.forEach((pixel, index) => {
      const offset = index * channels;
      data[offset] = pixel.red;
      data[offset + 1] = pixel.green;
      data[offset + 2] = pixel.blue;
      if (channels === 4) {
        data[offset + 3] = pixel.alpha ?? 255;
      }
    });

    const processedBuffer = await sharp(data, {
      raw: {
        width: source.header.width,
        height: source.header.height,
        channels,
      },
    })
      .png()
      .toBuffer();

    return new Blob([processedBuffer], { type: "image/png" });
  }
}
