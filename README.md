# IndTrace

IndTrace is an industrial manufacturing **traceability** system: it tracks parts through manufacturing
cycles across production lines (PLC ↔ SQL Server ↔ Blazor).

IndTrace **tracks; it does not control.** The machine, PLC and operator decide what physically happens.
IndTrace records what was done and validates whether it was legal (for example, whether a part's arrival at a
station is a legal successor in its routing). It never commands equipment.

## What is in this repository

This is the **community edition**, licensed under the GNU AGPL v3.0 or later.

| Area | Projects |
|---|---|
| Core | `IndTrace.Domain`, `IndTrace.Application` (CQRS handlers, `Result<T>` railway) |
| Persistence & hosting | `IndTrace.Persistence` (EF Core / SQL Server), `IndTrace.Dependencies`, `IndTrace.Identity` |
| Gateway | `IndTrace.Gateway` + `IndTrace.Communications` (host), `IndTrace.Hub.Server` (SignalR) |
| UI | `IndTrace.Monitor` (Blazor), `IndTrace.Components`, `IndTrace.UI.Models` |
| Devices | `IndTrace.Devices` — vendor-neutral PLC controller and barcode-reader contracts |

The community edition ships an **in-memory simulated PLC controller** and **no barcode-reader driver**.
Hardware drivers (e.g. Siemens S7, Cognex DataMan) are not part of this repository; they plug in through
`IPlcControllerFactory` and `IBarCodeReader` in `IndTrace.Devices`.

## Build and test

Requires the .NET 10 SDK.

```bash
dotnet build Src/IndTrace.sln
dotnet run --project Src/Tests/Core/Aggregation.BoundedTests/Application.AgregationTests.csproj
```

Warnings are errors. Test projects use Microsoft Testing Platform with xUnit v3; run them with
`dotnet run --project <test project>`.

## Run

The gateway refuses real PLCs without a driver. To run it against the simulated controller, enable simulation
mode on the gateway host:

```bash
export GatewaySimulationOptions__EnableSimulation=true
dotnet run --project Src/Code/Infrastructure/IndTrace.Communications/IndTrace.Communications.csproj
dotnet run --project Src/Code/Infrastructure/IndTrace.Hub/IndTrace.Hub.Server.csproj
dotnet run --project Src/Code/Presentation/IndTrace.Monitor/IndTrace.Monitor.csproj
```

Connection strings live in each host's `appsettings*.json` and can be overridden with environment variables
(`ConnectionStrings__IndTraceDbContext`, `ConnectionStrings__IndTraceDbIdentity`).

## License

Copyright (c) Exxerpro Solutions SA de CV.
Licensed under the [GNU Affero General Public License v3.0 or later](LICENSE).
