# NuGet Packaging and Publishing (Cross-Platform)

This repository supports cross-platform NuGet packaging and publishing with `dotnet` CLI scripts.

## Local packaging

```bash
./scripts/nuget-pack.sh
```

Output packages are written to `artifacts/`.

Optional flags:

```bash
./scripts/nuget-pack.sh --dry-run
./scripts/nuget-pack.sh --configuration Debug
./scripts/nuget-pack.sh --output my-packages
```

`nuget-pack.sh` discovers SDK-style `src/**/*.csproj` projects that have `<IsPackable>true</IsPackable>` in their project file.

## Local publishing

Publish to NuGet.org:

```bash
export NUGET_API_KEY="<token>"
./scripts/nuget-push.sh --source nuget --api-key-env NUGET_API_KEY
```

Publish to MyGet:

```bash
export MYGET_API_KEY="<token>"
./scripts/nuget-push.sh --source myget --api-key-env MYGET_API_KEY
```

Dry run:

```bash
./scripts/nuget-push.sh --source nuget --api-key-env NUGET_API_KEY --dry-run
```

## GitHub Actions (manual)

Workflow: `.github/workflows/nuget-publish.yml`

Required repository secrets:

- `NUGET_API_KEY` for NuGet.org publishing
- `MYGET_API_KEY` for MyGet publishing

Run the **NuGet Publish** workflow manually from the Actions tab and select:

- whether to publish to NuGet.org
- whether to publish to MyGet
