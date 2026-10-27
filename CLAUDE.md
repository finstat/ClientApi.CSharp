# ClientApi.CSharp

FinStat API client libraries (`FinStatApi`, `FinStatApiCZ`, shared code in `Shared/`) and their testers
(`Tester/`: SK/CZ console testers, the Avalonia desktop tester, the web tester).

## Web tester deployment (Portainer)

The web tester runs from the `finstat-web-tester:latest` image, imported into Portainer as a tar. The full
how-to — build, export, the Portainer settings and how to check the certificate loaded — is in
`Tester/WebFinstatApiTester/DEV_README.md` § *Nasadenie cez Portainer*. The facts that are easy to get wrong:

- host port `5101` → container `8080`
- the dev certificate for zdrojak.eu / zdrojakcz.eu is `apitester.pfx` in `C:\clientWebCertificates\Certs` on the
  host; bind it as `/run/desktop/mnt/host/c/clientWebCertificates/Certs` → `/app/certs` (read-only), because the
  host runs Docker Desktop and Portainer rejects a `C:\` path
- set `Security__DevCertificate__PfxPath=/app/certs/apitester.pfx` (the image defaults to `dev-cert.pfx`) and
  `Security__DevCertificate__PfxPassword`
- build the image from `C:/Source` (`docker build -f ClientApi.CSharp/Dockerfile -t finstat-web-tester:latest C:/Source`)
  and export only `latest` (`docker save finstat-web-tester:latest -o finstat-web-tester.tar`)

## Tester downloads (wiki)

`.github/workflows/tester-downloads.yml` publishes the desktop tester zips and the web tester tar into the repo's
wiki (fixed URLs `https://raw.githubusercontent.com/wiki/finstat/ClientApi.CSharp/<file>`, index page `Downloads`),
deliberately **not** as release assets. It force-pushes the wiki as one orphan commit so old binaries don't
accumulate — hand-written wiki pages survive, their edit history doesn't. Each run rebuilds the desktop tester and
re-exports `ghcr.io/…/finstat-web-tester:latest` together; it reruns after every successful *Web tester image* run
on `master`. Files must stay under GitHub's 100 MB per-file limit (today ~39 MB zips, ~59 MB tar).

`web-tester-image.yml` prunes ghcr after each push: keeps every non-`sha-` tag and the newest 5 `sha-*` builds, and
protects the untagged child manifests of kept images (deleting those breaks pulls of the tagged image).
