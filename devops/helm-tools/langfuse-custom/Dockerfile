# --- Stage 1: Builder ---
FROM node:20-bullseye-slim AS builder

WORKDIR /app

# Install system deps for git + prisma
RUN apt-get update && apt-get install -y git openssh-client && rm -rf /var/lib/apt/lists/*

# Enable pnpm
RUN corepack enable pnpm

# Clone source (specific tag)
RUN git clone --branch v3.108.0 https://github.com/langfuse/langfuse.git .

# Install dependencies without running postinstall scripts yet
RUN pnpm install --frozen-lockfile --ignore-scripts

# Generate Prisma client first
WORKDIR /app/packages/shared
RUN npx prisma generate

# Add dummy envs so validation works
ENV DATABASE_URL="postgres://dummy:dummy@localhost:5432/dummy"
ENV DIRECT_URL="postgres://dummy:dummy@localhost:5432/dummy"
RUN pnpm prisma validate

# Build shared package
RUN pnpm --filter @langfuse/shared build

# Build web with dummy envs
WORKDIR /app/web
ENV NEXTAUTH_SECRET="dummysecret"
ENV NEXTAUTH_URL="http://localhost:3000"
ENV SALT="dummy-salt"
ENV CLICKHOUSE_URL="http://localhost:8123"
ENV CLICKHOUSE_USER="default"
ENV CLICKHOUSE_PASSWORD="dummy"

ENV NODE_OPTIONS="--max-old-space-size=6144"
ENV NEXT_DISABLE_ESLINT=1
ENV NEXT_DISABLE_TYPECHECK=1

RUN pnpm build

# Patch static assets for subpath (/langfuse)
RUN find .next -type f -name "*.html" -exec sed -i 's|"/_next|"/langfuse/_next|g' {} \; ; \
    find .next -type f -name "*.js"   -exec sed -i 's|/_next|/langfuse/_next|g' {} \; ; \
    find .next -type f -name "*.js"   -exec sed -i 's|/icon.svg|/langfuse/icon.svg|g' {} \; ; \
    find .next -type f -name "*.js"   -exec sed -i 's|/favicon.ico|/langfuse/favicon.ico|g' {} \;

# --- Stage 2: Final runtime ---
FROM langfuse/langfuse:3.108.0

WORKDIR /app/web

# Copy only built assets, keep entrypoint.sh and runtime
COPY --from=builder /app/web/.next ./web/.next
COPY --from=builder /app/web/public ./web/public

# Subpath env vars
ENV BASE_PATH=/langfuse
ENV NEXT_PUBLIC_BASE_PATH=/langfuse
ENV NEXTAUTH_URL=https://teachin.westeurope.cloudapp.azure.com/langfuse
