// <copyright file="GlobalUsings.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

global using System.Collections;
global using System.Diagnostics;
global using System.Reactive.Linq;
global using System.Reactive.Subjects;
global using System.Text;
global using Xunit;
global using NSubstitute;
global using Shouldly;
global using IndTrace.Domain;
global using IndTrace.Domain.Models;
global using IndTrace.Domain.Entities;
global using IndTrace.Domain.Entities.BarCodes;
global using IndTrace.Domain.Enum;
global using IndQuestEnums;
global using IndTrace.Domain.Interfaces;
global using IndTrace.Domain.ValueObjects;
global using IndTrace.Application;
global using IndTrace.Application.Models.Helpers;
global using IndTrace.Domain.Enum.LookUpTable;
global using IndTrace.Domain.Enum.Attributes;
// IndQuestResults global usings
global using IndQuestResults;
global using IndQuestResults.Collections;
global using IndQuestResults.Operations;
global using IndQuestResults.Performance;
global using IndQuestResults.Reactive;
global using IndQuestResults.Async;
global using IndQuestResults.Functional;
global using IndQuestResults.Validation;