"use client";

import { api } from "../../../../convex/_generated/api";
import { useQuery } from "convex/react";

import { ImageConverter } from "@/components/image-converter";

import { formatNumber } from "@/lib/format";
import { formatFileSize } from "@/lib/format";

export default function ImageConverterPage() {
  const data = useQuery(api.data.getImageData);

  return (
    <div className="flex flex-col items-center justify-center">
      <div className="container px-4 py-8 md:py-12 space-y-10">
        <div className="mx-auto max-w-[800px] text-center space-y-6">
          <div className="space-y-4">
            <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold tracking-tight">
              Convert your images
            </h1>
            <p className="text-base md:text-lg text-muted-foreground text-balance max-w-[600px] mx-auto">
              Convert your images to various formats with our free online
              converter. Support for PNG, JPG, WEBP and HAG formats.
            </p>
          </div>
        </div>
        <ImageConverter />
        <p className="text-center text-lg md:text-xl font-semibold text-muted-foreground mt-10">
          We&apos;ve already converted {formatNumber(data?.count ?? 0)} images
          with a total size of {formatFileSize(data?.size ?? 0)}{" "}
        </p>
      </div>
    </div>
  );
}
