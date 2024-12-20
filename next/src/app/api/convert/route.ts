import { NextRequest, NextResponse } from "next/server";

import { ImageConverter } from "@/lib/converter";
import { SupportedFormat } from "@/lib/types";
import { fetchMutation } from "convex/nextjs";
import { api } from "../../../../convex/_generated/api";

export async function POST(req: NextRequest) {
  try {
    const formData = await req.formData();
    const file = formData.get("file") as File;
    const targetFormat = formData.get("targetFormat") as SupportedFormat;

    if (!file || !targetFormat) {
      return NextResponse.json(
        { error: "Missing file or target format" },
        { status: 400 }
      );
    }

    const fileBlob = await ImageConverter.convert(
      file,
      targetFormat as SupportedFormat
    );

    try {
      await fetchMutation(api.data.updateData, {
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
