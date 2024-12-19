import sharp from "sharp";
import { ISerializer, SIF, ColorFormat } from "../types";

export class JpegSerializer implements ISerializer {
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
        format: ColorFormat.RGB,
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
      });
    }

    return result;
  }

  async deserialize(source: SIF): Promise<Blob> {
    const data = new Uint8Array(source.body.pixels.length * 3);

    source.body.pixels.forEach((pixel, index) => {
      const offset = index * 3;
      data[offset] = pixel.red;
      data[offset + 1] = pixel.green;
      data[offset + 2] = pixel.blue;
    });

    const processedBuffer = await sharp(data, {
      raw: {
        width: source.header.width,
        height: source.header.height,
        channels: 3,
      },
    })
      .jpeg()
      .toBuffer();

    return new Blob([processedBuffer], { type: "image/jpeg" });
  }
}
