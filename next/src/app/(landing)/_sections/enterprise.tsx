import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { siteConfig } from "@/config/site";
import { ChevronRight, Zap } from "lucide-react";
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
            Convert Harmony
          </h2>
          <p className="text-muted-foreground text-lg">
            Convert your images to any format for free
          </p>
        </div>
        <Button asChild size="lg">
          <Link href="/convert">
            Get started <ChevronRight className="h-4 w-4" />
          </Link>
        </Button>
      </Card>
    </section>
  );
}
