// <copyright file="MutableAggregateCacheExclusionRatchetTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using IndTrace.Persistence.Caching;
using Shouldly;

namespace Architecture.Tests.Layers;

/// <summary>
/// #211 cache-exclusion ratchet: every aggregate ROOT and every write-enforced aggregate MEMBER in the #95
/// ratchet map (<see cref="AggregateWriteEnforcementTests.WriteEnforcedRootMembers"/>) must be classified as
/// NON-cacheable by <see cref="CacheableTypePolicy"/>. The FusionCache memory level returns entries by
/// reference, so caching a mutable aggregate type aliases one shared CLR instance across concurrent
/// executions — the #211 hazard. Roots are covered structurally by the <c>IAggregateRoot</c> marker; members
/// have no marker and are listed explicitly in the policy, so THIS test is what turns "a new member joined
/// the #95 write ratchet but nobody excluded it from the cache" into a build-red event.
/// </summary>
public class MutableAggregateCacheExclusionRatchetTests
{
    /// <summary>
    /// Every root key and every member value of the #95 write-enforcement map must be non-cacheable under
    /// <see cref="CacheableTypePolicy.IsCacheable"/>. Extending the #95 map without extending the policy's
    /// member set fails this test.
    /// </summary>
    [Fact]
    public void EveryWriteEnforcedRootAndMember_MustBeExcludedFromCaching()
    {
        var map = AggregateWriteEnforcementTests.WriteEnforcedRootMembers;
        map.ShouldNotBeEmpty();

        var offenders = new List<string>();

        foreach (var (root, members) in map)
        {
            if (CacheableTypePolicy.IsCacheable(root))
            {
                offenders.Add($"{root.Name} (aggregate root)");
            }

            offenders.AddRange(
                members.Where(CacheableTypePolicy.IsCacheable)
                       .Select(member => $"{member.Name} (member of the {root.Name} aggregate)"));
        }

        offenders.ShouldBeEmpty(
            "#211: mutable aggregate types must never be cacheable — the by-reference cache aliases one shared entity instance across concurrent executions; add the type to CacheableTypePolicy.MutableAggregateMembers (roots are covered by the IAggregateRoot marker)");
    }
}
