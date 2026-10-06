// <copyright file="MediatRRequestReturnTypeTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using IndTrace.Domain.Models;
using IndTrace.Application.Machines.Commands.Update;
using Shouldly;
using System.Reflection;

namespace Architecture.Tests.Layers;
/// <summary>
/// Validates handler conventions for the in-house dispatchers (Gateway/Monitor).
/// Ensures all handlers expose a ProcessAsync method returning Task<Result{T}>.
/// </summary>
public class DispatcherHandlerReturnTypeTests(ITestOutputHelper output)
{
    /// <summary>
    /// All handlers should return Task<Result{T}> from ProcessAsync.
    /// </summary>
    [Fact]
    public void AllHandlers_ProcessAsync_ShouldReturn_TaskOfResultT()
    {
        // Anchor on a known handler type in Application layer
        var assembly = Assembly.GetAssembly(typeof(MachineUpdateCommandHandler))!;
        var handlerTypes = assembly
            .GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Handler"))
            .ToList();

        var violatingHandlers = new List<string>();

        foreach (var handler in handlerTypes)
        {
            var method = handler.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "ProcessAsync");
            if (method == null)
            {
                // no ProcessAsync; allow for non-dispatcher types
                continue;
            }

            var returnType = method.ReturnType;
            var isTask = returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>);
            if (!isTask)
            {
                violatingHandlers.Add($"{handler.FullName}::ProcessAsync must return Task<Result<T>>");
                continue;
            }

            var inner = returnType.GetGenericArguments()[0];
            var isResult = inner.IsGenericType && inner.Name.StartsWith("Result`");
            if (!isResult)
            {
                violatingHandlers.Add($"{handler.FullName}::ProcessAsync must return Task<Result<T>>");
            }
        }

        foreach (var v in violatingHandlers)
        {
            output.WriteLine(v);
        }

        violatingHandlers.ShouldBeEmpty("All handlers should expose ProcessAsync returning Task<Result<T>>");
    }
}