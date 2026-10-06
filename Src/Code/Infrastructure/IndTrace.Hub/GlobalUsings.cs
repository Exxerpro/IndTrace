// <copyright file="GlobalUsings.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

global using System;
global using System.Collections.Generic;
global using System.Threading;
global using System.Threading.Tasks;
global using IndTrace.Dependencies.Startup;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
// Server project should not reference SignalR client namespaces globally
global using Microsoft.Extensions.Logging;
// IndQuestResults global usings
global using IndQuestResults;
global using IndQuestResults.Collections;
global using IndQuestResults.Operations;
global using IndQuestResults.Performance;
global using IndQuestResults.Reactive;
global using IndQuestResults.Async;
global using IndQuestResults.Functional;
global using IndQuestResults.Validation;