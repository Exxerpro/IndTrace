// <copyright file="GlobalUsings.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

// Global using directives

global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.InteropServices;
global using System.Text;
global using System.Threading.Tasks;
global using Castle.DynamicProxy;
global using DotNetEd.CoreAdmin;
global using IndTrace.Application.BarCodes.Queries.GetBarCodeDetail;
global using IndTrace.Application.BarCodes.Queries.GetBarCodeList;
global using IndTrace.Application.ConfigStations.Queries.GetConfigStationList;
global using IndTrace.Application.Configuration.Services;
global using IndTrace.Application.Machines.Commands.Create;
global using IndTrace.Application.Models.Behaviors;
global using IndTrace.Application.Models.CacheServices;
global using IndTrace.Application.Models.Interfaces;
global using IndTrace.Application.Models.Services;
global using IndTrace.Application.Notifications;
global using IndTrace.Application.Repository;
global using IndTrace.Domain.Entities;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.SignalR.Client;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using OpenTelemetry;
global using OpenTelemetry.Metrics;
global using OpenTelemetry.Trace;
global using Polly;
global using Polly.Extensions.Http;
global using static IndTrace.HubConnection.Contracts.HubMethods;
// IndQuestResults global usings
global using IndQuestResults;
global using IndQuestResults.Collections;
global using IndQuestResults.Operations;
global using IndQuestResults.Performance;
global using IndQuestResults.Reactive;
global using IndQuestResults.Async;
global using IndQuestResults.Functional;
global using IndQuestResults.Validation;