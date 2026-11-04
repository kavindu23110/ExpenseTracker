FROM node:22-slim AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
# Next.js fixes rewrites at build time, so the API address has to be known here.
ARG API_URL
ENV API_URL=$API_URL
RUN npm run build

FROM node:22-slim
WORKDIR /app
ENV NODE_ENV=production PORT=3000 HOSTNAME=0.0.0.0
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
COPY --from=build /app/public ./public
EXPOSE 3000
USER node
CMD ["node", "server.js"]
