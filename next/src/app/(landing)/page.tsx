import { EnterpriseSection } from "./_sections/enterprise";
import { HeroSection } from "./_sections/hero";
import { InstallationSection } from "./_sections/installation";

export default function LandingPage() {
  return (
    <div className="flex flex-col items-center justify-center">
      <div className="w-full flex justify-center items-center">
        <HeroSection />
      </div>
      <div className="border-y border-black dark:border-white overflow-clip w-full flex justify-center">
        <InstallationSection />
      </div>
      <div className="w-full flex justify-center">
        <EnterpriseSection />
      </div>
    </div>
  );
}
