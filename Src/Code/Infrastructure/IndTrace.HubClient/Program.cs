// <copyright file="Program.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Hub.Client;

/// <summary>
/// Represents the Program.
/// </summary>
public class Program
{
    /// <summary>
    /// Executes Main operation.
    /// </summary>
    /// <param name="args">The args.</param>
    public static void Main(string[] args)
    {
        try
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddSingleton<IndTrace.Domain.Interfaces.IDateTimeMachine, IndTrace.Domain.Models.DateTimeMachine>();
            builder.Services.AddHostedService<WorkerHubClient>();

            var host = builder.Build();
            host.Run();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        Console.ReadLine();
    }
}