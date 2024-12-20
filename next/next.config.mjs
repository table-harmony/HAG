/** @type {import('next').NextConfig} */
const nextConfig = {
  experimental: {
    serverActions: true,
  },
  webpack: (config) => {
    config.externals = [...(config.externals || []), "sharp"];
    return config;
  },
  serverRuntimeConfig: {
    maxDuration: 60,
  },
};

export default nextConfig;
