"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Upload, CheckCircle2, Trash2, ImageUpIcon } from "lucide-react";
import { SupportedImageFormat } from "@/lib/types";
import { toast } from "sonner";

type ImageWithFormat = {
  image: File;
  format: SupportedImageFormat | null;
};

const supportedFormats: SupportedImageFormat[] = ["png", "jpg", "webp", "hag"];

const MAX_IMAGES = 5;

export function ImageConverter() {
  const [images, setImages] = useState<ImageWithFormat[]>([]);
  const [converting, setConverting] = useState(false);
  const [progress, setProgress] = useState(0);

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    let selectedImages = Array.from(e.target.files || []);

    if (selectedImages.length === 0) {
      setImages([]);
      return;
    }

    if (selectedImages.length + images.length > MAX_IMAGES) {
      toast.error(`You can only upload up to ${MAX_IMAGES} images at a time.`);
    }

    selectedImages.forEach((image) => {
      try {
        const format = getFormat(image);

        if (!isSupportedFormat(format)) {
          throw new Error("Unsupported format");
        }
      } catch (err) {
        toast.error(`Cannot read image ${image.name}`);
        selectedImages = selectedImages.filter((f) => f !== image);
      }
    });

    const newImages = selectedImages.map((image) => ({
      image,
      format: getFormat(image),
    }));

    setImages((prev) => [...prev, ...newImages].slice(0, MAX_IMAGES));
  };

  const handleFormatChange = (index: number, format: SupportedImageFormat) => {
    setImages((prev) =>
      prev.map((item, i) => (i === index ? { ...item, format } : item))
    );
  };

  const handleRemoveImage = (index: number) => {
    setImages((prev) => prev.filter((_, i) => i !== index));
  };

  const handleConvert = async () => {
    const imagesToConvert = images.filter((f) => f.format !== null);

    if (imagesToConvert.length === 0) {
      toast.error(
        "Please select at least one file and choose a conversion format."
      );
      return;
    }

    setConverting(true);
    setProgress(0);

    try {
      const convertedImages = [];

      for (let i = 0; i < imagesToConvert.length; i++) {
        const { image: file, format } = imagesToConvert[i];

        const formData = new FormData();
        formData.append("file", file);
        formData.append("targetFormat", format as string);

        const response = await fetch("/api/convert/images", {
          method: "POST",
          body: formData,
        });

        if (!response.ok) {
          throw new Error(response.statusText || "Conversion failed");
        }

        const blob = await response.blob();
        convertedImages.push({
          name: `${file.name.split(".")[0]}.${format}`,
          blob,
        });

        setProgress(((i + 1) / imagesToConvert.length) * 100);
      }

      // Create a zip file containing all converted files
      const JSZip = (await import("jszip")).default;
      const zip = new JSZip();
      convertedImages.forEach((image) => {
        zip.file(image.name, image.blob);
        setImages((prev) =>
          prev.filter((f) => f.image.stream !== image.blob.stream)
        );
      });
      const content = await zip.generateAsync({ type: "blob" });

      // Download the zip file
      const url = URL.createObjectURL(content);
      const a = document.createElement("a");
      a.href = url;
      a.download = `converted_images.zip`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);

      toast.success(
        `Successfully converted ${imagesToConvert.length} image(s).`
      );
    } catch (err) {
      console.error(err);
      toast.error("An error occurred during the conversion process.");
    } finally {
      setConverting(false);
      setProgress(0);
    }
  };

  return (
    <div className="w-full max-w-3xl mx-auto space-y-8">
      <div className="flex items-center justify-center w-full">
        <label
          htmlFor="fileInput"
          className="flex flex-col items-center justify-center w-full h-64 border-2 border-dashed rounded-lg cursor-pointer bg-muted/50 hover:bg-muted/80 dark:hover:bg-muted/30"
        >
          <div className="flex flex-col items-center justify-center pt-5 pb-6">
            <Upload className="w-8 h-8 mb-4 text-muted-foreground" />
            <p className="mb-2 text-sm text-muted-foreground">
              <span className="font-semibold">Click to upload</span> or drag and
              drop
            </p>
            <p className="text-xs text-muted-foreground">
              PNG, JPG, WEBP, HAG.
            </p>
          </div>
          <input
            id="fileInput"
            type="file"
            className="hidden"
            onChange={handleImageChange}
            accept=".png,.jpg,.jpeg,.hag,.bmp,.webp,.qoi"
            multiple
          />
        </label>
      </div>

      {images.length > 0 && (
        <div className="space-y-4">
          <h3 className="font-semibold">Selected Files:</h3>
          <ul className="space-y-2">
            {images.map((file, index) => (
              <li
                key={index}
                className="flex items-center space-x-2 border rounded-md p-3"
              >
                <span className="flex-grow truncate">{file.image.name}</span>
                <Select
                  defaultValue={file.format || "png"}
                  onValueChange={(value) =>
                    handleFormatChange(index, value as SupportedImageFormat)
                  }
                >
                  <SelectTrigger className="w-[180px]">
                    <SelectValue placeholder="Select format" />
                  </SelectTrigger>
                  <SelectContent>
                    {supportedFormats.map((format) => (
                      <SelectItem key={format} value={format}>
                        {format.toUpperCase()}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={() => handleRemoveImage(index)}
                  aria-label={`Remove ${file.image.name}`}
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </li>
            ))}
          </ul>
        </div>
      )}

      {converting && (
        <div className="space-y-2">
          <Progress value={progress} className="w-full" />
          <p className="text-sm text-center text-muted-foreground">
            {`Converting... ${Math.round(progress)}%`}
          </p>
        </div>
      )}

      <Button
        onClick={handleConvert}
        disabled={images.length === 0 || converting}
        className="w-full select-none"
      >
        {converting ? (
          <ImageUpIcon className="mr-2 h-4 w-4 animate-spin" />
        ) : (
          <CheckCircle2 className="mr-2 h-4 w-4" />
        )}
        Convert images
      </Button>
    </div>
  );
}

function isSupportedFormat(format: string): format is SupportedImageFormat {
  if (format === "jpeg") return true;
  return supportedFormats.includes(format as SupportedImageFormat);
}

function getFormat(file: File): SupportedImageFormat {
  const format = file.name.split(".").pop();
  if (format === "jpeg") return "jpg";
  return format as SupportedImageFormat;
}
