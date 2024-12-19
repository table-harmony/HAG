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
import { Upload, FileType, CheckCircle2, Trash2 } from "lucide-react";
import { SupportedFormat } from "@/lib/types";
import { toast } from "sonner";

type FileWithFormat = {
  file: File;
  format: SupportedFormat | null;
};

const supportedFormats: SupportedFormat[] = [
  "png",
  "jpg",
  "jpeg",
  "webp",
  "hag",
];

export function ImageConverter() {
  const [files, setFiles] = useState<FileWithFormat[]>([]);
  const [converting, setConverting] = useState(false);
  const [progress, setProgress] = useState(0);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    let files = Array.from(e.target.files || []);

    if (files.length === 0) {
      setFiles([]);
      return;
    }

    files.forEach((file) => {
      try {
        const format = file.name.split(".")[1] as SupportedFormat;

        if (!supportedFormats.includes(format)) {
          throw new Error("Unsupported format");
        }
      } catch (err) {
        toast.error(`Cannot read file ${file.name}`);
        files = files.filter((f) => f !== file);
      }
    });

    const newFiles = files.map((file) => ({
      file,
      format: file.name.split(".")[1] as SupportedFormat,
    }));

    setFiles((prev) => [...prev, ...newFiles]);
  };

  const handleFormatChange = (index: number, format: SupportedFormat) => {
    setFiles((prev) =>
      prev.map((item, i) => (i === index ? { ...item, format } : item))
    );
  };

  const handleRemoveFile = (index: number) => {
    setFiles((prev) => prev.filter((_, i) => i !== index));
  };

  const handleConvert = async () => {
    const filesToConvert = files.filter((f) => f.format !== null);
    if (filesToConvert.length === 0) {
      toast.error(
        "Please select at least one file and choose a conversion format."
      );
      return;
    }

    setConverting(true);
    setProgress(0);

    try {
      const convertedFiles = [];
      for (let i = 0; i < filesToConvert.length; i++) {
        const { file, format } = filesToConvert[i];
        const formData = new FormData();
        formData.append("file", file);
        formData.append("targetFormat", format as string);

        const response = await fetch("/api/convert", {
          method: "POST",
          body: formData,
        });

        if (!response.ok) {
          throw new Error(response.statusText || "Conversion failed");
        }

        const blob = await response.blob();
        convertedFiles.push({
          name: `${file.name.split(".")[0]}.${format}`,
          blob,
        });

        setProgress(((i + 1) / filesToConvert.length) * 100);
      }

      // Create a zip file containing all converted files
      const JSZip = (await import("jszip")).default;
      const zip = new JSZip();
      convertedFiles.forEach((file) => {
        zip.file(file.name, file.blob);
        setFiles((prev) => prev.filter((f) => f.file.name !== file.name));
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

      toast.success(`Successfully converted ${filesToConvert.length} file(s).`);
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
            onChange={handleFileChange}
            accept=".png,.jpg,.jpeg,.hag,.bmp,.webp,.qoi"
            multiple
          />
        </label>
      </div>

      {files.length > 0 && (
        <div className="space-y-4">
          <h3 className="font-semibold">Selected Files:</h3>
          <ul className="space-y-2">
            {files.map((file, index) => (
              <li
                key={index}
                className="flex items-center space-x-2 border rounded-md p-3"
              >
                <span className="flex-grow truncate">{file.file.name}</span>
                <Select
                  defaultValue={file.format || "png"}
                  onValueChange={(value) =>
                    handleFormatChange(index, value as SupportedFormat)
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
                  onClick={() => handleRemoveFile(index)}
                  aria-label={`Remove ${file.file.name}`}
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
        disabled={files.length === 0 || converting}
        className="w-full"
      >
        {converting ? (
          <FileType className="mr-2 h-4 w-4 animate-spin" />
        ) : (
          <CheckCircle2 className="mr-2 h-4 w-4" />
        )}
        Convert images
      </Button>
    </div>
  );
}
