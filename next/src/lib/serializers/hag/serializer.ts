import { ISerializer, SIF } from "@/lib/types";
import { Hag, HagCodec } from "./codec";

export class HagSerializer implements ISerializer {
  async serialize(source: Blob): Promise<SIF> {
    const buffer = await source.arrayBuffer();
    const data = new Uint8Array(buffer);

    if (data.length < 8) {
      throw new Error("Invalid HAG file: header too small");
    }

    const width = new DataView(buffer, 0, 4).getInt32(0, false);
    const height = new DataView(buffer, 4, 4).getInt32(0, false);

    const hag: Hag = {
      header: {
        width,
        height,
        format: data.length > 8 && data[8] === 0xff ? 1 : 0,
      },
      body: {
        data: data.slice(8),
      },
    };

    return HagCodec.decode(hag);
  }

  async deserialize(source: SIF): Promise<Blob> {
    const hag = HagCodec.encode(source);

    const headerBuffer = new ArrayBuffer(8);
    const headerView = new DataView(headerBuffer);
    headerView.setInt32(0, hag.header.width, false);
    headerView.setInt32(4, hag.header.height, false);

    const combinedArray = new Uint8Array(8 + hag.body.data.length);
    combinedArray.set(new Uint8Array(headerBuffer), 0);
    combinedArray.set(hag.body.data, 8);

    return new Blob([combinedArray], { type: "application/octet-stream" });
  }
}
