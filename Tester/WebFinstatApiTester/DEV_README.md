# WebFinstatApiTester — dev príručka

## Čo to je

Webová (browser-based) verzia desktop testera pre FinStat API. Jeden ASP.NET Core projekt (`WebFinstatApiTester`, `net10.0`, SDK `Microsoft.NET.Sdk.Web`) ktorý:

- **Backend**: hostuje C# klientov `FinStatApi` (SK) a `FinStatApiCZ` (CZ) za minimal-API endpointmi (`/api/sk/*`, `/api/cz/*`, `/api/whoami`, `/api/settings/validate`, `/health`).
- **Frontend**: Birko.Web ribbon shell (TypeScript) v `wwwroot/`, bundlovaný cez esbuild do jedného `app.js`.
- **Credentials flow**: browser drží nastavenia v `localStorage` a posiela ich ako `X-Finstat-*` hlavičky (`ApiKey`, `PrivateKey`, `StationID`, `StationName`, `TimeOut`, `Url`, `UrlCZ`, `ResponseType`, `Lang`) → backend ich bindne do `FinstatCredentials` a `ApiClientFactory` postaví správneho klienta per request.
- **Nasadenie**: jeden artefakt (`dotnet publish`) alebo Docker image `finstat-web-tester:latest`.

Referencia: https://finstat.github.io/api-documentation/

---

## Závislosti na kompiláciu

### 1. Runtime / SDK

- **.NET 10 SDK** (`global.json` pinuje verziu).
- **Node.js 22+** a **npm** — MSBuild target `NpmBuild` v `.csproj` automaticky spustí `npm install` (ak chýba `node_modules`) a `npm run build:dev` / `npm run build` pred dotnet buildom.
  - Preskočiť sa dá cez `/p:NpmBuildSkip=true` (Docker to používa, keďže frontend buildne v samostatnej stage).
- **npm balíčky** (devDeps, `package.json`):
  - `esbuild ^0.24.0`, `typescript ^5.7.0`, `@types/node ^22.0.0`

### 2. Sibling .NET projekty

