// <copyright file="GlobalUsings.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

global using Gateway.Gateway;
global using IndTrace.Application.BarCodes.Commands.Create;
global using IndTrace.Application.BarCodes.Commands.Update;
global using IndTrace.Application.BarCodes.Queries.GetBarCodeGatewayDetail;
global using IndTrace.Application.BarCodes.Services;
global using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
global using IndTrace.Application.Configuration.Services;
global using IndTrace.Application.Cycles.Commands.Create;
global using IndTrace.Application.Cycles.Commands.UpdateCyclesNok;
global using IndTrace.Application.Cycles.Commands.UpdateCyclesOk;
global using IndTrace.Application.Models.Behaviors;
global using IndTrace.Application.Models.CacheServices;
global using IndTrace.Application.Models.Interfaces;
global using IndTrace.Application.Models.Notifications;
global using IndTrace.Application.Models.RequestHandler;
global using IndTrace.Application.Models.Services;
global using IndTrace.Application.Shifts.Services;
global using IndTrace.Application.UserService;
global using IndTrace.HubConnection.Abstractions;
global using IndTrace.HubConnection.Extensions;
global using IndTrace.Dependencies.Interceptors;
global using IndTrace.Dependencies.Startup;
global using IndTrace.Dependencies.Utilities;
global using IndTrace.Domain.Entities;
global using IndTrace.Domain.Entities.BarCodes;
global using IndTrace.Domain.Interfaces;
global using IndTrace.Domain.Models;
global using IndTrace.Persistence.DBContext;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Options;
// IndQuestResults global usings
global using IndQuestResults;
global using IndQuestResults.Collections;
global using IndQuestResults.Operations;
global using IndQuestResults.Performance;
global using IndQuestResults.Reactive;
global using IndQuestResults.Async;
global using IndQuestResults.Functional;
global using IndQuestResults.Validation;