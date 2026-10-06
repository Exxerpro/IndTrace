// <copyright file="CacheKeyBuilderStaticsCollection.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Xunit;

namespace IndTrace.Agregation.Dependices.Infrastructure.Caching;

/// <summary>
/// Serializes every test that mutates the process-global statics on
/// <c>IndTrace.Persistence.Caching.CacheKeyBuilderReadOnlyRepos</c> (partition provider and key
/// options). Those same statics are also written by every <c>DependenciesFactory</c>-based test
/// through <c>CachePartitionInitializer</c>, so without a non-parallel collection a concurrently
/// initializing fixture clobbers the GUID prefix a cache-key test captured in its constructor,
/// producing rotating flaky failures (GitHub issue #53). A collection marked
/// <see cref="CollectionDefinitionAttribute.DisableParallelization"/> does not run in parallel
/// against any other collection, so these tests never overlap a fixture that would overwrite the
/// prefix mid-flight.
/// </summary>
[CollectionDefinition("CacheKeyBuilderStatics", DisableParallelization = true)]
public sealed class CacheKeyBuilderStaticsCollection;
