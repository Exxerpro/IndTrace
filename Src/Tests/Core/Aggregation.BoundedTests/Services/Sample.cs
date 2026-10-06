// <copyright file="Sample.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Xunit.Runner.Common;

namespace IndTrace.Aggregation.BoundedTests.Services
{
    public class Sample
    {
        public Sample(ILogger<Sample> logger)
        {
            logger.LogInformation("Sample class instantiated");
        }

        public async Task<int> Execute()
        {
            await Task.Delay(1);
            return 3;
        }
    }
}