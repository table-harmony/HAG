import sharp from "sharp";
import { ISerializer, SIF, ColorFormat } from "../types";

export class WebpSerializer implements ISerializer {
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
    const channels = source.header.format === ColorFormat.RGB ? 3 : 4;
    const data = new Uint8Array(source.body.pixels.length * channels);

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
      .webp()
      .toBuffer();

    return new Blob([processedBuffer], { type: "image/webp" });
  }
}