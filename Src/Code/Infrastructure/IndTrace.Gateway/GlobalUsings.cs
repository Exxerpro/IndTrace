// <copyright file="GlobalUsings.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Text.Json;
global using Gateway.Extensions;
global using Gateway.Helpers;
global using IndTrace.Application.BarCodes.Queries.GetBarCodeGatewayDetail;
global using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
global using IndTrace.Application.Configuration.Services;
global using IndTrace.Application.Plcs.Queries.GetDetail;
global using IndTrace.HubConnection.Abstractions;
global using IndTrace.HubConnection.Extensions;
global using IndTrace.Domain.Entities;
global using IndTrace.Domain.Enum;
global using IndQuestEnums;
global using IndTrace.Domain.Interfaces;
global using IndTrace.Domain.Models;
global using IndTrace.Devices.Plc;
global using Microsoft.AspNetCore.SignalR.Client;
global using static Gateway.Gateway.GatewayConstants;
global using Result = IndQuestResults.Result;

// IndQuestResults global usings
global using IndQuestResults;
global using IndQuestResults.Collections;
global using IndQuestResults.Operations;
global using IndQuestResults.Performance;
global using IndQuestResults.Reactive;
global using IndQuestResults.Async;
global using IndQuestResults.Functional;
global using IndQuestResults.Validation;