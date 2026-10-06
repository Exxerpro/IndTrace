// <copyright file="SpecificationCacheKeyVisitorTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Application.Repository;
using IndTrace.Domain.Entities;
using Shouldly;
using Xunit;

namespace Application.UnitTests.Repository;

/// <summary>
/// Regression tests for #61 (P0-1): the cache-key expression visitor must render two-hop (and deeper) closure
/// values to their RUNTIME VALUE. The previous implementation only resolved a single-hop closure and fell back
/// to the bare member NAME for a two-hop closure such as <c>context.Request.PartNumber</c>, collapsing every
/// product/rule/machine on the barcode-create path onto ONE cache key.
///
/// The visitor is internal; its behaviour is exercised through the public <see cref="Specification{T}.Key"/>.
/// </summary>
public class SpecificationCacheKeyVisitorTests
{
    private sealed record Request(string PartNumber);

    private sealed record RequestContext(Request Request);

    [Fact]
    public void Key_TwoHopClosure_DifferentValues_ProduceDifferentKeys()
    {
        // Arrange — a genuine two-hop closure: the lambda captures `context` and reads `context.Request.PartNumber`.
        var contextA = new RequestContext(new Request("A"));
        var contextB = new RequestContext(new Request("B"));

        var specA = new Specification<Product>(p => p.PartNumber == contextA.Request.PartNumber);
        var specB = new Specification<Product>(p => p.PartNumber == contextB.Request.PartNumber);

        // Act
        var keyA = specA.Key;
        var keyB = specB.Key;

        // Assert — distinct closure values must yield distinct keys (the bug produced identical keys).
        keyA.ShouldNotBe(keyB, "two-hop closures with different values must not collapse to one cache key");

        // The key must embed the resolved runtime VALUE, not the member name.
        keyA.ShouldContain("\"A\"");
        keyB.ShouldContain("\"B\"");
    }

    [Fact]
    public void Key_TwoHopClosure_SameValue_ProducesStableKey()
    {
        // Arrange
        var context = new RequestContext(new Request("SAME"));
        var spec1 = new Specification<Product>(p => p.PartNumber == context.Request.PartNumber);
        var spec2 = new Specification<Product>(p => p.PartNumber == context.Request.PartNumber);

        // Act & Assert — equal closure values remain cache-key stable (no accidental over-partitioning).
        spec1.Key.ShouldBe(spec2.Key);
    }

    [Fact]
    public void Key_ParameterPath_RendersColumn_NotClosureValue()
    {
        // Arrange — the left side is a queried column and must render as the parameter path, not a value.
        var context = new RequestContext(new Request("A"));
        var spec = new Specification<Product>(p => p.PartNumber == context.Request.PartNumber);

        // Act
        var key = spec.Key;

        // Assert — the queried column must render as the parameter-rooted path.
        key.ShouldContain("p.PartNumber");
    }

