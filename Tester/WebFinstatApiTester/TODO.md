# WebFinstatApiTester — TODO

Browser-based equivalent of `DesktopFinstatApiTester`. Single ASP.NET Core project that hosts the C# `FinstatApi` / `FinstatApiCZ` clients behind a minimal-APIs surface and serves a Birko.Web front-end from `wwwroot/`.

**Reference docs:** https://finstat.github.io/api-documentation/

---

## Architecture

- **One project, one deployment.** `WebFinstatApiTester.csproj` (SDK=`Microsoft.NET.Sdk.Web`, `net10.0`) references `FinStatApi.csproj` and `FinStatApiCZ.csproj` (alias `CZ`). An MSBuild target runs `npm install && npm run build` before the C# build so `dotnet publish` produces a single deployable artifact.
- **Credentials flow.** Browser keeps settings in `localStorage` and sends them on every request as `X-Finstat-*` headers (`ApiKey`, `PrivateKey`, `StationID`, `StationName`, `TimeOut`, `Url`, `UrlCZ`, `ResponseType`). Backend binds them into a `FinstatCredentials` record and the `ApiClientFactory` constructs the correct SK/CZ client per request. `FinstatApiException` is translated to `ProblemDetails`.
- **UI.** Ribbon shell (`BAppShell` from `birko-web-shell`) mirroring the desktop tester's layout. Left sidebar shows response history (desktop's `ResponseItems`). Settings modal is reachable from the user-menu (desktop's Backstage).
- **Response viewer.** Three tabs matching desktop's object tree view:
  1. **Tree** — collapsible `name: value` nodes derived client-side by walking the parsed JSON, via Birko's `b-object-tree` / `b-json-viewer`. The desktop tester's Data/Headers tabs use the same component's XAML twin (`Birko.Xaml.Avalonia`'s `ObjectTree`), so both testers render a response graph identically: container nodes show their child count (`{2}` / `[5]`), values are type-colored, nulls show as `null`.
  2. **Raw JSON** — pretty-printed JSON.
  3. **Raw XML** — formatted XML when `ResponseType=xml`.

---

## Directory layout

```
Tester/WebFinstatApiTester/
├─ WebFinstatApiTester.csproj
├─ Program.cs
├─ Endpoints/
│   ├─ SkDetailEndpoints.cs
│   ├─ SkAutoEndpoints.cs
│   ├─ SkMonitoringEndpoints.cs
│   ├─ SkDistraintEndpoints.cs
│   ├─ SkDailyDiffEndpoints.cs
│   ├─ SkBankruptcyEndpoints.cs
│   ├─ SkStatementEndpoints.cs
│   ├─ SkReportingEndpoints.cs
│   ├─ CzDetailEndpoints.cs
│   └─ CzMonitoringEndpoints.cs
├─ Infrastructure/
│   ├─ FinstatCredentials.cs       # header model binder
│   └─ ApiClientFactory.cs          # builds SK/CZ clients per request
├─ package.json, build.js, dev-server.js, tsconfig.json, index.html
├─ src/
│   ├─ app.ts                       # registers components, mounts shell
│   ├─ shell/
│   │   └─ app-shell.ts             # extends BAppShell
│   ├─ settings/
│   │   ├─ settings-store.ts        # localStorage
│   │   └─ settings-modal.ts
│   ├─ history/
│   │   ├─ response-history.ts      # store + sidebar items
│   │   └─ response-viewer.ts       # tabs: Tree / Raw JSON / Raw XML
│   ├─ api/
│   │   └─ api-client.ts            # wraps birko-web-core ApiClient; injects headers
│   └─ pages/
│       ├─ sk/detail-page.ts        # v1
│       └─ …                        # v2/v3 pages
└─ wwwroot/                         # esbuild output
```

---

## Phased rollout

### V1 — Foundation + SK Detail

