import { SerializerFactory } from "./serializers/factory";
import { SupportedFormat } from "./types";

export class ImageConverter {
  static async convert(
    file: File,
    targetFormat: SupportedFormat
  ): Promise<Blob> {
    try {
      const sourceFormat = this.getFormatFromExtension(file.name);
      const sourceSerializer = SerializerFactory.create(sourceFormat);
      const targetSerializer = SerializerFactory.create(targetFormat);

      const sif = await sourceSerializer.serialize(file);
      return await targetSerializer.deserialize(sif);
    } catch (error) {
      console.error("Conversion error:", error);
      throw new Error(`Failed to convert image: ${error}`);
    }
  }

  static getFormatFromExtension(filename: string): SupportedFormat {
    const ext = filename.split(".").pop()?.toLowerCase();
    if (!ext || !["png", "jpg", "jpeg", "webp", "hag"].includes(ext)) {
      throw new Error("Unsupported format");
    }
    return ext as SupportedFormat;
  }
}
