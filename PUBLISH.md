# Publish

Releases are driven by **git tags** (`v0.1.0`, `v0.1.0-preview.4`, …).  
Workflow: [`.github/workflows/publish.yml`](.github/workflows/publish.yml).

```bash
./scripts/set-version.sh 0.1.0-preview.4   # optional: sync files in the repo
git commit -am "chore: version 0.1.0-preview.4"
git tag v0.1.0-preview.4
git push origin main --tags
```

Or run **Actions → publish → Run workflow** and type the version (no `v`).

## Where packages go

| Ecosystem | Destination | Secret / setup |
|-----------|-------------|----------------|
| .NET | [nuget.org](https://www.nuget.org) | `NUGET_API_KEY` |
| JS | [npmjs.com](https://www.npmjs.com) (`@clavi/*`) | `NPM_TOKEN` (automation token) |
| Python | [pypi.org](https://pypi.org) (`clavi-admin`) | `PYPI_API_TOKEN` |
| Java | Maven Central | opt-in: repo variable `PUBLISH_MAVEN=true` + `MAVEN_USERNAME`, `MAVEN_PASSWORD`, `GPG_PRIVATE_KEY`, `GPG_PASSPHRASE` |
| Go | module proxy (from this git tag) | no upload — tag is enough |

Create the secrets under the GitHub repo **Settings → Secrets and variables → Actions**.

### NuGet.org

1. nuget.org → API Keys → create a key scoped to push new packages / `Clavi.Sdk.*`.
2. Store it as `NUGET_API_KEY`.
3. First publish may need you to confirm the package ownership email from nuget.org.

### npm

1. npmjs → Access Tokens → Granular / Automation for `@clavi` scope (create the org/user first).
2. Store as `NPM_TOKEN`.
3. Preview versions publish with `--tag preview`; stable with `latest`.

### PyPI

1. Create project `clavi-admin` (or let the first upload create it).
2. API token → `PYPI_API_TOKEN` (use `__token__` as username; workflow already does).

### Maven Central (optional)

Enable only when Sonatype Central credentials and a signing key are ready. Set repository variable `PUBLISH_MAVEN=true` and the GPG/Maven secrets listed above. Until then the Maven job is skipped.

### Go

After `vX.Y.Z` is on GitHub:

```bash
go get github.com/BelousovMike/clavi-sdk/go/clavi@vX.Y.Z
```

## Local dry-run

```bash
dotnet pack Clavi.Sdk.sln -c Release -o ./artifacts
# do not push without intending a real release
```
