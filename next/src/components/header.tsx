"use client";

import Link from "next/link";
import { ImageIcon, UploadCloud } from "lucide-react";
import { Button } from "./ui/button";
import { usePathname } from "next/navigation";
import { siteConfig } from "@/config/site";

export function Header() {
  const pathname = usePathname();

  const isActive = (path: string) => pathname === path;

  return (
    <header className="sticky top-0 p-4 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container px-4 md:px-16 flex h-14 items-center">
        <div className="mr-4 flex">
          <Link href="/" className="mr-6 flex items-center space-x-2">
            <UploadCloud className="w-6 h-6" />
            <span className="hidden font-bold sm:inline-block">
              {siteConfig.name}
            </span>
          </Link>
          <nav className="flex items-center gap-2">
            <Button
              asChild
              variant={isActive("/convert/images") ? "secondary" : "ghost"}
            >
              <Link href="/convert/images">
                <ImageIcon className="w-4 h-4" />
                Images
              </Link>
            </Button>
          </nav>
        </div>
      </div>
    </header>
  );
}
