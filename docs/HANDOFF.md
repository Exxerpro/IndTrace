# Handoff: IndTrace community edition

Status as of 2026-10-07. This is where the open-sourcing effort stands, so the next session can pick it up.

## Current state

- **Repository:** `Exxerpro/IndTrace`, **private** until the owner decides to make it public.
- **License:** GNU AGPL v3.0 or later (GitHub detects `AGPL-3.0`). Copyright holder: Exxerpro Solutions SA de CV.
- **Model: open core.** This repository is the community edition. The proprietary device drivers (Siemens S7 /
  Sharp7 / Snap7 PLC communication, Cognex DataMan barcode readers) and the tooling that depends on them live
  only in the private enterprise repository.
- **History:** fresh. One initial commit, generated from the enterprise repository by its export tool. The
  enterprise history is not and will not be published.
- **Verified at export time:** a clean-clone restore and build, with nuget.org as the only package source and
  an empty package cache, has 0 errors and 0 warnings (warnings are errors). Test results:

  | Suite | Passed / total |
  |---|---|
  | `IndTrace.Devices.Tests` | 26/26 |
  | Architecture | 77/77 |
  | Domain | 2,733/2,733 |
  | Application | 7,031/7,032 (1 skipped) |
  | HubConnection | 136/137 (1 skipped) |
  | Monitor | 49/49 |
  | Identity | 3/3 |
  | Aggregation | 810/838 (28 skipped) |

## How this repository is produced (read this before editing anything)

**Source code here is GENERATED.** Code changes are made in the enterprise repository and re-exported. A
direct edit to `Src/**` in this repository is overwritten by the next sync.

- The enterprise repository holds the export tool (`tools/oss-export/`: `export.py`, `config.toml`,
  `templates/`). It takes only committed files, applies an allowlist, prunes the solution, anonymizes, writes
  AGPL license headers and adds the root templates (README, LICENSE, CONTRIBUTING, NOTICE, `Src/nuget.config`,
  `.gitignore`). It then runs gates: a deny-list scan, gitleaks, an isolated clean-clone build, and tests.
  Any gate failure aborts the export.
- **Owned by this repository** (never written by the export, safe to edit here): `docs/**`, `.github/**`
  (CI, community health files, issue and PR templates) and `site/**` (the GitHub Pages project website,
  English at `/` and Spanish at `/es/`, deployed by `.github/workflows/pages.yml`). Root files such as `README.md`, `NOTICE.md` and
  `CONTRIBUTING.md` are export templates: edit them in the enterprise repository.
- **Sync procedure** (from the enterprise repository, .NET 10 SDK on `PATH`; this clone checked out next to
  it as `../IndTrace-oss`):

  ```bash
  tools/oss-export/sync.sh --commit   # export (all gates must pass) -> mirror -> commit; never pushes
  git -C ../IndTrace-oss push
  ```

  `sync.sh` exports into a separate staging directory (`../IndTrace-oss-export`) and mirrors it into this
  clone, keeping `.git`, `docs/`, `.github/` and `site/`. It refuses a clone that is dirty, is not `Exxerpro/IndTrace`,
  or commits with a non-noreply e-mail. `export.py` refuses to write into a git checkout, because `--force`
  deletes its output directory. Do not re-run `git init` or force-push: later syncs are ordinary commits on
  top of `main`.

## Architecture essentials for contributors

- **IndTrace tracks; it does not control.** It records what the equipment and operator did and validates
  whether it was legal (for example, whether an arrival is a legal routing successor). It never commands a
  machine. Outbound messages to the PLC are acknowledgement, status and validation results only.
