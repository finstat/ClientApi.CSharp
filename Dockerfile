# syntax=docker/dockerfile:1.7

# ─── Usage ───────────────────────────────────────────────────────────────────
# Build context is the monorepo parent (C:/Source) so the Birko.* shared projects
# under Birko\Framework (imported via $(BirkoSrc) in WebFinstatApiTester.csproj) resolve,
# and the esbuild alias map can pick up the Birko.Web.* TypeScript sources.
#
#     docker build -f ClientApi.CSharp/Dockerfile -t finstat-web-tester:latest C:/Source
#
# The frontend is built inside the image — no need to run `npm run build` on the host.
# Dockerfile.dockerignore (next to this file) scopes the context transfer to just the
# directories we need, keeping the rest of C:/Source out of the daemon.

# ─── Frontend stage: bundle wwwroot/app.js via esbuild ───────────────────────
FROM node:22-alpine AS frontend
WORKDIR /src

# The esbuild alias map in build.js resolves `birko-web-*` imports to these paths.
# Sources live under the Birko\Web bucket; BIRKO_WEB_SRC (set below) points the
# resolver straight at them inside the image instead of walking the tree.
COPY Birko/Web/Birko.Web.Core/       Birko/Web/Birko.Web.Core/
COPY Birko/Web/Birko.Web.Components/ Birko/Web/Birko.Web.Components/
COPY Birko/Web/Birko.Web.Shell/      Birko/Web/Birko.Web.Shell/

WORKDIR /src/ClientApi.CSharp/Tester/WebFinstatApiTester
COPY ClientApi.CSharp/Tester/WebFinstatApiTester/package.json \
     ClientApi.CSharp/Tester/WebFinstatApiTester/package-lock.json* \
     ./
RUN npm ci || npm install

COPY ClientApi.CSharp/Tester/WebFinstatApiTester/tsconfig.json \
     ClientApi.CSharp/Tester/WebFinstatApiTester/build.js \
     ./
COPY ClientApi.CSharp/Tester/WebFinstatApiTester/src/     ./src/
COPY ClientApi.CSharp/Tester/WebFinstatApiTester/wwwroot/ ./wwwroot/

ENV BIRKO_WEB_SRC=/src/Birko/Web
RUN node build.js --minify

# ─── Backend stage: dotnet publish ───────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# ClientApi.CSharp — the project under build.
COPY ClientApi.CSharp/global.json \
     ClientApi.CSharp/Directory.Build.props \
     ClientApi.CSharp/Directory.Build.targets \
     ClientApi.CSharp/
COPY ClientApi.CSharp/FinStatApi/                 ClientApi.CSharp/FinStatApi/
COPY ClientApi.CSharp/FinStatApiCZ/               ClientApi.CSharp/FinStatApiCZ/
COPY ClientApi.CSharp/Shared/                     ClientApi.CSharp/Shared/
COPY ClientApi.CSharp/Tester/WebFinstatApiTester/ ClientApi.CSharp/Tester/WebFinstatApiTester/

# Replace the wwwroot with the freshly-bundled frontend output (app.js + css + assets).
COPY --from=frontend \
     /src/ClientApi.CSharp/Tester/WebFinstatApiTester/wwwroot/ \
     ClientApi.CSharp/Tester/WebFinstatApiTester/wwwroot/

# Birko.* shared projects (under the Birko\Framework bucket) imported via $(BirkoSrc)
# in the csproj; BirkoSrc resolves to ..\Birko\Framework, i.e. /src/Birko/Framework.
COPY Birko/Framework/Birko.Contracts/                    Birko/Framework/Birko.Contracts/
COPY Birko/Framework/Birko.Configuration/                Birko/Framework/Birko.Configuration/
COPY Birko/Framework/Birko.Serialization/                Birko/Framework/Birko.Serialization/
COPY Birko/Framework/Birko.Security/                     Birko/Framework/Birko.Security/
COPY Birko/Framework/Birko.Security.Vault/               Birko/Framework/Birko.Security.Vault/
COPY Birko/Framework/Birko.Security.Vault.Configuration/ Birko/Framework/Birko.Security.Vault.Configuration/
COPY Birko/Framework/Birko.Health/                       Birko/Framework/Birko.Health/
COPY Birko/Framework/Birko.Validation/                   Birko/Framework/Birko.Validation/

# The NpmBuild MSBuild target runs `npm run build`; skip it — frontend is already bundled.
RUN dotnet publish ClientApi.CSharp/Tester/WebFinstatApiTester/WebFinstatApiTester.csproj \
        -c Release \
        -o /app/publish \
        --no-self-contained \
        /p:NpmBuildSkip=true \
        /p:DebugType=None \
        /p:DebugSymbols=false \
        /p:UseAppHost=false

# ─── Runtime stage ───────────────────────────────────────────────────────────
# Chiseled image: no shell, no package manager, runs as non-root `app` (UID 1654).
# Trade-off: cannot `docker exec -it … sh` for debugging — swap to the non-chiseled
# tag if you need that.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Dev client certificate used to reach zdrojak.eu / zdrojakcz.eu. The repo's
# certs/ directory is copied in — drop `dev-cert.pfx` there before building.
# Empty folder is fine; the resolver quietly skips when the file is absent.
# PASSWORD IS NOT BAKED IN: pass Security__DevCertificate__PfxPassword at run time.
COPY ClientApi.CSharp/Tester/WebFinstatApiTester/certs/ ./certs/

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    Security__DevCertificate__PfxPath=/app/certs/dev-cert.pfx

EXPOSE 8080

ENTRYPOINT ["dotnet", "WebFinstatApiTester.dll"]
