import { ISerializer, SupportedFormat } from "../types";

import { PngSerializer } from "./png-serializer";
import { JpegSerializer } from "./jpeg-serializer";
import { WebpSerializer } from "./webp-serializer";
import { HagSerializer } from "./hag/serializer";

export class SerializerFactory {
  static create(format: SupportedFormat): ISerializer {
    switch (format) {
      case "png":
        return new PngSerializer();
      case "jpeg":
      case "jpg":
        return new JpegSerializer();
      case "webp":
        return new WebpSerializer();
      case "hag":
        return new HagSerializer();
      default:
        throw new Error(`Unsupported format: ${format}`);
    }
  }
}
