// <copyright file="TestCaseOrderer.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Xunit.Sdk;
using Xunit.v3;

namespace IndTrace.Domain.UnitTests.CommonTests;
/// <summary>
/// Represents the TestCaseOrderer.
/// </summary>

public class TestCaseOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
    {
        var sortedMethods = testCases
            .Select(tc => new
            {
                TestCase = tc,
                Priority = tc.TestMethod?.TestClass
                    ?.GetType() // Use GetType() to access the runtime type of the test class
                    ?.GetCustomAttributes(typeof(TestPriorityAttribute), inherit: true) // Use reflection to get custom attributes
                    ?.OfType<TestPriorityAttribute>() // Filter for TestPriorityAttribute
                    ?.FirstOrDefault()?.Priority ?? 0 // Get the Priority value or default to 0
            })
            .OrderBy(x => x.Priority)
            .Select(x => x.TestCase);

        return sortedMethods;
    }

    public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases) where TTestCase : notnull, ITestCase
    {
        var sortedMethods = testCases
            .Select(tc => new
            {
                TestCase = tc,
                Priority = tc.TestMethod?.TestClass
                    ?.GetType() // Use GetType() to access the runtime type of the test class
                    ?.GetCustomAttributes(typeof(TestPriorityAttribute), inherit: true) // Use reflection to get custom attributes
                    ?.OfType<TestPriorityAttribute>() // Filter for TestPriorityAttribute
                    ?.FirstOrDefault()?.Priority ?? 0 // Get the Priority value or default to 0
            })
            .OrderBy(x => x.Priority)
            .Select(x => x.TestCase)
            .ToList();

        return sortedMethods;
    }
}