- **Device seam:** `Src/Code/Infrastructure/IndTrace.Devices`.
  - `IIndTraceControllerRx` is the PLC controller contract; `IPlcControllerFactory` creates one per PLC; the
    gateway never constructs a vendor controller itself. `IBarCodeReader` is the barcode-reader contract.
  - Community defaults are registered with `AddCommunityDeviceDefaults()` (`TryAdd`, so an installed driver
    always wins):
    - `SimulatedPlcControllerFactory` gives simulation-enabled PLCs an in-memory `SimulatedControllerRx`. That
      controller raises `CommandChanged` on change and round-trips part data. A real PLC is refused loudly,
      because no driver is installed.
    - `NullBarCodeReader` never scans; pages fall back to manual barcode entry.
  - `IndTraceEnterprise` (`Src/Directory.Build.targets`) is `true` only when the S7 driver project exists in
    the tree. In this repository it is always `false`, so `*.Enterprise.cs` files and driver references never
    exist here. Keep new code free of vendor types.
- **Running the gateway** needs simulation mode: `GatewaySimulationOptions__EnableSimulation=true`. See
  `README.md`.
- **Conventions:**
  - Railway-oriented `Result<T>` (IndQuestResults), not exceptions across boundaries.
  - CQRS through `IMonitorRequestDispatcher` (UI) and `IMonitorRequestDispatcherGateway` (PLC side); no
    MediatR.
  - `IDateTimeMachine` for time, never `DateTime.Now`.
  - Shouldly and NSubstitute in tests; AutoMapper, FluentAssertions and Moq are banned.
  - No `!` null-forgiving operator and no `#pragma` suppressions.
- **Tests** run on Microsoft Testing Platform with xUnit v3, using `dotnet run --project <test csproj>`.
  `dotnet test` is unreliable for these suites on .NET 10.

## Deliberately NOT in this repository

- PLC and barcode-reader drivers, the virtual-PLC network, the line simulator, OEE and analytics apps, and the
  enterprise-only tests.
- Tests that need a real SQL Server or real devices (integration and gateway hardware suites).
- Production or customer data. Test fixtures were anonymized at export: customer, product, part-number, host,
  person and e-mail identifiers were replaced with neutral values, and the deny-list gate keeps them out.
- Internal planning documents and AI-tooling folders.

## Open items: next steps toward going public

1. **Owner review.** Read `README.md`, `NOTICE.md` and `CONTRIBUTING.md` and adjust the wording.
2. **Brand names in test fixtures.** Fictional fixtures still use well-known company names (e.g. "Ford",
   "Tesla") as illustrative data, about 1,600 mentions; addresses and hostnames were already neutralized.
   Kept on purpose for now, and `NOTICE.md` covers it. If they should go, add substitutions in the enterprise
   export config and re-sync; do not edit them here.
3. **CI.** Add `.github/workflows/ci.yml`: restore, build `Src/IndTrace.sln`, and run the eight suites above
   with `dotnet run --project`. Use .NET 10 on `ubuntu-latest`; no external services are needed.
4. **Repository hygiene before going public:**
   - `SECURITY.md` (private vulnerability reporting);
   - `CODE_OF_CONDUCT.md`;
   - issue and PR templates;
   - branch protection on `main`;
   - repository topics.
5. **Contribution policy.** Decide whether outside contributions need a CLA, or whether the current
   "inbound = outbound AGPL" wording in `CONTRIBUTING.md` is enough. This matters for the open-core model,
   because the enterprise edition is not AGPL.
6. **Repository size (about 97 MB).** Mostly duplicated UI images: the same `wwwroot/img` assets are copied
   across `IndTrace.Monitor`, `IndTrace.Identity` and `IndTrace.Components`. Consider de-duplicating them into
   one static-assets project in the enterprise repository, then re-sync.
7. **Getting-started docs.** Add a database bootstrap guide (EF migrations or schema scripts for an empty SQL
   Server) and a "simulate a part through a line" walkthrough using the simulated controller.
8. **Going public.** Do it only after items 1–5. Re-run the export immediately before switching visibility, so
   the gates have run on exactly what becomes public.

## Known caveats

- `Src/Directory.Packages.props` still lists a few central package versions that no exported project
  references; they are harmless and can be pruned in the enterprise repository.
- The community edition has no barcode-reader driver, so the final-report page works only with manual entry.