    [Fact]
    public void Key_UnresolvableMemberChain_FailsLoud()
    {
        // Arrange — a member (`.Length`) accessed on a method-call result that still captures the parameter.
        // This is neither a fully closure-bound value nor a clean parameter path, so a stable key cannot be
        // produced. A poisoned key on a traceability wire is never acceptable: the visitor must fail loud.
        var spec = new Specification<Product>(p => p.PartNumber.Trim().Length == 1);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => spec.Key);
    }

    // ---------------------------------------------------------------------------------------------------------
    // Follow-up (#61): the visitor previously COLLAPSED collection-valued closures. For the pervasive predicate
    // shape `someList.Contains(b.Column)`, the closure list was resolved to its runtime List<T> and then rendered
    // by object.ToString() → the content-INDEPENDENT type name, and the `Contains` method name was never emitted.
    // Two specs with different lists ([1,2] vs [3,4]) produced the SAME key → the second caller got the first's
    // cached rows. These tests pin the collection-contents + method-name rendering.
    // ---------------------------------------------------------------------------------------------------------

    [Fact]
    public void Key_CollectionContains_DifferentContents_ProduceDifferentKeys()
    {
        // Arrange — the GetBarCodeReportQuery shape: a captured list is tested against a queried column.
        var listA = new List<int> { 1, 2 };
        var listB = new List<int> { 3, 4 };

        var specA = new Specification<Product>(p => listA.Contains(p.RuleId));
        var specB = new Specification<Product>(p => listB.Contains(p.RuleId));

        // Act
        var keyA = specA.Key;
        var keyB = specB.Key;

        // Assert — distinct list contents must yield distinct keys (the bug produced identical keys).
        keyA.ShouldNotBe(keyB, "collection closures with different contents must not collapse to one cache key");

        // The key must embed the resolved element VALUES and the method name, not the collection's type name.
        keyA.ShouldContain("[1,2]");
        keyB.ShouldContain("[3,4]");
        keyA.ShouldContain("Contains(");
        keyA.ShouldContain("p.RuleId");
    }

    [Fact]
    public void Key_CollectionContains_SameContents_ProducesStableKey()
    {
        // Arrange — two independently-built lists with equal contents must remain cache-key stable.
        var list1 = new List<int> { 7, 8, 9 };
        var list2 = new List<int> { 7, 8, 9 };

        var spec1 = new Specification<Product>(p => list1.Contains(p.RuleId));
        var spec2 = new Specification<Product>(p => list2.Contains(p.RuleId));

        // Act & Assert — equal contents remain stable (no accidental over-partitioning on list identity).
        spec1.Key.ShouldBe(spec2.Key);
    }

    [Fact]
    public void Key_CollectionContains_ElementOrder_IsDistinguished()
    {
        // Arrange — a Contains predicate against differently-ordered lists targets a different logical row set
        // only insofar as ordering matters to the query; regardless, the visitor renders order faithfully so it
        // never UNDER-partitions. Distinct orderings must not silently share one key.
        var ascending = new List<int> { 1, 2, 3 };
        var descending = new List<int> { 3, 2, 1 };

        var specAsc = new Specification<Product>(p => ascending.Contains(p.RuleId));
        var specDesc = new Specification<Product>(p => descending.Contains(p.RuleId));

        // Act & Assert
        specAsc.Key.ShouldNotBe(specDesc.Key);
    }

    [Fact]
    public void Key_MixedContainsAndScalarClosure_DistinguishesOnBoth()
    {
        // Arrange — a mixed spec: a collection Contains AND a scalar closure equality. Changing EITHER the list
        // contents OR the scalar closure value must change the key.
        var list = new List<int> { 1, 2 };
        var customerX = 10;
        var customerY = 20;

        var baseline = new Specification<Product>(p => list.Contains(p.RuleId) && p.CustomerId == customerX);

        // Differs only in the scalar closure value.
        var scalarChanged = new Specification<Product>(p => list.Contains(p.RuleId) && p.CustomerId == customerY);

        // Differs only in the collection contents.
        var otherList = new List<int> { 3, 4 };
        var collectionChanged = new Specification<Product>(p => otherList.Contains(p.RuleId) && p.CustomerId == customerX);

        // Act
        var baseKey = baseline.Key;

        // Assert — the key discriminates on the scalar dimension AND the collection dimension independently.
        baseKey.ShouldNotBe(scalarChanged.Key, "changing the scalar closure value must change the key");
        baseKey.ShouldNotBe(collectionChanged.Key, "changing the collection contents must change the key");

        // Both dimensions are visible in the key.
        baseKey.ShouldContain("[1,2]");
        baseKey.ShouldContain("p.CustomerId");
        baseKey.ShouldContain("10");
    }

    [Fact]
    public void Key_MixedContainsAndScalarClosure_SameInputs_ProducesStableKey()
    {
        // Arrange — identical mixed specs remain cache-key stable.
        var list1 = new List<int> { 5, 6 };
        var list2 = new List<int> { 5, 6 };
        var customer = 42;

        var spec1 = new Specification<Product>(p => list1.Contains(p.RuleId) && p.CustomerId == customer);
        var spec2 = new Specification<Product>(p => list2.Contains(p.RuleId) && p.CustomerId == customer);

        // Act & Assert
        spec1.Key.ShouldBe(spec2.Key);
    }
}
