import { EnterpriseSection } from "./_sections/enterprise";
import { HeroSection } from "./_sections/hero";
import { InstallationSection } from "./_sections/installation";

export default function LandingPage() {
  return (
    <div className="flex flex-col items-center justify-center">
      <div className="gradient-bg bg-gradient-to-b from-white to-blue-50 w-full flex justify-center">
        <HeroSection />
      </div>
      <div className="border-y border-black overflow-clip w-full flex justify-center">
        <InstallationSection />
      </div>
      <div className="w-full flex justify-center bg-gray-50">
        <EnterpriseSection />
      </div>
    </div>
  );
}
