import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { siteConfig } from "@/config/site";
import { ChevronRight } from "lucide-react";
import Link from "next/link";

export function EnterpriseSection() {
  return (
    <section className="container px-4 py-16 md:py-24">
      <Card className="max-w-[500px] mx-auto p-8 space-y-6">
        <div className="inline-flex h-6 items-center rounded-full bg-[#00DC82] px-3 text-sm font-medium text-black">
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
        <Button
          asChild
          size="lg"
          className="group bg-[#00DC82] text-black hover:bg-[#00DC82]/90"
        >
          <Link href="/">
            Get started{" "}
            <ChevronRight className="ml-2 h-4 w-4 transition-transform group-hover:translate-x-1" />
          </Link>
        </Button>
      </Card>
    </section>
  );
}
