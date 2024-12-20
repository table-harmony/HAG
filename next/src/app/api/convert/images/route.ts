import { NextRequest, NextResponse } from "next/server";

import { SupportedImageFormat } from "@/lib/types";
import { ImageConverter } from "@/lib/image-converter";

import { fetchMutation } from "convex/nextjs";
import { api } from "../../../../../convex/_generated/api";

export async function POST(req: NextRequest) {
  try {
    const formData = await req.formData();
    const file = formData.get("file") as File;
    const targetFormat = formData.get("targetFormat") as SupportedImageFormat;

    if (!file || !targetFormat) {
      return NextResponse.json(
        { error: "Missing file or target format" },
        { status: 400 }
      );
    }

    const fileBlob = await ImageConverter.convert(
      file,
      targetFormat as SupportedImageFormat
    );

    try {
      await fetchMutation(api.data.updateImageData, {
        size: fileBlob.size,
        count: 1,
      });
    } catch (error) {
      console.error(error);
    }

    return new NextResponse(fileBlob, {
      headers: {
        "Content-Type": "application/octet-stream",
        "Content-Disposition": `attachment; filename="converted.${targetFormat}"`,
      },
    });
  } catch (error) {
    console.error(error);
    return NextResponse.json({ error: "Conversion failed" }, { status: 500 });
  }
}
