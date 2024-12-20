import Link from "next/link";
import { Github } from "lucide-react";
import { Button } from "@/components/ui/button";
import { ModeToggle } from "./mode-toggle";
import { siteConfig } from "@/config/site";

export function Footer() {
  return (
    <footer className="border-t bg-background py-8 lg:py-12 flex justify-center">
      <div className="container flex flex-col lg:flex-row lg:h-14 items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link
              href={siteConfig.links.github}
              target="_blank"
              rel="noreferrer"
              aria-label="GitHub"
            >
              <Github className="h-4 w-4" />
            </Link>
          </Button>
        </div>

        <p className="text-sm text-muted-foreground text-center">
          Copyright © {new Date().getFullYear()} Table Harmony. All rights
          reserved.
        </p>

        <ModeToggle />
      </div>
    </footer>
  );
}
