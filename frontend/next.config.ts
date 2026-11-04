import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  devIndicators: false,
  allowedDevOrigins: ["*.trycloudflare.com", "192.168.1.23"],
  turbopack: {
    rules: {
      "*.css": { loaders: ["@tailwindcss/turbopack"], as: "*.css" },
    },
  },
  async rewrites() {
    return [{ source: "/api/:path*", destination: `${process.env.API_URL ?? "http://localhost:5200"}/api/:path*` }];
  },
};

export default nextConfig;
