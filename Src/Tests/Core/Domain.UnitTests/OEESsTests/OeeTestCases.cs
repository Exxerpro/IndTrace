// <copyright file="OeeTestCases.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.OEESsTests;

public static class OeeTestCases
{
    /// <summary>
    /// Represents the AllOeeTestCases.
    /// </summary>
    public class AllOeeTestCases : IEnumerable<object[]>
    {
        /// <summary>
        /// Executes GetEnumerator operation.
        /// </summary>
        /// <returns>The result of GetEnumerator.</returns>
        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (var item in OeeTestCasesNormalCases.GetWarningCases())
                yield return item;
            foreach (var item in OeeTestCasesEdgeCases.GetEdgeCasesWithErrors())
                yield return item;
            // Add more as needed
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}