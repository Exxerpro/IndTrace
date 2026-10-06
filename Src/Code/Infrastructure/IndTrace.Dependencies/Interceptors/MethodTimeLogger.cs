// <copyright file="MethodTimeLogger.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Serilog;
using ILogger = Serilog.ILogger;

namespace IndTrace.Dependencies.Interceptors
{
    public static class MethodTimeLogger
    {
        public static ILogger? Logger { get; set; }

        public static void Log(MethodBase methodBase, long milliseconds, string message)
        {
            //Do some logging here

            Logger?.Information(
                "{MethodName} took {Milliseconds} milliseconds. {Message}",
                methodBase.Name,
                milliseconds,
                message);
        }
    }
}