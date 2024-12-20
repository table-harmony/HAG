import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { siteConfig } from "@/config/site";
import { ChevronRight, FileIcon, ImageIcon, Zap } from "lucide-react";
import Link from "next/link";

export function EnterpriseSection() {
  return (
    <section className="container px-4 py-16 md:py-24">
      <Card className="max-w-[500px] mx-auto p-8 space-y-6">
        <div className="inline-flex h-8 items-center rounded-full bg-primary px-4 text-sm font-medium text-primary-foreground">
          <Zap className="mr-2 h-4 w-4" />
          New
        </div>
        <div className="space-y-2">
          <h2 className="text-3xl font-semibold tracking-tight">
            {siteConfig.name}
          </h2>
          <p className="text-muted-foreground text-lg">
            {siteConfig.description}
          </p>
        </div>
        <div className="flex w-full flex-col md:flex-row justify-center gap-4">
          <Button asChild size="lg" className="w-full">
            <Link href="/convert/images">
              <ImageIcon className="h-4 w-4" />
              Images
            </Link>
          </Button>
        </div>
      </Card>
    </section>
  );
}