- [ ] **#1** csproj skeleton (`WebFinstatApiTester.csproj`, `Program.cs`, MSBuild npm target)
- [ ] **#2** Frontend build pipeline (`package.json`, `build.js` with Birko.Web aliases, `dev-server.js` with API proxy, `tsconfig.json`, `index.html`, `src/app.ts`)
- [ ] **#3** Credentials binding + `ApiClientFactory` (`Infrastructure/FinstatCredentials.cs`, `ApiClientFactory.cs`, exception → `ProblemDetails` mapper)
- [ ] **#4** `AppShell` (ribbon + left sidebar for history). Brand "FinStat API Tester". Ribbon tabs: Detail, Other, Monitoring, Distraint, DailyDiff, Statement, Reporting, CZ Detail, CZ Other, CZ Monitoring.
- [ ] **#5** Settings store + Settings modal (localStorage key `finstat_tester_settings`). Fields: ApiKey, PrivateKey, StationID (default `api-tester`), StationName (default `Api Tester Web`), TimeOut (3000), FinStatApiUrl (`https://www.finstat.sk/api`), FinStatApiUrlCZ (`https://cz.finstat.sk/api`), ResponseType (`xml` | `json`). Buttons: **Use** / **Use and Save** / **Cancel**.
- [ ] **#6** Typed TS API client. Wraps `birko-web-core` `ApiClient`, reads settings-store, attaches `X-Finstat-*` headers. Every successful/failed call appends to response-history store.
- [ ] **#7** SK Detail page — ICO input + 4 action buttons (Basic, Detail, Extended, Ultimate). Endpoints: `POST api/sk/detail/{basic|detail|extended|ultimate}` → `RequestBasic` / `RequestDetail` / `RequestExtendedDetail` / `RequestUltimateDetail`.
- [ ] **#8** Response history + viewer (Tree / Raw JSON / Raw XML tabs; Clear / Remove).
- [ ] **#9** Smoke test — `dotnet run`, browser at Kestrel port, enter test keys from https://finstat.github.io/api-documentation/, fire 4 detail calls against a known ICO, verify tree/raw display.

### V2 — Remaining SK endpoints

- [ ] **#10** AutoComplete + AutoLogin (`BaseApiClient.RequestAutocomplete`, `RequestAutoLogin`).
- [ ] **#11** Monitoring — full `ApiMonitoringClient` surface: `Add`, `Add(category)`, `Remove`, `Remove(category)`, `GetMonitorings`, `GetMonitorings(category)`, `GetReport`, `GetReport(category)`, `GetProceedings`, `AddDate`, `RemoveDate`, `GetDateMonitorings`, `GetDateReport`, `GetDateProceedings`, `GetCategories`.
- [ ] **#12** Distraint — `RequestDistraintSearch`, `RequestDistraintDetail(token, ids)`, `RequestDistraintResults`, `RequestDistraintResultsByToken`, `RequestDistraintStoredDetail`.
- [ ] **#13** Daily Diff family — `ApiDailyDiffClient`, `ApiDailyStatementDiffClient`, `ApiDailyStatement2014DiffClient`, `ApiDailyUltimateDiffClient`. Desktop writes files to disk; web streams bytes through the endpoint as a browser download. Includes `RequestStatementLegend` + `RequestStatement2014Legend`.
- [ ] **#14** Bankruptcies/Restructurings — `RequestPersonBankruptcyProceedings(name, surname, dateOfBirth)`, `RequestCompanyBankruptcyRestructuring(ico, name)`.
- [ ] **#15** Statement + Reporting — `RequestStatements(ico)`, `RequestStatementDetail(ico, year, template)`, `RequestStatementLegend(template, lang)`, `RequestTopics`, `RequestList(topic)`, `DownloadReportFile`.

### V3 — CZ endpoints

- [ ] **#16** CZ Detail (Basic/Detail/Premium/Elite), CZ AutoComplete/AutoLogin (inherited), CZ Monitoring (Add/Remove/GetMonitorings/GetReport/GetCategories — CZ has no proceedings or date variants).

---

## Open questions / decisions deferred

- File-download ergonomics for Daily Diff / Reporting: stream bytes via endpoint vs. server-side temp file. Current plan: stream.
- Whether to persist response history across reloads (localStorage / IndexedDB) or keep it in-memory only. Default: in-memory; revisit after V1.
- Whether to add syntax highlighting for Raw JSON/XML tabs. Default: skip in V1; tiny custom highlighter later if useful.
