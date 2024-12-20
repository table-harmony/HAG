export enum ColorFormat {
  RGB,
  RGBA,
}

export interface ImageHeader {
  width: number;
  height: number;
  format: ColorFormat;
}

export interface Pixel {
  red: number;
  green: number;
  blue: number;
  alpha?: number;
}

export interface ImageBody {
  pixels: Pixel[];
}

export interface SIF {
  header: ImageHeader;
  body: ImageBody;
}

export type SupportedImageFormat = "png" | "jpg" | "jpeg" | "webp" | "hag";

export interface ISerializer {
  serialize(source: Blob): Promise<SIF>;
  deserialize(source: SIF): Promise<Blob>;
}
