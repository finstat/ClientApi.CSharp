# certs

Drop `dev-cert.pfx` here for a local Docker build that needs to reach zdrojak.eu / zdrojakcz.eu.
`*.pfx` is git-ignored, so the certificate never reaches the repo or the CI-built image on GHCR.
The folder is tracked only because the Dockerfile copies it; the app skips the certificate when it is absent.
The password is never baked in: pass `Security__DevCertificate__PfxPassword` at run time.
