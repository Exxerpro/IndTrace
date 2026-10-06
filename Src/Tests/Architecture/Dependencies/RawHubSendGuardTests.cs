// <copyright file="RawHubSendGuardTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Shouldly;

namespace Architecture.Tests.Dependencies;

/// <summary>
/// Enforces that the hub worker hosts never call raw <c>SendAsync</c> again (issue #106).
/// <para>
/// A raw <c>IHubConnection.SendAsync</c> inside a <c>BackgroundService</c> loop throws on a
/// disconnected connection, and because neither hub host overrides
/// <c>HostOptions.BackgroundServiceExceptionBehavior</c>, any exception escaping
/// <c>ExecuteAsync</c> stops the whole process (.NET default <c>StopHost</c>). The sanctioned
/// seam is the <c>TrySendAsync</c> extension, which checks connection state first, reports
/// delivery as a boolean, and never lets a transient send failure kill the host.
/// </para>
/// </summary>
public class RawHubSendGuardTests(ITestOutputHelper output)
{
    /// <summary>
    /// Matches any raw <c>.SendAsync(</c> invocation. <c>TrySendAsync</c> can never match because
    /// the required leading dot is immediately followed by <c>SendAsync</c>.
    /// </summary>
    private static readonly Regex RawSendAsyncRegex = new(
        "\\.SendAsync\\s*\\(",
        RegexOptions.Compiled);

    /// <summary>
    /// The worker host files whose sends must go through <c>TrySendAsync</c>.
    /// </summary>
    private static readonly string[] GuardedWorkerFiles =
    [
        Path.Combine("Code", "Infrastructure", "IndTrace.Hub", "WorkerHubServer.cs"),
        Path.Combine("Code", "Infrastructure", "IndTrace.HubClient", "WorkerHubClient.cs"),
    ];

    /// <summary>
    /// Neither hub worker host may invoke raw <c>SendAsync</c>; every worker send must go through
    /// the state-checking, non-throwing <c>TrySendAsync</c> extension.
    /// </summary>
    [Fact]
    public void HubWorkerHosts_MustNot_Call_Raw_SendAsync()
    {
        var sourceRoot = FindRepositorySourceRoot();
        var violations = new List<string>();

        foreach (var relativePath in GuardedWorkerFiles)
        {
            var file = Path.Combine(sourceRoot, relativePath);
            File.Exists(file).ShouldBeTrue($"Guarded worker file not found: {relativePath}");

            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                // Comment lines may legitimately DOCUMENT the banned pattern; the guard bans
                // executable sends only.
                if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                if (RawSendAsyncRegex.IsMatch(lines[i]))
                {
                    violations.Add($"{relativePath.Replace('\\', '/')}:{i + 1}  {lines[i].Trim()}");
                }
            }
        }

        foreach (var violation in violations)
        {
            output.WriteLine(violation);
        }

        violations.ShouldBeEmpty(
            "Raw SendAsync call found in a hub worker host. A raw send throws on a disconnected " +
            "connection and kills the whole host process (issue #106). Use the TrySendAsync " +
            "extension from IndTrace.HubConnection.Extensions instead — it checks the connection " +
            "state, reports delivery as a boolean, and never lets a transient failure stop the host.");
    }

    /// <summary>
    /// Walks up from the test assembly location to the repository "Src" root,
    /// identified by the presence of Directory.Packages.props.
    /// </summary>
    private static string FindRepositorySourceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Directory.Packages.props")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the repository source root (Directory.Packages.props) above " + AppContext.BaseDirectory);
    }
}
