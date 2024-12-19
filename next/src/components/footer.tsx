import Link from "next/link";
import { Github } from "lucide-react";
import { Button } from "@/components/ui/button";

export function Footer() {
  return (
    <footer className="border-t bg-background py-12 flex justify-center">
      <div className="container flex h-14 items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link
              href="https://github.com/tableharmony/HAG"
              aria-label="GitHub"
            >
              <Github className="h-4 w-4" />
            </Link>
          </Button>
        </div>

        <p className="text-sm text-muted-foreground">
          Copyright © {new Date().getFullYear()} Table Harmony. All rights
          reserved.
        </p>

        <Button className="flex items-center gap-2 cursor-default">
          <div className="h-2 w-2 rounded-full bg-[#00DC82] animate-pulse" />
          <span className="text-sm">All systems operational</span>
        </Button>
      </div>
    </footer>
  );
}
