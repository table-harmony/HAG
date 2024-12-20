"use client";

import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { CopyIcon, DownloadIcon } from "lucide-react";
import Link from "next/link";
import { useState } from "react";

export function InstallationSection() {
  const [selectedOS, setSelectedOS] = useState<"mac" | "windows">("windows");

  const handleDownload = () => {
    const a = document.createElement("a");
    a.href =
      selectedOS === "windows"
        ? "/downloads/HagViewer.exe"
        : "/downloads/Troll.txt";
    a.download = selectedOS === "windows" ? "Hag Viewer.exe" : "Troll.txt";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.location.reload();
  };

  return (
    <section className="container px-4 py-16 md:py-24">
      <div className="max-w-[800px] mx-auto text-center space-y-8">
        <h2 className="text-3xl md:text-4xl font-bold tracking-tight">
          Install HAG{" "}
          <span className="text-muted-foreground font-normal">v1.0.0</span>
        </h2>
        <p className="text-muted-foreground text-balance max-w-[350px] mx-auto">
          HAG Viewer is a tool to view your Harmonic Advanced Graphics (
          <Link
            className="underline-offset-2 underline"
            href="/hag-specification"
          >
            HAG
          </Link>
          ) files.
        </p>

        <div className="flex justify-center gap-2">
          <Button
            variant={selectedOS === "mac" ? "default" : "outline"}
            onClick={() => setSelectedOS("mac")}
            className="rounded-full"
          >
            MacOS/Linux
          </Button>
          <Button
            variant={selectedOS === "windows" ? "default" : "outline"}
            onClick={() => setSelectedOS("windows")}
            className="rounded-full"
          >
            Windows
          </Button>
        </div>

        <Card className="flex flex-col items-center justify-center p-4 gap-4 max-w-2xl mx-auto">
          <Button onClick={handleDownload}>
            <DownloadIcon className="mr-2 h-4 w-4" />
            Download
          </Button>
          <p className="text-muted-foreground text-sm">
            Download the installer and run it to install HAG on your computer.
          </p>
        </Card>
      </div>
    </section>
  );
}