Importované cez `$(BirkoSrc)\Birko.X\*.projitems` (shared projects, nie NuGet!). Všetky sú v monorepe [Birko-Framework/Birko.Framework](https://github.com/Birko-Framework/Birko.Framework):

- [Birko.Contracts](https://github.com/Birko-Framework/Birko.Framework/tree/main/Birko.Contracts)
- [Birko.Configuration](https://github.com/Birko-Framework/Birko.Framework/tree/main/Birko.Configuration)
- [Birko.Serialization](https://github.com/Birko-Framework/Birko.Framework/tree/main/Birko.Serialization)
- [Birko.Security](https://github.com/Birko-Framework/Birko.Framework/tree/main/Birko.Security)
- [Birko.Security.Vault](https://github.com/Birko-Framework/Birko.Framework/tree/main/Birko.Security.Vault)
- [Birko.Security.Vault.Configuration](https://github.com/Birko-Framework/Birko.Framework/tree/main/Birko.Security.Vault.Configuration)
- [Birko.Health](https://github.com/Birko-Framework/Birko.Framework/tree/main/Birko.Health) (pre `/health`)
- [Birko.Validation](https://github.com/Birko-Framework/Birko.Framework/tree/main/Birko.Validation) (cherry-pick: `Core/*.cs`, `Rules/*.cs`, `Fluent/*.cs`)

Cestu určuje MSBuild property `$(BirkoSrc)` z `Directory.Build.props`. Poradie rezolúcie:

1. `/p:BirkoSrc=D:\dev\Birko\Framework` — CLI override
2. `BIRKO_SRC` environment variable
3. Default: `$(MSBuildThisFileDirectory)..\Birko\Framework` = framework bucket (`C:\Source\Birko\Framework\`)

### 3. Sibling TypeScript projekty (pre esbuild alias map)

Rezolvujú sa cez `BIRKO_WEB_SRC` env var v `build.js`; bez env var `build.js` vylezie po stromčeku hore a nájde checkout `Birko\Web` (žiadna absolútna cesta sa necommituje):

- [Birko.Web.Core](https://github.com/Birko-Framework/Birko.Web/tree/main/Birko.Web.Core) (`birko-web-core/*`)
- [Birko.Web.Components](https://github.com/Birko-Framework/Birko.Web/tree/main/Birko.Web.Components) (`birko-web-components/*`, + CSS `tokens.css`, `reset.css`)
- [Birko.Web.Shell](https://github.com/Birko-Framework/Birko.Web/tree/main/Birko.Web.Shell) (`birko-web-shell/*`)

> .NET a TS build používajú **dve rôzne** env vars, lebo ukazujú na rôzne buckety: `BIRKO_SRC` → `Birko\Framework` (.NET), `BIRKO_WEB_SRC` → `Birko\Web` (esbuild). Pri štandardnom layoute `C:\Source\Birko\{Framework,Web}` netreba nastavovať ani jednu.

### 4. Interné project referencie

- `FinStatApi/FinStatApi.csproj`
- `FinStatApiCZ/FinStatApiCZ.csproj` (alias `CZ`)

### Build lokálne

```bash
cd C:\Source\ClientApi.CSharp\Tester\WebFinstatApiTester
dotnet build                # spustí npm install + npm run build:dev + dotnet build
dotnet run                  # http://localhost:5xxx
```

Ak máš sibling Birko repá **inde ako `C:\Source\`**, nastav `BIRKO_SRC` a `BIRKO_WEB_SRC`:

```powershell
# PowerShell — BIRKO_SRC pre .NET Import, BIRKO_WEB_SRC pre esbuild alias map
$env:BIRKO_SRC     = "D:\dev\Birko\Framework"
$env:BIRKO_WEB_SRC = "D:\dev\Birko\Web"
dotnet build

# alebo jednorazovo cez CLI property (len .NET)
dotnet build /p:BirkoSrc=D:\dev\Birko\Framework
```

---

## Kde sú uložené nastavenia

### A) Backend konfigurácia (ASP.NET)

Poradie zdrojov (neskorší prebije predošlého):

1. **`appsettings.json`** (gitované, bez secretov) — základy: logging, `Security:DevCertificate` defaulty (`IssuerName = root_ca_dev_zdrojak.eu`, `DevHosts = [zdrojak.eu, zdrojakcz.eu, localhost]`).
2. **`appsettings.Development.json`** (gitignored) — lokálny override, kopíruje sa zo šablóny `appsettings.Development.json.example`. Sem sa dáva napr. lokálny `Security:DevCertificate:Fingerprint`.
3. **LocalVault** (ak `LOCAL_VAULT_ENABLED=true`) — overlay na `IConfiguration`. Kľúče `Security--DevCertificate--Fingerprint` sa mapujú na `Security:DevCertificate:Fingerprint`.
4. **Environment variables** — napr. `Security__DevCertificate__PfxPassword` (heslo k PFX, nikdy nie v súbore).

Kľúčové configuračné sekcie:

- `Logging:LogLevel`
- `Security:DevCertificate`:
  - `Fingerprint` (SHA-1, 40 hex znakov) — primárny spôsob lookupu klient certu.
  - `IssuerName` — fallback keď chýba fingerprint.
  - `DevHosts` — substrings URL ktoré triggernú injekciu dev certu.
  - `PfxPath` — cesta k PKCS#12 súboru (potrebné v Linux kontajneri — vo Windowse sa default číta z cert store).
  - `PfxPassword` — heslo k PFX (cez env var alebo Vault, nie do súboru).
  - `PfxBase64` — alternatíva k PfxPath, PFX bajty priamo v configu/Vaulte.

### B) Frontend (browser)

Per-user nastavenia testera (API key, private key, station, URLs, response type) — uložené v **`localStorage`** pod kľúčom definovaným v `src/settings/settings-store.ts`. Posielajú sa na backend ako `X-Finstat-*` hlavičky. Pred-validácia cez `POST /api/settings/validate`.

### C) Certifikáty

- `Tester/WebFinstatApiTester/certs/` — sem sa vloží `dev-cert.pfx` (mimo gitu). V Dockeri sa priečinok kopíruje do `/app/certs/` a `PfxPath` je nastavený na `/app/certs/dev-cert.pfx`.

---

## Ako sa buildne Docker

Dockerfile: `C:\Source\ClientApi.CSharp\Dockerfile` — multi-stage, self-contained, frontend sa buildne **vnútri image**.

### Príkaz

```bash
docker build -f ClientApi.CSharp/Dockerfile -t finstat-web-tester:latest C:/Source
```

**Dôležité**: build context je **`C:/Source`** (monorepo parent), nie projekt! Je to preto, aby si sibling `Birko.*` shared projekty (cez `..\..\..\Birko.X`) a TypeScript aliasy (`Birko.Web.*`) sedeli. Rozsah kontextu zužuje `ClientApi.CSharp/Dockerfile.dockerignore`.

### Stages

1. **`frontend`** (`node:22-alpine`):
   - Skopíruje `Birko.Web.Core`, `Birko.Web.Components`, `Birko.Web.Shell` do `/src/Birko/Web/`.
   - Skopíruje `package.json`, `tsconfig.json`, `build.js`, `src/`, `wwwroot/` testera.
   - `npm ci || npm install`.
   - Nastaví `BIRKO_WEB_SRC=/src/Birko/Web` a spustí `node build.js --minify` → `wwwroot/app.js` + CSS + hash stamping v `index.html`.

2. **`build`** (`mcr.microsoft.com/dotnet/sdk:10.0`):
   - Skopíruje `ClientApi.CSharp/` (global.json, Directory.Build.*, FinStatApi, FinStatApiCZ, Shared, Tester/WebFinstatApiTester).
   - Prekryje `wwwroot/` výstupom z `frontend` stage.
   - Skopíruje sibling `Birko.*` projekty (`Birko.Contracts`, `Birko.Configuration`, `Birko.Serialization`, `Birko.Security`, `Birko.Security.Vault`, `Birko.Security.Vault.Configuration`, `Birko.Health`, `Birko.Validation`).
   - `dotnet publish -c Release -o /app/publish --no-self-contained /p:NpmBuildSkip=true`.

3. **`runtime`** (`mcr.microsoft.com/dotnet/aspnet:10.0`):
   - Skopíruje publish výstup do `/app`.
   - Skopíruje `Tester/WebFinstatApiTester/certs/` → `/app/certs/` (môže byť prázdny — resolver len ticho preskočí).
   - ENV: `ASPNETCORE_URLS=http://+:8080`, `ASPNETCORE_ENVIRONMENT=Production`, `Security__DevCertificate__PfxPath=/app/certs/dev-cert.pfx`.
   - `EXPOSE 8080`, `ENTRYPOINT ["dotnet", "WebFinstatApiTester.dll"]`.

### Export image do tar (offline deploy)

```bash
docker save finstat-web-tester:latest -o finstat-web-tester.tar
```

(Súbor `finstat-web-tester.tar` vzniká v koreni repa; v gite nie je.)

### Run

```bash
docker run -d -p 8080:8080 \
  -e LOCAL_VAULT_ENABLED=true \
  -e LOCAL_VAULT_ADDR=https://vault.internal:8200 \
  -e LOCAL_VAULT_TOKEN=<token> \
  -e LOCAL_VAULT_DOMAIN=finstat \
  -e LOCAL_VAULT_ENVIRONMENT=prod \
  -e Security__DevCertificate__PfxPassword=<password> \
  -v /host/certs:/app/certs:ro \
  finstat-web-tester:latest
```

### Nasadenie cez Portainer (náš server)

Image sa do Portainera importuje ako tar (Images → Import), tag iba `latest`:

```bash
docker save finstat-web-tester:latest -o finstat-web-tester.tar
```

Hotový tar z posledného buildu na `master` je aj na priamom odkaze (workflow *Tester downloads*):
<https://raw.githubusercontent.com/wiki/finstat/ClientApi.CSharp/finstat-web-tester.tar.gz> (gzip — Portainer ho
importuje priamo, netreba rozbaľovať).

Containers → Add container:

| Nastavenie | Hodnota |
|---|---|
| Image | `finstat-web-tester:latest`, **Always pull the image** vypnuté (image je importovaný, nie z registry) |
| Port | host `5101` → container `8080` (TCP) |
| Volume (Bind, read-only) | host `/run/desktop/mnt/host/c/clientWebCertificates/Certs` → container `/app/certs` |
| Env `Security__DevCertificate__PfxPath` | `/app/certs/apitester.pfx` (certifikát sa volá `apitester`, nie `dev-cert`) |
| Env `Security__DevCertificate__PfxPassword` | heslo k PFX |

Alternatíva bez volume: namiesto bindu a `PfxPath` nastav `Security__DevCertificate__PfxBase64` na celý `.pfx` v
base64 (`[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\clientWebCertificates\Certs\apitester.pfx"))`,
na Linuxe `base64 -w 0 apitester.pfx`); heslo ostáva v `PfxPassword`. Keď sú nastavené obe, vyhráva `PfxBase64`
a log ukáže `loaded cert from PfxBase64`.
| Restart policy | Unless stopped |

- Host beží Docker Desktop (Linux engine): cesta `C:\clientWebCertificates\Certs` v Portaineri zlyhá s
  `invalid volume specification` — Portainer posiela cestu enginu bez prekladu, preto
  `/run/desktop/mnt/host/c/...`.
- Tester potom beží na `http://<host>:5101`; API kľúče sa nezadávajú v Portaineri, ale v nastaveniach testera v prehliadači.
- Kontrola: po prvom volaní na zdrojak.eu má log kontajnera riadok `DevCertificate … loaded cert from …`.

---

## Odkiaľ sa ťahajú Vault kľúče

### Mechanizmus

Na štarte v `Program.cs` sa volá:

```csharp
builder.Configuration.AddLocalVaultConfiguration("finstat-apitester");
```

Toto je **no-op**, pokým `LOCAL_VAULT_ENABLED=true`. Ak je enabled, overlayuje `IConfiguration` KV secretmi z HashiCorp Vaultu.

### Environment variables (rovnaké mená ako originálny LocalVault NuGet)

| Premenná | Účel | Default |
|---|---|---|
| `LOCAL_VAULT_ENABLED` | master switch (`true`/`false`) | `false` (vypnuté) |
| `LOCAL_VAULT_ADDR` | Vault server URL | `http://localhost:8200` |
| `LOCAL_VAULT_TOKEN` | Vault token (ak prázdny → skip) | — |
| `LOCAL_VAULT_USER` | user scope (prebije project) | prázdny |
| `LOCAL_VAULT_DOMAIN` | prefix (napr. `finstat`) | prázdny |
| `LOCAL_VAULT_ENVIRONMENT` | env suffix (`dev`, `prod`, …) | prázdny |

### Cesty v KV (poradie = priorita, neskorší prebije)

Pre projekt `finstat-apitester`, s `DOMAIN=finstat` a `ENVIRONMENT=prod`:

1. `finstat/projects/defaults`
2. `finstat/projects/defaults.prod`
3. `finstat/projects/finstat-apitester`
4. `finstat/projects/finstat-apitester.prod`
5. `finstat/users/<user>/finstat-apitester` *(len ak je nastavený `LOCAL_VAULT_USER`)*
6. `finstat/users/<user>/finstat-apitester.prod`

### Mapovanie kľúčov na config

Vault KV fields používajú `--` ako oddeľovač hierarchie, ktorý sa mapuje na `:` v `IConfiguration`:

| Vault field | Config key | .NET model |
|---|---|---|
| `Security--DevCertificate--Fingerprint` | `Security:DevCertificate:Fingerprint` | `DevCertificateSettings.Fingerprint` |
| `Security--DevCertificate--PfxBase64` | `Security:DevCertificate:PfxBase64` | `DevCertificateSettings.PfxBase64` |
| `Security--DevCertificate--PfxPassword` | `Security:DevCertificate:PfxPassword` | `DevCertificateSettings.PfxPassword` |

### Čo sa reálne ťahá

Primárne **dev klient certifikát** pre volania na `zdrojak.eu` / `zdrojakcz.eu`:

- `Fingerprint` (SHA-1 thumbprint) — ak Vault poskytne, resolver hľadá cert v store podľa thumbprintu.
- `PfxBase64` + `PfxPassword` — alternatíva, keď cert nemá žiť na disku (typické pre Linux kontajner).

### Diagnostika

Pri štarte sa loguje:

```
LocalVault: ENABLED=<val>, ADDR=<val>, DOMAIN=<val>, ENVIRONMENT=<val>, TOKEN=<set|unset>
```

Ak `ENABLED≠true` alebo `TOKEN` je prázdny, Vault sa nekontaktuje.

---

## Rýchly checklist pre nového developera

- [ ] Nainštalovať .NET 10 SDK, Node.js 22+.
- [ ] Naklonovať `ClientApi.CSharp` (do `C:/Source/`) **a všetky Birko.* repá do `C:/Source/Birko/Framework/`** — viď clone script nižšie.
- [ ] Skopírovať `appsettings.Development.json.example` → `appsettings.Development.json`.
- [ ] (Voliteľne) získať `dev-cert.pfx` a vložiť do `Tester/WebFinstatApiTester/certs/`.
- [ ] (Voliteľne) nastaviť `LOCAL_VAULT_*` env vars ak chcem ťahať fingerprint z Vaultu.
- [ ] `dotnet run` v `Tester/WebFinstatApiTester/`.

### Clone všetkých potrebných repozitárov

```bash
# Birko.* shared projekty idú do bucketu Birko/Framework
mkdir -p C:/Source/Birko
cd C:/Source/Birko
git clone https://github.com/Birko-Framework/Birko.Framework.git Framework   # alebo kdekoľvek, len nastav BIRKO_SRC

# Birko.Web.* TypeScript projekty idú do bucketu Birko/Web
cd C:/Source/Birko
git clone https://github.com/Birko-Framework/Birko.Web.git Web               # alebo kdekoľvek, len nastav BIRKO_WEB_SRC

# a samotný ClientApi.CSharp do rootu C:/Source (ak ho ešte nemáš)
cd C:/Source
git clone <interné-URL>/ClientApi.CSharp.git
```
