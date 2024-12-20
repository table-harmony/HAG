import "@/app/globals.css";
import { Rubik as FontSans } from "next/font/google";
import { Metadata, Viewport } from "next";
import { cn } from "@/lib/utils";
import { ContextProvider } from "@/components/context-provider";
import NextTopLoader from "nextjs-toploader";
import { siteConfig } from "@/config/site";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";

const fontSans = FontSans({
  subsets: ["latin"],
  weight: ["300", "400", "500", "600", "700"],
  variable: "--font-sans",
});

export const metadata: Metadata = {
  title: "Conify - Convert your files to various formats.",
  metadataBase: new URL(siteConfig.url),
  description: siteConfig.description,
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "white" },
    { media: "(prefers-color-scheme: dark)", color: "black" },
  ],
};

interface RootLayoutProps {
  children: React.ReactNode;
}

export default function RootLayout({ children }: RootLayoutProps) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body
        className={cn(
          "min-h-screen bg-background font-sans antialiased",
          fontSans.variable
        )}
      >
        <ContextProvider>
          <div className="relative flex min-h-screen flex-col">
            <NextTopLoader showSpinner={false} />
            <Header />
            <main className="flex-1 flex flex-col">{children}</main>
            <Footer />
          </div>
        </ContextProvider>
      </body>
    </html>
  );
}
