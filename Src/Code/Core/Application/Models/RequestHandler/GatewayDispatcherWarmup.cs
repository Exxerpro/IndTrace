// <copyright file="GatewayDispatcherWarmup.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Models.RequestHandler;

using IndTrace.Application.Cycles.Commands.Create;
using IndTrace.Application.Models.Behaviors;

public static class GatewayDispatcherWarmup
{
    public static void PreloadDispatcherCache()
    {
        // Preload known handler and behavior method metadata into the dispatcher caches
        var handlerTypes = new[]
        {
            typeof(IGatewayRequestHandler<CreateBarCodeCommand, TaskGatewayResponseDto>),
            typeof(IGatewayRequestHandler<ReadBarCodeQuery, TaskGatewayResponseDto>),
            typeof(IGatewayRequestHandler<CreateCyclesCommand, TaskGatewayResponseDto>),
            typeof(IGatewayRequestHandler<UpdateCyclesOkCommand, TaskGatewayResponseDto>),
            typeof(IGatewayRequestHandler<UpdateCyclesNotOkCommand, TaskGatewayResponseDto>),
            typeof(IGatewayRequestHandler<UpdateBarCodeCommand, TaskGatewayResponseDto>),
        };

        foreach (var type in handlerTypes)
        {
            _ = type.GetMethod("ProcessAsync");
        }

        // Also preload behaviors if needed
        var behaviorTypes = new[]
        {
            typeof(LoggingBehavior<,>),
            typeof(GatewayPersistenceBehavior<,>),
            typeof(ValidationBehavior<,>),
            typeof(EventLoggerBehavior<,>),
            typeof(RequestPerformanceBehaviour<,>),
            typeof(UnhandledExceptionBehaviour<,>),
        };

        foreach (var openGeneric in behaviorTypes)
        {
            var closed = openGeneric.MakeGenericType(typeof(CreateBarCodeCommand), typeof(TaskGatewayResponseDto));
            _ = closed.GetMethod("HandleAsync");
        }
    }
}