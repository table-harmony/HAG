import { ISerializer, SIF } from "@/lib/types";
import { Hag, HagCodec } from "./codec";

export class HagSerializer implements ISerializer {
  async serialize(source: Blob): Promise<SIF> {
    const buffer = await source.arrayBuffer();
    const data = new Uint8Array(buffer);

    if (data.length < 9) {
      throw new Error("Invalid HAG file: header too small");
    }

    const view = new DataView(buffer);
    const width = view.getInt32(0, false);
    const height = view.getInt32(4, false);
    const format = data[8];

    const hag: Hag = {
      header: {
        width,
        height,
        format: format === 0xff ? 1 : 0,
      },
      body: {
        data: data.slice(9),
      },
    };

    return HagCodec.decode(hag);
  }

  async deserialize(source: SIF): Promise<Blob> {
    const hag = HagCodec.encode(source);

    const headerBuffer = new ArrayBuffer(9);
    const headerView = new DataView(headerBuffer);
    headerView.setInt32(0, hag.header.width, false);
    headerView.setInt32(4, hag.header.height, false);

    const headerArray = new Uint8Array(headerBuffer);
    headerArray[8] = hag.header.format === 1 ? 0xff : 0xfe;

    const combinedArray = new Uint8Array(9 + hag.body.data.length);
    combinedArray.set(headerArray, 0);
    combinedArray.set(hag.body.data, 9);

    return new Blob([combinedArray], { type: "image/hag" });
  }
}
