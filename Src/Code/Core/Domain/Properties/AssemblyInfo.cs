// <copyright file="AssemblyInfo.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Runtime.CompilerServices;

// Story 2.5: the test-data builders (IndTrace.TestData) seed barcodes/cycles into reachable lifecycle
// states by firing the Story 2.3 guarded transition methods, and seed the unreachable initial Created state
// (None->Created has no entity method) plus the arbitrary status pairs the legacy property-bag tests assert
// via the internal `CreateFixture` factories on BarCode/Cycle. Exposing internals here lets the builders use
// those factories WITHOUT re-introducing public status setters, so they survive the Story 2.3b `private set`
// restriction. The repo sets GenerateAssemblyInfo=false globally (Src\Directory.Build.props), so the
// InternalsVisibleTo attribute is declared explicitly in source rather than via an MSBuild item.
[assembly: InternalsVisibleTo("IndTrace.TestData")]

// Audit finding D3: the five trusted lifecycle "apply" seams (BarCode.ApplyFlowStatus / ApplyPartStatus /
// ApplyFlowAndPartStatus and Cycle.ApplyCycleStatus / ApplyCycleAndPartStatus) are now `internal`, not public,
// so presentation and other outer layers can no longer bypass the domain guards by calling a renamed setter.
// The Application layer is the legitimate orchestrator of those seams (cycle-update strategies, the
// Reject/Restore/MarkInvalid/MarkScrap/CancelCycle handlers, the create/update projections, and the
// view/DTO `ToEntity` materializers), so it — and ONLY it among the outer layers — is granted internals access.
// Presentation assemblies (IndTrace.Components, IndTrace.Monitor) are deliberately NOT granted access.
[assembly: InternalsVisibleTo("IndTrace.Application")]

// Story 2.3: Recipe's invariant-bearing config fields (CycleTimeMinimum/Maximum, MaxCyclesOk/MaxCyclesNOk,
// Retry) are now `private set`, with the public guarded `Recipe.Create` factory and the internal unguarded
// `Recipe.CreateFixture` test-data seam (mirroring Cycle/BarCode). One Domain unit-test suite,
// CycleTimeValidatorTests, deliberately exercises the validator against INVALID recipe windows (inverted and
// negative bounds) that `Recipe.Create` legitimately rejects; it can only reconstruct those legacy bounds
// through the unguarded `CreateFixture` seam, so this single Domain test assembly is granted internals access.
// Story 2.5 extends this same grant to KpiOee: KpiOeeTests likewise drives the unguarded `KpiOee.CreateFixture`
// seam to hold arbitrary/extreme metric values (outside the ranges `KpiOee.Create` legitimately rejects),
// which can only be reconstructed through that internal factory.
// Story 2.1 (#26) extends this same grant to Product: ProductCreateTests drives the unguarded
// `Product.CreateFixture` seam to seed arbitrary/legacy product values (and to assert that the seam itself
// bypasses the `Product.Create` non-null identity guard), which can only be reconstructed through that
// internal factory.
// Story 2.2 (#26) extends this same grant to Variable: VariableCreateTests drives the unguarded
// `Variable.CreateFixture` seam to seed arbitrary/legacy variable values (including a seeded VariableId and
// the Validated flag) and to assert that the seam itself bypasses the `Variable.Create` non-null identity
// guard, which can only be reconstructed through that internal factory.
// Story 2.4 (#26) extends this same grant to Plc and MachinePlc: PlcCreateTests and MachinePlcCreateTests
// drive the unguarded `Plc.CreateFixture` / `MachinePlc.CreateFixture` seams to seed arbitrary/legacy values
// (including a seeded PlcId) and to assert that those seams bypass the `Plc.Create` non-null identity guard,
// which can only be reconstructed through those internal factories.
// #39 (Register write-once) rides this SAME existing grant for the two original grantees: the test-data builders
// (IndTrace.TestData) and RegisterTests (IndTrace.Domain.UnitTests) seed registers — including legacy/arbitrary
// values that the guarded `Register.Create` would reject — through the unguarded internal `Register.CreateFixture`
// seam.
// For Recipe/KpiOee/Product/Variable/Plc this remained the ONLY domain unit-test grantee, because those entities
// kept PUBLIC property setters and so their fixtures could stay as `new T { … }` object initializers in any test
// assembly. #39 is different: Register was LOCKED DOWN to factory-only (private setters + private constructor) to
// enforce the write-once / append-only audit invariant at the type level. That removed the public object-initializer
// path everywhere, so every test assembly that seeds a Register must now go through the `CreateFixture` seam. The
// grant is therefore extended below to exactly those Register-constructing test assemblies. This is scoped to the
// test-data seam only; it does not widen access to any lifecycle "apply" seams.
[assembly: InternalsVisibleTo("IndTrace.Domain.UnitTests")]

// #39: Register lockdown — test assemblies that seed Register fixtures via the internal `Register.CreateFixture`
// seam (their former `new Register { … }` object initializers no longer compile against the private setters).
[assembly: InternalsVisibleTo("Application.UnitTests")]
[assembly: InternalsVisibleTo("IndTrace.Aggregation.BoundedTests")]
[assembly: InternalsVisibleTo("IndTrace.Agregation.Dependices")]
[assembly: InternalsVisibleTo("IndTrace.Filters.Tests")]
[assembly: InternalsVisibleTo("IndTrace.Oee.Tests")]
[assembly: InternalsVisibleTo("GateWay.Tests")]

// Story 27.2b-1 (#27 / F4): BarCodeLabel.FromPersisted is an internal, converter-only, TOTAL read-path factory
// (never throws / never normalizes) that trusts the DB-enforced NOT NULL nvarchar(80) column. The persistence
// layer's EF value converter is its intended runtime consumer, so IndTrace.Persistence is granted internals
// access — this mirrors the existing InternalsVisibleTo seam pattern (e.g. BarCode.CreateFixture -> IndTrace.TestData).
// Create(string?) remains the SOLE public railway boundary for NEW labels; FromPersisted stays hidden from
// application/presentation code, which must keep going through Create.
[assembly: InternalsVisibleTo("IndTrace.Persistence")]

// Story 27.2b-1: the real-SQL proof harness (Integration.Tests) exercises FromPersisted directly against a
// persisted-then-read raw string (including a legacy Label='' row) to prove the read path is total and
// byte-preserving on the live \SQL2025 instance. This grant also keeps the analyzer satisfied that the internal
// factory is consumed by an assembly-visible caller.
[assembly: InternalsVisibleTo("Integration.Tests")]
