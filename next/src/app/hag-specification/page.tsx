import Image from "next/image";

export default function SpecificationPage() {
  return (
    <div className="flex flex-col items-center justify-center">
      <div className="container px-4 py-8 md:py-12 space-y-10">
        <div className="flex items-center justify-center flex-col">
          <div className="flex flex-col text-center space-y-4 mb-12">
            <h1 className="text-4xl md:text-5xl lg:text-6xl font-bold tracking-tight">
              HAG Specification
            </h1>
            <p className="text-lg text-muted-foreground text-balance max-w-[600px]">
              A comprehensive document that outlines the features and
              capabilities of the HAG (Harmonic Advanced Graphics) format.
            </p>
          </div>
          <div className="w-full max-w-[1000px] mx-auto">
            <div className="-m-2 rounded-xl bg-neutral-900/5 p-2 ring-1 ring-inset ring-neutral-900/10 dark:bg-neutral-100/10 lg:-m-4 lg:rounded-2xl lg:p-4">
              <Image
                src="/specification.png"
                alt="Specification"
                width={1000}
                height={1000}
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
