import { Button } from "@/components/ui/button";
import { siteConfig } from "@/config/site";
import { ChevronRightIcon, GithubIcon } from "lucide-react";
import Image from "next/image";
import Link from "next/link";

export function HeroSection() {
  return (
    <section className="container px-4 py-12 md:py-16 lg:py-28">
      <div className="flex gap-10 flex-col lg:flex-row justify-center items-center max-w-[1000px] mx-auto">
        <div className="flex flex-col gap-6 text-center lg:text-left">
          <div className="inline-flex h-8 items-center rounded-full border px-4 text-sm font-semibold self-center lg:self-start bg-primary text-primary-foreground">
            Discover Harmony
          </div>
          <div className="space-y-4">
            <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold tracking-tight">
              Simplify Your
              <br />
              Image Workflow
            </h1>
            <p className="text-base md:text-lg text-muted-foreground text-balance max-w-[350px] mx-auto lg:mx-0">
              {siteConfig.description}
            </p>
          </div>
          <div className="flex flex-col sm:flex-row gap-4 justify-center lg:justify-start">
            <Button asChild className="h-11 px-8">
              <Link href="/convert">
                Get Started <ChevronRightIcon className="ml-2 h-4 w-4" />
              </Link>
            </Button>
            <Button asChild className="h-11 px-8" variant="outline">
              <Link
                href={siteConfig.links.github}
                target="_blank"
                rel="noreferrer"
              >
                <GithubIcon className="w-4 h-4 mr-2" /> GitHub
              </Link>
            </Button>
          </div>
        </div>

        <div className="w-full max-w-[350px] mx-auto">
          <div className="-m-2 rounded-xl bg-neutral-900/5 p-2 ring-1 ring-inset ring-neutral-900/10 dark:bg-neutral-100/10 lg:-m-4 lg:rounded-2xl lg:p-4">
            <Image
              src="/logo.webp"
              className="rounded-xl"
              alt="Hero"
              width={350}
              height={350}
            />
          </div>
        </div>
      </div>
    </section>
  );
}
