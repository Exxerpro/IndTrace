// <copyright file="CyclesRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;
using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for Cycle entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// IMPORTED: Contains all 370 entities from Cycles.json
/// Generated on: 2025-09-03 06:01:53
/// </summary>
internal static class CyclesRawData
{
    private static readonly ImmutableDictionary<int, Cycle> _cyclesDict =
        new Dictionary<int, Cycle>
        {
            [1] = CycleRow(1, 1, 100, CycleStatus.Started, PartStatus.Ok, 1, 82, 139, new DateTime(2023, 08, 27, 00, 49, 12), new DateTime(2023, 08, 27, 00, 50, 34)),
            [2] = CycleRow(2, 2, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 90, 171, new DateTime(2023, 08, 27, 02, 54, 19), new DateTime(2023, 08, 27, 02, 55, 49)),
            [3] = CycleRow(3, 3, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 77, 176, new DateTime(2023, 08, 27, 08, 18, 00), new DateTime(2023, 08, 27, 08, 19, 17)),
            [4] = CycleRow(4, 4, 100, CycleStatus.Started, PartStatus.Ok, 1, 94, 166, new DateTime(2023, 08, 27, 03, 41, 07), new DateTime(2023, 08, 27, 03, 42, 41)),
            [5] = CycleRow(5, 5, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 154, new DateTime(2023, 08, 27, 09, 27, 28), new DateTime(2023, 08, 27, 09, 29, 07)),
            [6] = CycleRow(6, 6, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 56, 145, new DateTime(2023, 08, 27, 10, 01, 55), new DateTime(2023, 08, 27, 10, 02, 51)),
            [7] = CycleRow(7, 7, 100, CycleStatus.Started, PartStatus.Ok, 1, 91, 152, new DateTime(2023, 08, 27, 11, 50, 57), new DateTime(2023, 08, 27, 11, 52, 28)),
            [8] = CycleRow(8, 8, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 85, 156, new DateTime(2023, 08, 27, 12, 03, 04), new DateTime(2023, 08, 27, 12, 04, 29)),
            [9] = CycleRow(9, 9, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 78, 162, new DateTime(2023, 08, 27, 12, 20, 08), new DateTime(2023, 08, 27, 12, 21, 26)),
            [10] = CycleRow(10, 10, 100, CycleStatus.Started, PartStatus.Ok, 1, 55, 134, new DateTime(2023, 08, 27, 12, 19, 35), new DateTime(2023, 08, 27, 12, 20, 30)),
            [11] = CycleRow(11, 11, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 64, 153, new DateTime(2023, 08, 27, 09, 22, 03), new DateTime(2023, 08, 27, 09, 23, 07)),
            [12] = CycleRow(12, 12, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 84, 178, new DateTime(2023, 08, 27, 09, 25, 05), new DateTime(2023, 08, 27, 09, 26, 29)),
            [13] = CycleRow(13, 13, 100, CycleStatus.Started, PartStatus.Ok, 1, 87, 139, new DateTime(2023, 08, 27, 12, 09, 01), new DateTime(2023, 08, 27, 12, 10, 28)),
            [14] = CycleRow(14, 14, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 76, 155, new DateTime(2023, 08, 27, 12, 12, 01), new DateTime(2023, 08, 27, 12, 13, 17)),
            [15] = CycleRow(15, 15, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 95, 171, new DateTime(2023, 08, 27, 12, 16, 03), new DateTime(2023, 08, 27, 12, 17, 38)),
            [16] = CycleRow(16, 16, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 73, 149, new DateTime(2023, 08, 27, 12, 19, 31), new DateTime(2023, 08, 27, 12, 20, 44)),
            [17] = CycleRow(17, 16, 300, CycleStatus.Started, PartStatus.Ok, 1, 73, 132, new DateTime(2023, 08, 27, 12, 20, 44), new DateTime(2023, 08, 27, 12, 21, 57)),
            [18] = CycleRow(18, 17, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 86, 175, new DateTime(2023, 08, 27, 12, 20, 56), new DateTime(2023, 08, 27, 12, 22, 22)),
            [19] = CycleRow(19, 17, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 97, 161, new DateTime(2023, 08, 27, 12, 22, 22), new DateTime(2023, 08, 27, 12, 23, 59)),
            [20] = CycleRow(20, 18, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 169, new DateTime(2023, 08, 27, 12, 22, 36), new DateTime(2023, 08, 27, 12, 24, 15)),
            [21] = CycleRow(21, 18, 300, CycleStatus.Started, PartStatus.Ok, 1, 90, 149, new DateTime(2023, 08, 27, 12, 24, 15), new DateTime(2023, 08, 27, 12, 25, 45)),
            [22] = CycleRow(22, 19, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 68, 168, new DateTime(2023, 08, 27, 12, 21, 23), new DateTime(2023, 08, 27, 12, 22, 31)),
            [23] = CycleRow(23, 19, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 90, 165, new DateTime(2023, 08, 27, 12, 22, 31), new DateTime(2023, 08, 27, 12, 24, 01)),
            [24] = CycleRow(24, 20, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 63, 145, new DateTime(2023, 08, 27, 12, 22, 16), new DateTime(2023, 08, 27, 12, 23, 19)),
            [25] = CycleRow(25, 20, 300, CycleStatus.Started, PartStatus.Ok, 1, 88, 174, new DateTime(2023, 08, 27, 12, 23, 19), new DateTime(2023, 08, 27, 12, 24, 47)),
            [26] = CycleRow(26, 21, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 61, 134, new DateTime(2023, 08, 27, 12, 20, 02), new DateTime(2023, 08, 27, 12, 21, 03)),
            [27] = CycleRow(27, 21, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 77, 154, new DateTime(2023, 08, 27, 12, 21, 03), new DateTime(2023, 08, 27, 12, 22, 20)),
            [28] = CycleRow(28, 22, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 96, 148, new DateTime(2023, 08, 27, 12, 21, 58), new DateTime(2023, 08, 27, 12, 23, 34)),
            [29] = CycleRow(29, 22, 300, CycleStatus.Started, PartStatus.Ok, 1, 85, 152, new DateTime(2023, 08, 27, 12, 23, 34), new DateTime(2023, 08, 27, 12, 24, 59)),
            [30] = CycleRow(30, 23, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 66, 166, new DateTime(2023, 08, 27, 13, 15, 45), new DateTime(2023, 08, 27, 13, 16, 51)),
            [31] = CycleRow(31, 23, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 76, 167, new DateTime(2023, 08, 27, 13, 16, 51), new DateTime(2023, 08, 27, 13, 18, 07)),
            [32] = CycleRow(32, 24, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 75, 161, new DateTime(2023, 08, 27, 13, 26, 29), new DateTime(2023, 08, 27, 13, 27, 44)),
            [33] = CycleRow(33, 24, 300, CycleStatus.Started, PartStatus.Ok, 1, 62, 123, new DateTime(2023, 08, 27, 13, 27, 44), new DateTime(2023, 08, 27, 13, 28, 46)),
            [34] = CycleRow(34, 25, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 60, 159, new DateTime(2023, 08, 27, 13, 31, 55), new DateTime(2023, 08, 27, 13, 32, 55)),
            [35] = CycleRow(35, 25, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 87, 168, new DateTime(2023, 08, 27, 13, 32, 55), new DateTime(2023, 08, 27, 13, 34, 22)),
            [36] = CycleRow(36, 26, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 95, 187, new DateTime(2023, 08, 27, 13, 37, 13), new DateTime(2023, 08, 27, 13, 38, 48)),
            [37] = CycleRow(37, 26, 300, CycleStatus.Started, PartStatus.Ok, 1, 54, 139, new DateTime(2023, 08, 27, 13, 38, 48), new DateTime(2023, 08, 27, 13, 39, 42)),
            [38] = CycleRow(38, 27, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 56, 147, new DateTime(2023, 08, 27, 15, 03, 53), new DateTime(2023, 08, 27, 15, 04, 49)),
            [39] = CycleRow(39, 27, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 97, 174, new DateTime(2023, 08, 27, 15, 04, 49), new DateTime(2023, 08, 27, 15, 06, 26)),
            [40] = CycleRow(40, 28, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 67, 129, new DateTime(2023, 08, 27, 16, 54, 58), new DateTime(2023, 08, 27, 16, 56, 05)),
            [41] = CycleRow(41, 28, 300, CycleStatus.Started, PartStatus.Ok, 1, 60, 157, new DateTime(2023, 08, 27, 16, 56, 05), new DateTime(2023, 08, 27, 16, 57, 05)),
            [42] = CycleRow(42, 29, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 79, 139, new DateTime(2023, 08, 27, 09, 52, 09), new DateTime(2023, 08, 27, 09, 53, 28)),
            [43] = CycleRow(43, 29, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 83, 138, new DateTime(2023, 08, 27, 09, 53, 28), new DateTime(2023, 08, 27, 09, 54, 51)),
            [44] = CycleRow(44, 30, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 54, 106, new DateTime(2023, 08, 27, 09, 54, 09), new DateTime(2023, 08, 27, 09, 55, 03)),
            [45] = CycleRow(45, 30, 300, CycleStatus.Started, PartStatus.Ok, 1, 73, 158, new DateTime(2023, 08, 27, 09, 55, 03), new DateTime(2023, 08, 27, 09, 56, 16)),
            [46] = CycleRow(46, 31, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 66, 158, new DateTime(2023, 08, 27, 10, 00, 17), new DateTime(2023, 08, 27, 10, 01, 23)),
            [47] = CycleRow(47, 31, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 87, 153, new DateTime(2023, 08, 27, 10, 01, 23), new DateTime(2023, 08, 27, 10, 02, 50)),
            [48] = CycleRow(48, 31, 500, CycleStatus.Started, PartStatus.Ok, 1, 80, 155, new DateTime(2023, 08, 27, 10, 02, 50), new DateTime(2023, 08, 27, 10, 04, 10)),
            [49] = CycleRow(49, 32, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 85, 149, new DateTime(2023, 08, 27, 10, 01, 14), new DateTime(2023, 08, 27, 10, 02, 39)),
            [50] = CycleRow(50, 32, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 185, new DateTime(2023, 08, 27, 10, 02, 39), new DateTime(2023, 08, 27, 10, 04, 07)),
            [51] = CycleRow(51, 32, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 62, 128, new DateTime(2023, 08, 27, 10, 01, 14), new DateTime(2023, 08, 27, 10, 02, 16)),
            [52] = CycleRow(52, 33, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 95, 177, new DateTime(2023, 08, 27, 10, 01, 50), new DateTime(2023, 08, 27, 10, 03, 25)),
            [53] = CycleRow(53, 33, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 100, 160, new DateTime(2023, 08, 27, 10, 03, 25), new DateTime(2023, 08, 27, 10, 05, 05)),
            [54] = CycleRow(54, 33, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 74, 174, new DateTime(2023, 08, 27, 10, 05, 05), new DateTime(2023, 08, 27, 10, 06, 19)),
            [55] = CycleRow(55, 34, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 79, 139, new DateTime(2023, 08, 27, 10, 03, 23), new DateTime(2023, 08, 27, 10, 04, 42)),
            [56] = CycleRow(56, 34, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 65, 133, new DateTime(2023, 08, 27, 10, 04, 42), new DateTime(2023, 08, 27, 10, 05, 47)),
            [57] = CycleRow(57, 34, 500, CycleStatus.Started, PartStatus.Ok, 1, 75, 146, new DateTime(2023, 08, 27, 10, 05, 47), new DateTime(2023, 08, 27, 10, 07, 02)),
            [58] = CycleRow(58, 35, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 89, 182, new DateTime(2023, 08, 27, 10, 05, 46), new DateTime(2023, 08, 27, 10, 07, 15)),
            [59] = CycleRow(59, 35, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 52, 145, new DateTime(2023, 08, 27, 10, 07, 15), new DateTime(2023, 08, 27, 10, 08, 07)),
            [60] = CycleRow(60, 35, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 52, 105, new DateTime(2023, 08, 27, 10, 05, 46), new DateTime(2023, 08, 27, 10, 06, 38)),
            [61] = CycleRow(61, 36, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 98, 165, new DateTime(2023, 08, 27, 10, 06, 13), new DateTime(2023, 08, 27, 10, 07, 51)),
            [62] = CycleRow(62, 36, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 79, 163, new DateTime(2023, 08, 27, 10, 07, 51), new DateTime(2023, 08, 27, 10, 09, 10)),
            [63] = CycleRow(63, 36, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 100, 174, new DateTime(2023, 08, 27, 10, 09, 10), new DateTime(2023, 08, 27, 10, 10, 50)),
            [64] = CycleRow(64, 37, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 78, 132, new DateTime(2023, 08, 27, 10, 06, 27), new DateTime(2023, 08, 27, 10, 07, 45)),
            [65] = CycleRow(65, 37, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 81, 144, new DateTime(2023, 08, 27, 10, 07, 45), new DateTime(2023, 08, 27, 10, 09, 06)),
            [66] = CycleRow(66, 37, 500, CycleStatus.Started, PartStatus.Ok, 1, 84, 173, new DateTime(2023, 08, 27, 10, 09, 06), new DateTime(2023, 08, 27, 10, 10, 30)),
            [67] = CycleRow(67, 38, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 55, 112, new DateTime(2023, 08, 27, 10, 07, 47), new DateTime(2023, 08, 27, 10, 08, 42)),
            [68] = CycleRow(68, 38, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 100, 158, new DateTime(2023, 08, 27, 10, 08, 42), new DateTime(2023, 08, 27, 10, 10, 22)),
            [69] = CycleRow(69, 38, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 84, 154, new DateTime(2023, 08, 27, 10, 07, 47), new DateTime(2023, 08, 27, 10, 09, 11)),
            [70] = CycleRow(70, 39, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 90, 149, new DateTime(2023, 08, 27, 10, 13, 47), new DateTime(2023, 08, 27, 10, 15, 17)),
            [71] = CycleRow(71, 39, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 68, 139, new DateTime(2023, 08, 27, 10, 15, 17), new DateTime(2023, 08, 27, 10, 16, 25)),
            [72] = CycleRow(72, 39, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 86, 154, new DateTime(2023, 08, 27, 10, 16, 25), new DateTime(2023, 08, 27, 10, 17, 51)),
            [73] = CycleRow(73, 40, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 63, 135, new DateTime(2023, 08, 27, 10, 13, 59), new DateTime(2023, 08, 27, 10, 15, 02)),
            [74] = CycleRow(74, 40, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 100, 159, new DateTime(2023, 08, 27, 10, 15, 02), new DateTime(2023, 08, 27, 10, 16, 42)),
            [75] = CycleRow(75, 40, 500, CycleStatus.Started, PartStatus.Ok, 1, 100, 183, new DateTime(2023, 08, 27, 10, 16, 42), new DateTime(2023, 08, 27, 10, 18, 22)),
            [76] = CycleRow(76, 41, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 61, 133, new DateTime(2023, 08, 27, 10, 16, 39), new DateTime(2023, 08, 27, 10, 17, 40)),
            [77] = CycleRow(77, 41, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 92, 163, new DateTime(2023, 08, 27, 10, 17, 40), new DateTime(2023, 08, 27, 10, 19, 12)),
            [78] = CycleRow(78, 41, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 93, 157, new DateTime(2023, 08, 27, 10, 16, 39), new DateTime(2023, 08, 27, 10, 18, 12)),
            [79] = CycleRow(79, 42, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 92, 190, new DateTime(2023, 08, 27, 10, 35, 21), new DateTime(2023, 08, 27, 10, 36, 53)),
            [80] = CycleRow(80, 42, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 83, 165, new DateTime(2023, 08, 27, 10, 36, 53), new DateTime(2023, 08, 27, 10, 38, 16)),
            [81] = CycleRow(81, 42, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 77, 146, new DateTime(2023, 08, 27, 10, 38, 16), new DateTime(2023, 08, 27, 10, 39, 33)),
            [82] = CycleRow(82, 43, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 82, 134, new DateTime(2023, 08, 27, 10, 35, 49), new DateTime(2023, 08, 27, 10, 37, 11)),
            [83] = CycleRow(83, 43, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 92, 185, new DateTime(2023, 08, 27, 10, 37, 11), new DateTime(2023, 08, 27, 10, 38, 43)),
            [84] = CycleRow(84, 43, 500, CycleStatus.Started, PartStatus.Ok, 1, 80, 147, new DateTime(2023, 08, 27, 10, 38, 43), new DateTime(2023, 08, 27, 10, 40, 03)),
            [85] = CycleRow(85, 44, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 173, new DateTime(2023, 08, 27, 10, 43, 32), new DateTime(2023, 08, 27, 10, 45, 11)),
            [86] = CycleRow(86, 44, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 69, 120, new DateTime(2023, 08, 27, 10, 45, 11), new DateTime(2023, 08, 27, 10, 46, 20)),
            [87] = CycleRow(87, 44, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 99, 174, new DateTime(2023, 08, 27, 10, 43, 32), new DateTime(2023, 08, 27, 10, 45, 11)),
            [88] = CycleRow(88, 45, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 97, 193, new DateTime(2023, 08, 27, 10, 49, 58), new DateTime(2023, 08, 27, 10, 51, 35)),
            [89] = CycleRow(89, 45, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 61, 119, new DateTime(2023, 08, 27, 10, 51, 35), new DateTime(2023, 08, 27, 10, 52, 36)),
            [90] = CycleRow(90, 45, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 66, 143, new DateTime(2023, 08, 27, 10, 52, 36), new DateTime(2023, 08, 27, 10, 53, 42)),
            [91] = CycleRow(91, 46, 100, CycleStatus.Started, PartStatus.Ok, 1, 93, 153, new DateTime(2023, 08, 27, 10, 50, 56), new DateTime(2023, 08, 27, 10, 52, 29)),
            [92] = CycleRow(92, 47, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 83, 162, new DateTime(2023, 08, 27, 10, 51, 25), new DateTime(2023, 08, 27, 10, 52, 48)),
            [93] = CycleRow(93, 48, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 100, 200, new DateTime(2023, 08, 27, 10, 53, 08), new DateTime(2023, 08, 27, 10, 54, 48)),
            [94] = CycleRow(94, 49, 100, CycleStatus.Started, PartStatus.Ok, 1, 65, 148, new DateTime(2023, 08, 27, 10, 53, 32), new DateTime(2023, 08, 27, 10, 54, 37)),
            [95] = CycleRow(95, 50, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 60, 145, new DateTime(2023, 08, 27, 10, 59, 49), new DateTime(2023, 08, 27, 11, 00, 49)),
            [96] = CycleRow(96, 51, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 76, 163, new DateTime(2023, 08, 27, 11, 01, 12), new DateTime(2023, 08, 27, 11, 02, 28)),
            [97] = CycleRow(97, 52, 100, CycleStatus.Started, PartStatus.Ok, 1, 57, 113, new DateTime(2023, 08, 27, 11, 01, 23), new DateTime(2023, 08, 27, 11, 02, 20)),
            [98] = CycleRow(98, 53, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 72, 122, new DateTime(2023, 08, 27, 12, 27, 18), new DateTime(2023, 08, 27, 12, 28, 30)),
            [99] = CycleRow(99, 54, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 72, 139, new DateTime(2023, 08, 27, 13, 36, 40), new DateTime(2023, 08, 27, 13, 37, 52)),
            [100] = CycleRow(100, 55, 100, CycleStatus.Started, PartStatus.Ok, 1, 90, 181, new DateTime(2023, 08, 27, 14, 01, 00), new DateTime(2023, 08, 27, 14, 02, 30)),
            [101] = CycleRow(101, 56, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 82, 171, new DateTime(2023, 08, 27, 16, 25, 12), new DateTime(2023, 08, 27, 16, 26, 34)),
            [102] = CycleRow(102, 57, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 67, 153, new DateTime(2023, 08, 27, 17, 04, 37), new DateTime(2023, 08, 27, 17, 05, 44)),
            [103] = CycleRow(103, 58, 100, CycleStatus.Started, PartStatus.Ok, 1, 95, 162, new DateTime(2023, 08, 27, 17, 16, 44), new DateTime(2023, 08, 27, 17, 18, 19)),
            [104] = CycleRow(104, 59, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 94, 165, new DateTime(2023, 08, 27, 17, 19, 30), new DateTime(2023, 08, 27, 17, 21, 04)),
            [105] = CycleRow(105, 60, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 85, 143, new DateTime(2023, 08, 27, 15, 06, 56), new DateTime(2023, 08, 27, 15, 08, 21)),
            [106] = CycleRow(106, 61, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 60, 160, new DateTime(2023, 08, 27, 15, 44, 41), new DateTime(2023, 08, 27, 15, 45, 41)),
            [107] = CycleRow(107, 61, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 82, 146, new DateTime(2023, 08, 27, 15, 45, 41), new DateTime(2023, 08, 27, 15, 47, 03)),
            [108] = CycleRow(108, 62, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 72, 169, new DateTime(2023, 08, 27, 15, 45, 43), new DateTime(2023, 08, 27, 15, 46, 55)),
            [109] = CycleRow(109, 62, 300, CycleStatus.Started, PartStatus.Ok, 1, 63, 154, new DateTime(2023, 08, 27, 15, 46, 55), new DateTime(2023, 08, 27, 15, 47, 58)),
            [110] = CycleRow(110, 63, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 96, 191, new DateTime(2023, 08, 27, 16, 25, 05), new DateTime(2023, 08, 27, 16, 26, 41)),
            [111] = CycleRow(111, 63, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 96, 179, new DateTime(2023, 08, 27, 16, 26, 41), new DateTime(2023, 08, 27, 16, 28, 17)),
            [112] = CycleRow(112, 64, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 93, 155, new DateTime(2023, 08, 27, 09, 02, 38), new DateTime(2023, 08, 27, 09, 04, 11)),
            [113] = CycleRow(113, 64, 300, CycleStatus.Started, PartStatus.Ok, 1, 88, 179, new DateTime(2023, 08, 27, 09, 04, 11), new DateTime(2023, 08, 27, 09, 05, 39)),
            [114] = CycleRow(114, 65, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 93, 175, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 19, 47)),
            [115] = CycleRow(115, 65, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 57, 108, new DateTime(2023, 08, 27, 09, 19, 47), new DateTime(2023, 08, 27, 09, 20, 44)),
            [116] = CycleRow(116, 66, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 90, 153, new DateTime(2023, 08, 27, 10, 58, 00), new DateTime(2023, 08, 27, 10, 59, 30)),
            [117] = CycleRow(117, 66, 300, CycleStatus.Started, PartStatus.Ok, 1, 75, 140, new DateTime(2023, 08, 27, 10, 59, 30), new DateTime(2023, 08, 27, 11, 00, 45)),
            [118] = CycleRow(118, 67, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 89, 144, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 33, 21)),
            [119] = CycleRow(119, 67, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 71, 161, new DateTime(2023, 08, 27, 15, 33, 21), new DateTime(2023, 08, 27, 15, 34, 32)),
            [120] = CycleRow(120, 68, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 100, 161, new DateTime(2023, 08, 27, 15, 46, 00), new DateTime(2023, 08, 27, 15, 47, 40)),
            [121] = CycleRow(121, 68, 300, CycleStatus.Started, PartStatus.Ok, 1, 61, 149, new DateTime(2023, 08, 27, 15, 47, 40), new DateTime(2023, 08, 27, 15, 48, 41)),
            [122] = CycleRow(122, 69, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 69, 131, new DateTime(2023, 08, 27, 10, 53, 32), new DateTime(2023, 08, 27, 10, 54, 41)),
            [123] = CycleRow(123, 69, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 87, 181, new DateTime(2023, 08, 27, 10, 54, 41), new DateTime(2023, 08, 27, 10, 56, 08)),
            [124] = CycleRow(124, 70, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 92, 188, new DateTime(2023, 08, 27, 10, 59, 49), new DateTime(2023, 08, 27, 11, 01, 21)),
            [125] = CycleRow(125, 70, 300, CycleStatus.Started, PartStatus.Ok, 1, 56, 118, new DateTime(2023, 08, 27, 11, 01, 21), new DateTime(2023, 08, 27, 11, 02, 17)),
            [126] = CycleRow(126, 71, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 64, 133, new DateTime(2023, 08, 27, 11, 01, 12), new DateTime(2023, 08, 27, 11, 02, 16)),
            [127] = CycleRow(127, 71, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 68, 159, new DateTime(2023, 08, 27, 11, 02, 16), new DateTime(2023, 08, 27, 11, 03, 24)),
            [128] = CycleRow(128, 72, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 52, 138, new DateTime(2023, 08, 27, 11, 01, 23), new DateTime(2023, 08, 27, 11, 02, 15)),
            [129] = CycleRow(129, 72, 300, CycleStatus.Started, PartStatus.Ok, 1, 65, 165, new DateTime(2023, 08, 27, 11, 02, 15), new DateTime(2023, 08, 27, 11, 03, 20)),
            [130] = CycleRow(130, 73, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 71, 126, new DateTime(2023, 08, 27, 12, 27, 18), new DateTime(2023, 08, 27, 12, 28, 29)),
            [131] = CycleRow(131, 73, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 72, 149, new DateTime(2023, 08, 27, 12, 28, 29), new DateTime(2023, 08, 27, 12, 29, 41)),
            [132] = CycleRow(132, 74, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 91, 156, new DateTime(2023, 08, 27, 13, 36, 40), new DateTime(2023, 08, 27, 13, 38, 11)),
            [133] = CycleRow(133, 74, 300, CycleStatus.Started, PartStatus.Ok, 1, 72, 143, new DateTime(2023, 08, 27, 13, 38, 11), new DateTime(2023, 08, 27, 13, 39, 23)),
            [134] = CycleRow(134, 75, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 71, 135, new DateTime(2023, 08, 27, 14, 01, 00), new DateTime(2023, 08, 27, 14, 02, 11)),
            [135] = CycleRow(135, 75, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 84, 157, new DateTime(2023, 08, 27, 14, 02, 11), new DateTime(2023, 08, 27, 14, 03, 35)),
            [136] = CycleRow(136, 76, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 56, 106, new DateTime(2023, 08, 27, 16, 25, 12), new DateTime(2023, 08, 27, 16, 26, 08)),
            [137] = CycleRow(137, 76, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 66, 117, new DateTime(2023, 08, 27, 16, 26, 08), new DateTime(2023, 08, 27, 16, 27, 14)),
            [138] = CycleRow(138, 76, 500, CycleStatus.Started, PartStatus.Ok, 1, 76, 143, new DateTime(2023, 08, 27, 16, 27, 14), new DateTime(2023, 08, 27, 16, 28, 30)),
            [139] = CycleRow(139, 77, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 87, 147, new DateTime(2023, 08, 27, 17, 04, 37), new DateTime(2023, 08, 27, 17, 06, 04)),
            [140] = CycleRow(140, 77, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 56, 112, new DateTime(2023, 08, 27, 17, 06, 04), new DateTime(2023, 08, 27, 17, 07, 00)),
            [141] = CycleRow(141, 77, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 96, 146, new DateTime(2023, 08, 27, 17, 04, 37), new DateTime(2023, 08, 27, 17, 06, 13)),
            [142] = CycleRow(142, 78, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 163, new DateTime(2023, 08, 27, 17, 16, 44), new DateTime(2023, 08, 27, 17, 18, 23)),
            [143] = CycleRow(143, 78, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 160, new DateTime(2023, 08, 27, 17, 18, 23), new DateTime(2023, 08, 27, 17, 19, 51)),
            [144] = CycleRow(144, 78, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 74, 162, new DateTime(2023, 08, 27, 17, 19, 51), new DateTime(2023, 08, 27, 17, 21, 05)),
            [145] = CycleRow(145, 79, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 66, 166, new DateTime(2023, 08, 27, 17, 19, 30), new DateTime(2023, 08, 27, 17, 20, 36)),
            [146] = CycleRow(146, 79, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 55, 114, new DateTime(2023, 08, 27, 17, 20, 36), new DateTime(2023, 08, 27, 17, 21, 31)),
            [147] = CycleRow(147, 79, 500, CycleStatus.Started, PartStatus.Ok, 1, 50, 103, new DateTime(2023, 08, 27, 17, 21, 31), new DateTime(2023, 08, 27, 17, 22, 21)),
            [148] = CycleRow(148, 80, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 70, 163, new DateTime(2023, 08, 27, 15, 06, 56), new DateTime(2023, 08, 27, 15, 08, 06)),
            [149] = CycleRow(149, 80, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 80, 139, new DateTime(2023, 08, 27, 15, 08, 06), new DateTime(2023, 08, 27, 15, 09, 26)),
            [150] = CycleRow(150, 80, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 67, 145, new DateTime(2023, 08, 27, 15, 06, 56), new DateTime(2023, 08, 27, 15, 08, 03)),
            [151] = CycleRow(151, 81, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 184, new DateTime(2023, 08, 27, 15, 44, 41), new DateTime(2023, 08, 27, 15, 46, 05)),
            [152] = CycleRow(152, 81, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 71, 132, new DateTime(2023, 08, 27, 15, 46, 05), new DateTime(2023, 08, 27, 15, 47, 16)),
            [153] = CycleRow(153, 81, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 76, 142, new DateTime(2023, 08, 27, 15, 47, 16), new DateTime(2023, 08, 27, 15, 48, 32)),
            [154] = CycleRow(154, 82, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 144, new DateTime(2023, 08, 27, 15, 45, 43), new DateTime(2023, 08, 27, 15, 47, 07)),
            [155] = CycleRow(155, 82, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 96, 177, new DateTime(2023, 08, 27, 15, 47, 07), new DateTime(2023, 08, 27, 15, 48, 43)),
            [156] = CycleRow(156, 82, 500, CycleStatus.Started, PartStatus.Ok, 1, 97, 194, new DateTime(2023, 08, 27, 15, 48, 43), new DateTime(2023, 08, 27, 15, 50, 20)),
            [157] = CycleRow(157, 83, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 95, 150, new DateTime(2023, 08, 27, 16, 25, 05), new DateTime(2023, 08, 27, 16, 26, 40)),
            [158] = CycleRow(158, 83, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 93, 185, new DateTime(2023, 08, 27, 16, 26, 40), new DateTime(2023, 08, 27, 16, 28, 13)),
            [159] = CycleRow(159, 83, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 74, 141, new DateTime(2023, 08, 27, 16, 25, 05), new DateTime(2023, 08, 27, 16, 26, 19)),
            [160] = CycleRow(160, 84, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 69, 143, new DateTime(2023, 08, 27, 09, 02, 38), new DateTime(2023, 08, 27, 09, 03, 47)),
            [161] = CycleRow(161, 84, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 81, 156, new DateTime(2023, 08, 27, 09, 03, 47), new DateTime(2023, 08, 27, 09, 05, 08)),
            [162] = CycleRow(162, 84, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 74, 171, new DateTime(2023, 08, 27, 09, 05, 08), new DateTime(2023, 08, 27, 09, 06, 22)),
            [163] = CycleRow(163, 85, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 146, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 19, 42)),
            [164] = CycleRow(164, 85, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 52, 110, new DateTime(2023, 08, 27, 09, 19, 42), new DateTime(2023, 08, 27, 09, 20, 34)),
            [165] = CycleRow(165, 85, 500, CycleStatus.Started, PartStatus.Ok, 1, 54, 115, new DateTime(2023, 08, 27, 09, 20, 34), new DateTime(2023, 08, 27, 09, 21, 28)),
            [166] = CycleRow(166, 86, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 96, 154, new DateTime(2023, 08, 27, 10, 58, 00), new DateTime(2023, 08, 27, 10, 59, 36)),
            [167] = CycleRow(167, 86, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 67, 127, new DateTime(2023, 08, 27, 10, 59, 36), new DateTime(2023, 08, 27, 11, 00, 43)),
            [168] = CycleRow(168, 86, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 91, 185, new DateTime(2023, 08, 27, 10, 58, 00), new DateTime(2023, 08, 27, 10, 59, 31)),
            [169] = CycleRow(169, 87, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 153, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 33, 20)),
            [170] = CycleRow(170, 87, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 63, 128, new DateTime(2023, 08, 27, 15, 33, 20), new DateTime(2023, 08, 27, 15, 34, 23)),
            [171] = CycleRow(171, 87, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 68, 148, new DateTime(2023, 08, 27, 15, 34, 23), new DateTime(2023, 08, 27, 15, 35, 31)),
            [172] = CycleRow(172, 88, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 56, 111, new DateTime(2023, 08, 27, 15, 46, 00), new DateTime(2023, 08, 27, 15, 46, 56)),
            [173] = CycleRow(173, 88, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 100, 179, new DateTime(2023, 08, 27, 15, 46, 56), new DateTime(2023, 08, 27, 15, 48, 36)),
            [174] = CycleRow(174, 88, 500, CycleStatus.Started, PartStatus.Ok, 1, 66, 137, new DateTime(2023, 08, 27, 15, 48, 36), new DateTime(2023, 08, 27, 15, 49, 42)),
            [175] = CycleRow(175, 89, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 63, 155, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 32, 55)),
            [176] = CycleRow(176, 89, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 59, 155, new DateTime(2023, 08, 27, 15, 32, 55), new DateTime(2023, 08, 27, 15, 33, 54)),
            [177] = CycleRow(177, 89, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 96, 169, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 33, 28)),
            [178] = CycleRow(178, 90, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 63, 158, new DateTime(2023, 08, 27, 15, 46, 00), new DateTime(2023, 08, 27, 15, 47, 03)),
            [179] = CycleRow(179, 90, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 89, 168, new DateTime(2023, 08, 27, 15, 47, 03), new DateTime(2023, 08, 27, 15, 48, 32)),
            [180] = CycleRow(180, 90, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 82, 178, new DateTime(2023, 08, 27, 15, 48, 32), new DateTime(2023, 08, 27, 15, 49, 54)),
            [181] = CycleRow(181, 91, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 193, new DateTime(2023, 08, 27, 15, 44, 41), new DateTime(2023, 08, 27, 15, 46, 20)),
            [182] = CycleRow(182, 91, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 64, 137, new DateTime(2023, 08, 27, 15, 46, 20), new DateTime(2023, 08, 27, 15, 47, 24)),
            [183] = CycleRow(183, 91, 500, CycleStatus.Started, PartStatus.Ok, 1, 65, 155, new DateTime(2023, 08, 27, 15, 47, 24), new DateTime(2023, 08, 27, 15, 48, 29)),
            [184] = CycleRow(184, 92, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 69, 139, new DateTime(2023, 08, 27, 15, 45, 43), new DateTime(2023, 08, 27, 15, 46, 52)),
            [185] = CycleRow(185, 92, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 70, 157, new DateTime(2023, 08, 27, 15, 46, 52), new DateTime(2023, 08, 27, 15, 48, 02)),
            [186] = CycleRow(186, 92, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 72, 130, new DateTime(2023, 08, 27, 15, 45, 43), new DateTime(2023, 08, 27, 15, 46, 55)),
            [187] = CycleRow(187, 93, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 100, 196, new DateTime(2023, 08, 27, 16, 25, 05), new DateTime(2023, 08, 27, 16, 26, 45)),
            [188] = CycleRow(188, 93, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 100, 199, new DateTime(2023, 08, 27, 16, 26, 45), new DateTime(2023, 08, 27, 16, 28, 25)),
            [189] = CycleRow(189, 93, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 98, 195, new DateTime(2023, 08, 27, 16, 28, 25), new DateTime(2023, 08, 27, 16, 30, 03)),
            [190] = CycleRow(190, 94, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 97, 182, new DateTime(2023, 08, 27, 09, 02, 38), new DateTime(2023, 08, 27, 09, 04, 15)),
            [191] = CycleRow(191, 94, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 86, 165, new DateTime(2023, 08, 27, 09, 04, 15), new DateTime(2023, 08, 27, 09, 05, 41)),
            [192] = CycleRow(192, 94, 500, CycleStatus.Started, PartStatus.Ok, 1, 62, 137, new DateTime(2023, 08, 27, 09, 05, 41), new DateTime(2023, 08, 27, 09, 06, 43)),
            [193] = CycleRow(193, 95, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 71, 140, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 19, 25)),
            [194] = CycleRow(194, 95, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 160, new DateTime(2023, 08, 27, 09, 19, 25), new DateTime(2023, 08, 27, 09, 20, 49)),
            [195] = CycleRow(195, 95, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 94, 183, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 19, 48)),
            [196] = CycleRow(196, 96, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 92, 148, new DateTime(2023, 08, 27, 10, 58, 00), new DateTime(2023, 08, 27, 10, 59, 32)),
            [197] = CycleRow(197, 96, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 77, 156, new DateTime(2023, 08, 27, 10, 59, 32), new DateTime(2023, 08, 27, 11, 00, 49)),
            [198] = CycleRow(198, 96, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 58, 116, new DateTime(2023, 08, 27, 11, 00, 49), new DateTime(2023, 08, 27, 11, 01, 47)),
            [199] = CycleRow(199, 97, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 70, 163, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 33, 02)),
            [200] = CycleRow(200, 97, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 64, 123, new DateTime(2023, 08, 27, 15, 33, 02), new DateTime(2023, 08, 27, 15, 34, 06)),
            [201] = CycleRow(201, 97, 500, CycleStatus.Started, PartStatus.Ok, 1, 98, 157, new DateTime(2023, 08, 27, 15, 34, 06), new DateTime(2023, 08, 27, 15, 35, 44)),
            [202] = CycleRow(202, 98, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 85, 147, new DateTime(2023, 08, 27, 15, 46, 00), new DateTime(2023, 08, 27, 15, 47, 25)),
            [203] = CycleRow(203, 98, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 61, 122, new DateTime(2023, 08, 27, 15, 47, 25), new DateTime(2023, 08, 27, 15, 48, 26)),
            [204] = CycleRow(204, 98, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 60, 132, new DateTime(2023, 08, 27, 15, 46, 00), new DateTime(2023, 08, 27, 15, 47, 00)),
            [205] = CycleRow(205, 99, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 170, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 33, 20)),
            [206] = CycleRow(206, 99, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 63, 137, new DateTime(2023, 08, 27, 15, 33, 20), new DateTime(2023, 08, 27, 15, 34, 23)),
            [207] = CycleRow(207, 99, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 81, 134, new DateTime(2023, 08, 27, 15, 34, 23), new DateTime(2023, 08, 27, 15, 35, 44)),
            [208] = CycleRow(208, 100, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 81, 139, new DateTime(2023, 08, 27, 15, 46, 00), new DateTime(2023, 08, 27, 15, 47, 21)),
            [209] = CycleRow(209, 100, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 55, 140, new DateTime(2023, 08, 27, 15, 47, 21), new DateTime(2023, 08, 27, 15, 48, 16)),
            [210] = CycleRow(210, 100, 500, CycleStatus.Started, PartStatus.Ok, 1, 60, 139, new DateTime(2023, 08, 27, 15, 48, 16), new DateTime(2023, 08, 27, 15, 49, 16)),
            [211] = CycleRow(211, 101, 100, CycleStatus.Started, PartStatus.Ok, 1, 65, 135, new DateTime(2023, 08, 27, 00, 49, 12), new DateTime(2023, 08, 27, 00, 50, 17)),
            [212] = CycleRow(212, 102, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 97, 154, new DateTime(2023, 08, 27, 02, 54, 19), new DateTime(2023, 08, 27, 02, 55, 56)),
            [213] = CycleRow(213, 103, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 83, 143, new DateTime(2023, 08, 27, 08, 18, 00), new DateTime(2023, 08, 27, 08, 19, 23)),
            [214] = CycleRow(214, 104, 100, CycleStatus.Started, PartStatus.Ok, 1, 92, 187, new DateTime(2023, 08, 27, 03, 41, 07), new DateTime(2023, 08, 27, 03, 42, 39)),
            [215] = CycleRow(215, 105, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 186, new DateTime(2023, 08, 27, 09, 27, 28), new DateTime(2023, 08, 27, 09, 28, 56)),
            [216] = CycleRow(216, 106, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 90, 143, new DateTime(2023, 08, 27, 10, 01, 55), new DateTime(2023, 08, 27, 10, 03, 25)),
            [217] = CycleRow(217, 107, 100, CycleStatus.Started, PartStatus.Ok, 1, 91, 149, new DateTime(2023, 08, 27, 11, 50, 57), new DateTime(2023, 08, 27, 11, 52, 28)),
            [218] = CycleRow(218, 108, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 95, 153, new DateTime(2023, 08, 27, 12, 03, 04), new DateTime(2023, 08, 27, 12, 04, 39)),
            [219] = CycleRow(219, 109, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 90, 185, new DateTime(2023, 08, 27, 12, 20, 08), new DateTime(2023, 08, 27, 12, 21, 38)),
            [220] = CycleRow(220, 110, 100, CycleStatus.Started, PartStatus.Ok, 1, 94, 153, new DateTime(2023, 08, 27, 12, 19, 35), new DateTime(2023, 08, 27, 12, 21, 09)),
            [221] = CycleRow(221, 111, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 77, 160, new DateTime(2023, 08, 27, 09, 22, 03), new DateTime(2023, 08, 27, 09, 23, 20)),
            [222] = CycleRow(222, 112, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 59, 120, new DateTime(2023, 08, 27, 09, 25, 05), new DateTime(2023, 08, 27, 09, 26, 04)),
            [223] = CycleRow(223, 113, 100, CycleStatus.Started, PartStatus.Ok, 1, 51, 118, new DateTime(2023, 08, 27, 12, 09, 01), new DateTime(2023, 08, 27, 12, 09, 52)),
            [224] = CycleRow(224, 114, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 93, 166, new DateTime(2023, 08, 27, 12, 12, 01), new DateTime(2023, 08, 27, 12, 13, 34)),
            [225] = CycleRow(225, 115, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 96, 148, new DateTime(2023, 08, 27, 12, 16, 03), new DateTime(2023, 08, 27, 12, 17, 39)),
            [226] = CycleRow(226, 116, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 98, 148, new DateTime(2023, 08, 27, 12, 19, 31), new DateTime(2023, 08, 27, 12, 21, 09)),
            [227] = CycleRow(227, 116, 300, CycleStatus.Started, PartStatus.Ok, 1, 99, 166, new DateTime(2023, 08, 27, 12, 21, 09), new DateTime(2023, 08, 27, 12, 22, 48)),
            [228] = CycleRow(228, 117, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 75, 145, new DateTime(2023, 08, 27, 12, 20, 56), new DateTime(2023, 08, 27, 12, 22, 11)),
            [229] = CycleRow(229, 117, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 59, 109, new DateTime(2023, 08, 27, 12, 22, 11), new DateTime(2023, 08, 27, 12, 23, 10)),
            [230] = CycleRow(230, 118, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 58, 145, new DateTime(2023, 08, 27, 12, 22, 36), new DateTime(2023, 08, 27, 12, 23, 34)),
            [231] = CycleRow(231, 118, 300, CycleStatus.Started, PartStatus.Ok, 1, 99, 186, new DateTime(2023, 08, 27, 12, 23, 34), new DateTime(2023, 08, 27, 12, 25, 13)),
            [232] = CycleRow(232, 119, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 146, new DateTime(2023, 08, 27, 12, 21, 23), new DateTime(2023, 08, 27, 12, 22, 47)),
            [233] = CycleRow(233, 119, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 76, 147, new DateTime(2023, 08, 27, 12, 22, 47), new DateTime(2023, 08, 27, 12, 24, 03)),
            [234] = CycleRow(234, 120, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 50, 131, new DateTime(2023, 08, 27, 12, 22, 16), new DateTime(2023, 08, 27, 12, 23, 06)),
            [235] = CycleRow(235, 120, 300, CycleStatus.Started, PartStatus.Ok, 1, 83, 181, new DateTime(2023, 08, 27, 12, 23, 06), new DateTime(2023, 08, 27, 12, 24, 29)),
            [236] = CycleRow(236, 121, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 146, new DateTime(2023, 08, 27, 12, 20, 02), new DateTime(2023, 08, 27, 12, 21, 26)),
            [237] = CycleRow(237, 121, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 89, 149, new DateTime(2023, 08, 27, 12, 21, 26), new DateTime(2023, 08, 27, 12, 22, 55)),
            [238] = CycleRow(238, 122, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 85, 154, new DateTime(2023, 08, 27, 12, 21, 58), new DateTime(2023, 08, 27, 12, 23, 23)),
            [239] = CycleRow(239, 122, 300, CycleStatus.Started, PartStatus.Ok, 1, 94, 180, new DateTime(2023, 08, 27, 12, 23, 23), new DateTime(2023, 08, 27, 12, 24, 57)),
            [240] = CycleRow(240, 123, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 146, new DateTime(2023, 08, 27, 13, 15, 45), new DateTime(2023, 08, 27, 13, 17, 13)),
            [241] = CycleRow(241, 123, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 82, 169, new DateTime(2023, 08, 27, 13, 17, 13), new DateTime(2023, 08, 27, 13, 18, 35)),
            [242] = CycleRow(242, 124, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 87, 156, new DateTime(2023, 08, 27, 13, 26, 29), new DateTime(2023, 08, 27, 13, 27, 56)),
            [243] = CycleRow(243, 124, 300, CycleStatus.Started, PartStatus.Ok, 1, 75, 125, new DateTime(2023, 08, 27, 13, 27, 56), new DateTime(2023, 08, 27, 13, 29, 11)),
            [244] = CycleRow(244, 125, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 81, 157, new DateTime(2023, 08, 27, 13, 31, 55), new DateTime(2023, 08, 27, 13, 33, 16)),
            [245] = CycleRow(245, 125, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 94, 147, new DateTime(2023, 08, 27, 13, 33, 16), new DateTime(2023, 08, 27, 13, 34, 50)),
            [246] = CycleRow(246, 126, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 180, new DateTime(2023, 08, 27, 13, 37, 13), new DateTime(2023, 08, 27, 13, 38, 37)),
            [247] = CycleRow(247, 126, 300, CycleStatus.Started, PartStatus.Ok, 1, 65, 146, new DateTime(2023, 08, 27, 13, 38, 37), new DateTime(2023, 08, 27, 13, 39, 42)),
            [248] = CycleRow(248, 127, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 62, 134, new DateTime(2023, 08, 27, 15, 03, 53), new DateTime(2023, 08, 27, 15, 04, 55)),
            [249] = CycleRow(249, 127, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 59, 150, new DateTime(2023, 08, 27, 15, 04, 55), new DateTime(2023, 08, 27, 15, 05, 54)),
            [250] = CycleRow(250, 128, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 69, 133, new DateTime(2023, 08, 27, 16, 54, 58), new DateTime(2023, 08, 27, 16, 56, 07)),
            [251] = CycleRow(251, 128, 300, CycleStatus.Started, PartStatus.Ok, 1, 75, 141, new DateTime(2023, 08, 27, 16, 56, 07), new DateTime(2023, 08, 27, 16, 57, 22)),
            [252] = CycleRow(252, 129, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 85, 146, new DateTime(2023, 08, 27, 09, 52, 09), new DateTime(2023, 08, 27, 09, 53, 34)),
            [253] = CycleRow(253, 129, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 87, 185, new DateTime(2023, 08, 27, 09, 53, 34), new DateTime(2023, 08, 27, 09, 55, 01)),
            [254] = CycleRow(254, 130, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 58, 149, new DateTime(2023, 08, 27, 09, 54, 09), new DateTime(2023, 08, 27, 09, 55, 07)),
            [255] = CycleRow(255, 130, 300, CycleStatus.Started, PartStatus.Ok, 1, 83, 159, new DateTime(2023, 08, 27, 09, 55, 07), new DateTime(2023, 08, 27, 09, 56, 30)),
            [256] = CycleRow(256, 131, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 65, 139, new DateTime(2023, 08, 27, 10, 00, 17), new DateTime(2023, 08, 27, 10, 01, 22)),
            [257] = CycleRow(257, 131, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 58, 124, new DateTime(2023, 08, 27, 10, 01, 22), new DateTime(2023, 08, 27, 10, 02, 20)),
            [258] = CycleRow(258, 131, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 57, 122, new DateTime(2023, 08, 27, 10, 00, 17), new DateTime(2023, 08, 27, 10, 01, 14)),
            [259] = CycleRow(259, 132, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 66, 124, new DateTime(2023, 08, 27, 10, 01, 14), new DateTime(2023, 08, 27, 10, 02, 20)),
            [260] = CycleRow(260, 132, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 80, 156, new DateTime(2023, 08, 27, 10, 02, 20), new DateTime(2023, 08, 27, 10, 03, 40)),
            [261] = CycleRow(261, 132, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 168, new DateTime(2023, 08, 27, 10, 03, 40), new DateTime(2023, 08, 27, 10, 05, 04)),
            [262] = CycleRow(262, 133, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 78, 159, new DateTime(2023, 08, 27, 10, 01, 50), new DateTime(2023, 08, 27, 10, 03, 08)),
            [263] = CycleRow(263, 133, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 70, 153, new DateTime(2023, 08, 27, 10, 03, 08), new DateTime(2023, 08, 27, 10, 04, 18)),
            [264] = CycleRow(264, 133, 500, CycleStatus.Started, PartStatus.Ok, 1, 73, 141, new DateTime(2023, 08, 27, 10, 04, 18), new DateTime(2023, 08, 27, 10, 05, 31)),
            [265] = CycleRow(265, 134, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 64, 142, new DateTime(2023, 08, 27, 10, 03, 23), new DateTime(2023, 08, 27, 10, 04, 27)),
            [266] = CycleRow(266, 134, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 98, 166, new DateTime(2023, 08, 27, 10, 04, 27), new DateTime(2023, 08, 27, 10, 06, 05)),
            [267] = CycleRow(267, 134, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 59, 118, new DateTime(2023, 08, 27, 10, 03, 23), new DateTime(2023, 08, 27, 10, 04, 22)),
            [268] = CycleRow(268, 135, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 59, 159, new DateTime(2023, 08, 27, 10, 05, 46), new DateTime(2023, 08, 27, 10, 06, 45)),
            [269] = CycleRow(269, 135, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 71, 163, new DateTime(2023, 08, 27, 10, 06, 45), new DateTime(2023, 08, 27, 10, 07, 56)),
            [270] = CycleRow(270, 135, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 61, 120, new DateTime(2023, 08, 27, 10, 07, 56), new DateTime(2023, 08, 27, 10, 08, 57)),
            [271] = CycleRow(271, 136, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 90, 167, new DateTime(2023, 08, 27, 10, 06, 13), new DateTime(2023, 08, 27, 10, 07, 43)),
            [272] = CycleRow(272, 136, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 78, 151, new DateTime(2023, 08, 27, 10, 07, 43), new DateTime(2023, 08, 27, 10, 09, 01)),
            [273] = CycleRow(273, 136, 500, CycleStatus.Started, PartStatus.Ok, 1, 95, 161, new DateTime(2023, 08, 27, 10, 09, 01), new DateTime(2023, 08, 27, 10, 10, 36)),
            [274] = CycleRow(274, 137, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 86, 154, new DateTime(2023, 08, 27, 10, 06, 27), new DateTime(2023, 08, 27, 10, 07, 53)),
            [275] = CycleRow(275, 137, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 83, 133, new DateTime(2023, 08, 27, 10, 07, 53), new DateTime(2023, 08, 27, 10, 09, 16)),
            [276] = CycleRow(276, 137, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 78, 136, new DateTime(2023, 08, 27, 10, 06, 27), new DateTime(2023, 08, 27, 10, 07, 45)),
            [277] = CycleRow(277, 138, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 62, 122, new DateTime(2023, 08, 27, 10, 07, 47), new DateTime(2023, 08, 27, 10, 08, 49)),
            [278] = CycleRow(278, 138, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 82, 141, new DateTime(2023, 08, 27, 10, 08, 49), new DateTime(2023, 08, 27, 10, 10, 11)),
            [279] = CycleRow(279, 138, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 64, 129, new DateTime(2023, 08, 27, 10, 10, 11), new DateTime(2023, 08, 27, 10, 11, 15)),
            [280] = CycleRow(280, 139, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 76, 160, new DateTime(2023, 08, 27, 10, 13, 47), new DateTime(2023, 08, 27, 10, 15, 03)),
            [281] = CycleRow(281, 139, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 96, 188, new DateTime(2023, 08, 27, 10, 15, 03), new DateTime(2023, 08, 27, 10, 16, 39)),
            [282] = CycleRow(282, 139, 500, CycleStatus.Started, PartStatus.Ok, 1, 84, 146, new DateTime(2023, 08, 27, 10, 16, 39), new DateTime(2023, 08, 27, 10, 18, 03)),
            [283] = CycleRow(283, 140, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 183, new DateTime(2023, 08, 27, 10, 13, 59), new DateTime(2023, 08, 27, 10, 15, 23)),
            [284] = CycleRow(284, 140, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 87, 154, new DateTime(2023, 08, 27, 10, 15, 23), new DateTime(2023, 08, 27, 10, 16, 50)),
            [285] = CycleRow(285, 140, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 72, 171, new DateTime(2023, 08, 27, 10, 13, 59), new DateTime(2023, 08, 27, 10, 15, 11)),
            [286] = CycleRow(286, 141, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 157, new DateTime(2023, 08, 27, 10, 16, 39), new DateTime(2023, 08, 27, 10, 18, 07)),
            [287] = CycleRow(287, 141, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 74, 164, new DateTime(2023, 08, 27, 10, 18, 07), new DateTime(2023, 08, 27, 10, 19, 21)),
            [288] = CycleRow(288, 141, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 88, 165, new DateTime(2023, 08, 27, 10, 19, 21), new DateTime(2023, 08, 27, 10, 20, 49)),
            [289] = CycleRow(289, 142, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 90, 140, new DateTime(2023, 08, 27, 10, 35, 21), new DateTime(2023, 08, 27, 10, 36, 51)),
            [290] = CycleRow(290, 142, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 64, 146, new DateTime(2023, 08, 27, 10, 36, 51), new DateTime(2023, 08, 27, 10, 37, 55)),
            [291] = CycleRow(291, 142, 500, CycleStatus.Started, PartStatus.Ok, 1, 50, 138, new DateTime(2023, 08, 27, 10, 37, 55), new DateTime(2023, 08, 27, 10, 38, 45)),
            [292] = CycleRow(292, 143, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 55, 123, new DateTime(2023, 08, 27, 10, 35, 49), new DateTime(2023, 08, 27, 10, 36, 44)),
            [293] = CycleRow(293, 143, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 58, 108, new DateTime(2023, 08, 27, 10, 36, 44), new DateTime(2023, 08, 27, 10, 37, 42)),
            [294] = CycleRow(294, 143, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 64, 164, new DateTime(2023, 08, 27, 10, 35, 49), new DateTime(2023, 08, 27, 10, 36, 53)),
            [295] = CycleRow(295, 144, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 66, 145, new DateTime(2023, 08, 27, 10, 43, 32), new DateTime(2023, 08, 27, 10, 44, 38)),
            [296] = CycleRow(296, 144, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 53, 119, new DateTime(2023, 08, 27, 10, 44, 38), new DateTime(2023, 08, 27, 10, 45, 31)),
            [297] = CycleRow(297, 144, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 76, 172, new DateTime(2023, 08, 27, 10, 45, 31), new DateTime(2023, 08, 27, 10, 46, 47)),
            [298] = CycleRow(298, 145, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 93, 179, new DateTime(2023, 08, 27, 10, 49, 58), new DateTime(2023, 08, 27, 10, 51, 31)),
            [299] = CycleRow(299, 145, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 82, 173, new DateTime(2023, 08, 27, 10, 51, 31), new DateTime(2023, 08, 27, 10, 52, 53)),
            [300] = CycleRow(300, 145, 500, CycleStatus.Started, PartStatus.Ok, 1, 77, 164, new DateTime(2023, 08, 27, 10, 52, 53), new DateTime(2023, 08, 27, 10, 54, 10)),
            [301] = CycleRow(301, 146, 100, CycleStatus.Started, PartStatus.Ok, 1, 97, 192, new DateTime(2023, 08, 27, 10, 50, 56), new DateTime(2023, 08, 27, 10, 52, 33)),
            [302] = CycleRow(302, 147, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 91, 150, new DateTime(2023, 08, 27, 10, 51, 25), new DateTime(2023, 08, 27, 10, 52, 56)),
            [303] = CycleRow(303, 148, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 96, 147, new DateTime(2023, 08, 27, 10, 53, 08), new DateTime(2023, 08, 27, 10, 54, 44)),
            [304] = CycleRow(304, 149, 100, CycleStatus.Started, PartStatus.Ok, 1, 57, 118, new DateTime(2023, 08, 27, 10, 53, 32), new DateTime(2023, 08, 27, 10, 54, 29)),
            [305] = CycleRow(305, 150, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 53, 125, new DateTime(2023, 08, 27, 10, 59, 49), new DateTime(2023, 08, 27, 11, 00, 42)),
            [306] = CycleRow(306, 151, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 86, 167, new DateTime(2023, 08, 27, 11, 01, 12), new DateTime(2023, 08, 27, 11, 02, 38)),
            [307] = CycleRow(307, 152, 100, CycleStatus.Started, PartStatus.Ok, 1, 52, 128, new DateTime(2023, 08, 27, 11, 01, 23), new DateTime(2023, 08, 27, 11, 02, 15)),
            [308] = CycleRow(308, 153, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 74, 138, new DateTime(2023, 08, 27, 12, 27, 18), new DateTime(2023, 08, 27, 12, 28, 32)),
            [309] = CycleRow(309, 154, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 81, 168, new DateTime(2023, 08, 27, 13, 36, 40), new DateTime(2023, 08, 27, 13, 38, 01)),
            [310] = CycleRow(310, 155, 100, CycleStatus.Started, PartStatus.Ok, 1, 50, 130, new DateTime(2023, 08, 27, 14, 01, 00), new DateTime(2023, 08, 27, 14, 01, 50)),
            [311] = CycleRow(311, 156, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 92, 163, new DateTime(2023, 08, 27, 16, 25, 12), new DateTime(2023, 08, 27, 16, 26, 44)),
            [312] = CycleRow(312, 157, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 94, 146, new DateTime(2023, 08, 27, 17, 04, 37), new DateTime(2023, 08, 27, 17, 06, 11)),
            [313] = CycleRow(313, 158, 100, CycleStatus.Started, PartStatus.Ok, 1, 92, 188, new DateTime(2023, 08, 27, 17, 16, 44), new DateTime(2023, 08, 27, 17, 18, 16)),
            [314] = CycleRow(314, 159, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 62, 122, new DateTime(2023, 08, 27, 17, 19, 30), new DateTime(2023, 08, 27, 17, 20, 32)),
            [315] = CycleRow(315, 160, 100, CycleStatus.FinishedNok, PartStatus.NOk, 1, 81, 156, new DateTime(2023, 08, 27, 15, 06, 56), new DateTime(2023, 08, 27, 15, 08, 17)),
            [316] = CycleRow(316, 161, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 60, 158, new DateTime(2023, 08, 27, 15, 44, 41), new DateTime(2023, 08, 27, 15, 45, 41)),
            [317] = CycleRow(317, 161, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 75, 141, new DateTime(2023, 08, 27, 15, 45, 41), new DateTime(2023, 08, 27, 15, 46, 56)),
            [318] = CycleRow(318, 162, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 53, 105, new DateTime(2023, 08, 27, 15, 45, 43), new DateTime(2023, 08, 27, 15, 46, 36)),
            [319] = CycleRow(319, 162, 300, CycleStatus.Started, PartStatus.Ok, 1, 85, 140, new DateTime(2023, 08, 27, 15, 46, 36), new DateTime(2023, 08, 27, 15, 48, 01)),
            [320] = CycleRow(320, 163, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 79, 147, new DateTime(2023, 08, 27, 16, 25, 05), new DateTime(2023, 08, 27, 16, 26, 24)),
            [321] = CycleRow(321, 163, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 92, 150, new DateTime(2023, 08, 27, 16, 26, 24), new DateTime(2023, 08, 27, 16, 27, 56)),
            [322] = CycleRow(322, 164, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 54, 150, new DateTime(2023, 08, 27, 09, 02, 38), new DateTime(2023, 08, 27, 09, 03, 32)),
            [323] = CycleRow(323, 164, 300, CycleStatus.Started, PartStatus.Ok, 1, 93, 184, new DateTime(2023, 08, 27, 09, 03, 32), new DateTime(2023, 08, 27, 09, 05, 05)),
            [324] = CycleRow(324, 165, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 75, 175, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 19, 29)),
            [325] = CycleRow(325, 165, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 67, 143, new DateTime(2023, 08, 27, 09, 19, 29), new DateTime(2023, 08, 27, 09, 20, 36)),
            [326] = CycleRow(326, 166, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 154, new DateTime(2023, 08, 27, 10, 58, 00), new DateTime(2023, 08, 27, 10, 59, 39)),
            [327] = CycleRow(327, 166, 300, CycleStatus.Started, PartStatus.Ok, 1, 78, 172, new DateTime(2023, 08, 27, 10, 59, 39), new DateTime(2023, 08, 27, 11, 00, 57)),
            [328] = CycleRow(328, 167, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 97, 177, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 33, 29)),
            [329] = CycleRow(329, 167, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 77, 151, new DateTime(2023, 08, 27, 15, 33, 29), new DateTime(2023, 08, 27, 15, 34, 46)),
            [330] = CycleRow(330, 168, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 92, 145, new DateTime(2023, 08, 27, 15, 46, 00), new DateTime(2023, 08, 27, 15, 47, 32)),
            [331] = CycleRow(331, 168, 300, CycleStatus.Started, PartStatus.Ok, 1, 80, 150, new DateTime(2023, 08, 27, 15, 47, 32), new DateTime(2023, 08, 27, 15, 48, 52)),
            [332] = CycleRow(332, 169, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 60, 136, new DateTime(2023, 08, 27, 10, 53, 32), new DateTime(2023, 08, 27, 10, 54, 32)),
            [333] = CycleRow(333, 169, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 75, 131, new DateTime(2023, 08, 27, 10, 54, 32), new DateTime(2023, 08, 27, 10, 55, 47)),
            [334] = CycleRow(334, 170, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 93, 158, new DateTime(2023, 08, 27, 10, 59, 49), new DateTime(2023, 08, 27, 11, 01, 22)),
            [335] = CycleRow(335, 170, 300, CycleStatus.Started, PartStatus.Ok, 1, 59, 122, new DateTime(2023, 08, 27, 11, 01, 22), new DateTime(2023, 08, 27, 11, 02, 21)),
            [336] = CycleRow(336, 171, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 180, new DateTime(2023, 08, 27, 11, 01, 12), new DateTime(2023, 08, 27, 11, 02, 36)),
            [337] = CycleRow(337, 171, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 72, 127, new DateTime(2023, 08, 27, 11, 02, 36), new DateTime(2023, 08, 27, 11, 03, 48)),
            [338] = CycleRow(338, 172, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 66, 166, new DateTime(2023, 08, 27, 11, 01, 23), new DateTime(2023, 08, 27, 11, 02, 29)),
            [339] = CycleRow(339, 172, 300, CycleStatus.Started, PartStatus.Ok, 1, 59, 127, new DateTime(2023, 08, 27, 11, 02, 29), new DateTime(2023, 08, 27, 11, 03, 28)),
            [340] = CycleRow(340, 173, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 57, 125, new DateTime(2023, 08, 27, 12, 27, 18), new DateTime(2023, 08, 27, 12, 28, 15)),
            [341] = CycleRow(341, 173, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 91, 151, new DateTime(2023, 08, 27, 12, 28, 15), new DateTime(2023, 08, 27, 12, 29, 46)),
            [342] = CycleRow(342, 174, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 72, 152, new DateTime(2023, 08, 27, 13, 36, 40), new DateTime(2023, 08, 27, 13, 37, 52)),
            [343] = CycleRow(343, 174, 300, CycleStatus.Started, PartStatus.Ok, 1, 92, 148, new DateTime(2023, 08, 27, 13, 37, 52), new DateTime(2023, 08, 27, 13, 39, 24)),
            [344] = CycleRow(344, 175, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 63, 146, new DateTime(2023, 08, 27, 14, 01, 00), new DateTime(2023, 08, 27, 14, 02, 03)),
            [345] = CycleRow(345, 175, 300, CycleStatus.FinishedNok, PartStatus.Ok, 1, 84, 184, new DateTime(2023, 08, 27, 14, 02, 03), new DateTime(2023, 08, 27, 14, 03, 27)),
            [346] = CycleRow(346, 176, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 94, 154, new DateTime(2023, 08, 27, 16, 25, 12), new DateTime(2023, 08, 27, 16, 26, 46)),
            [347] = CycleRow(347, 176, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 84, 174, new DateTime(2023, 08, 27, 16, 26, 46), new DateTime(2023, 08, 27, 16, 28, 10)),
            [348] = CycleRow(348, 176, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 74, 166, new DateTime(2023, 08, 27, 16, 25, 12), new DateTime(2023, 08, 27, 16, 26, 26)),
            [349] = CycleRow(349, 177, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 68, 135, new DateTime(2023, 08, 27, 17, 04, 37), new DateTime(2023, 08, 27, 17, 05, 45)),
            [350] = CycleRow(350, 177, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 190, new DateTime(2023, 08, 27, 17, 05, 45), new DateTime(2023, 08, 27, 17, 07, 24)),
            [351] = CycleRow(351, 177, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 81, 145, new DateTime(2023, 08, 27, 17, 07, 24), new DateTime(2023, 08, 27, 17, 08, 45)),
            [352] = CycleRow(352, 178, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 56, 121, new DateTime(2023, 08, 27, 17, 16, 44), new DateTime(2023, 08, 27, 17, 17, 40)),
            [353] = CycleRow(353, 178, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 83, 152, new DateTime(2023, 08, 27, 17, 17, 40), new DateTime(2023, 08, 27, 17, 19, 03)),
            [354] = CycleRow(354, 178, 500, CycleStatus.Started, PartStatus.Ok, 1, 79, 143, new DateTime(2023, 08, 27, 17, 19, 03), new DateTime(2023, 08, 27, 17, 20, 22)),
            [355] = CycleRow(355, 179, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 64, 157, new DateTime(2023, 08, 27, 17, 19, 30), new DateTime(2023, 08, 27, 17, 20, 34)),
            [356] = CycleRow(356, 179, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 91, 168, new DateTime(2023, 08, 27, 17, 20, 34), new DateTime(2023, 08, 27, 17, 22, 05)),
            [357] = CycleRow(357, 179, 500, CycleStatus.FinishedNok, PartStatus.NOk, 1, 86, 147, new DateTime(2023, 08, 27, 17, 19, 30), new DateTime(2023, 08, 27, 17, 20, 56)),
            [358] = CycleRow(358, 180, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 55, 135, new DateTime(2023, 08, 27, 15, 06, 56), new DateTime(2023, 08, 27, 15, 07, 51)),
            [359] = CycleRow(359, 180, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 58, 110, new DateTime(2023, 08, 27, 15, 07, 51), new DateTime(2023, 08, 27, 15, 08, 49)),
            [360] = CycleRow(360, 180, 500, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [361] = CycleRow(361, 181, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [362] = CycleRow(362, 181, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [363] = CycleRow(363, 182, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [364] = CycleRow(364, 182, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [365] = CycleRow(365, 183, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [366] = CycleRow(366, 183, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [367] = CycleRow(367, 184, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [368] = CycleRow(368, 184, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [369] = CycleRow(369, 185, 100, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28)),
            [370] = CycleRow(370, 185, 300, CycleStatus.FinishedOk, PartStatus.Ok, 1, 99, 161, new DateTime(2023, 08, 27, 15, 08, 49), new DateTime(2023, 08, 27, 15, 10, 28))
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<Cycle>> _fixtureCache =
        new(() => _cyclesDict.Values.ToList());

    /// <summary>
    /// Get all Cycle entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<Cycle> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific Cycle by ID - O(1) lookup
    /// </summary>
    public static Cycle? GetById(int id) =>
        _cyclesDict.TryGetValue(id, out var cycle) ? cycle : null;

    /// <summary>
    /// Get a specific Cycle by ID - O(1) lookup (legacy method name)
    /// </summary>
    public static Cycle? GetCycle(int id) => GetById(id);

    /// <summary>
    /// Direct dictionary access for advanced scenarios
    /// </summary>
    public static IImmutableDictionary<int, Cycle> Dictionary => _cyclesDict;

    /// <summary>
    /// Direct dictionary access (legacy property name)
    /// </summary>
    public static IImmutableDictionary<int, Cycle> Cycles => _cyclesDict;

    /// <summary>
    /// Check if a Cycle exists by ID
    /// </summary>
    public static bool Contains(int id) => _cyclesDict.ContainsKey(id);

    /// <summary>
    /// Get count of Cycles
    /// </summary>
    public static int Count => _cyclesDict.Count;

    /// <summary>
    /// Get Cycle by MachineId - O(n) operation
    /// </summary>
    public static IEnumerable<Cycle> GetByMachineId(int machineId) =>
        _cyclesDict.Values.Where(c => c.MachineId == new MachineId(machineId));

    /// <summary>
    /// Get Cycle by BarCodeId - O(n) operation
    /// </summary>
    public static Cycle? GetByBarCodeId(int barCodeId) =>
        _cyclesDict.Values.FirstOrDefault(c => c.BarCodeId.Value == barCodeId);

    /// <summary>
    /// Get Cycles by CycleStatus - O(n) operation
    /// </summary>
    public static IEnumerable<Cycle> GetByStatus(CycleStatus status) =>
        _cyclesDict.Values.Where(c => c.CycleStatus == status);

    /// <summary>
    /// Story 2.5: routes the persisted-row fixtures through the internal domain seam so the status fields
    /// (<see cref="CycleStatus"/>/<see cref="PartStatus"/>) survive Story 2.3b restricting their setters to
    /// <c>private set</c>. The status pair is seeded inside the <c>IndTrace.Domain</c> assembly via
    /// <see cref="Cycle.CreateFixture"/>; every other field is assigned here, preserving the exact original values.
    /// </summary>
    private static Cycle CycleRow(int cycleId, int barCodeId, int machineId, CycleStatus cycleStatus,
        PartStatus partStatus, int cyclesOk, int cycleTime, int taktTime, DateTime startedOn, DateTime finishedOn)
    {
        var cycle = Cycle.CreateFixture(cycleStatus, partStatus);
        cycle.CycleId = new CycleId(cycleId);
        cycle.BarCodeId = new BarCodeId(barCodeId);
        cycle.MachineId = new MachineId(machineId);
        cycle.CyclesOk = cyclesOk;
        cycle.CycleTime = cycleTime;
        cycle.TaktTime = taktTime;
        cycle.StartedOn = startedOn;
        cycle.FinishedOn = finishedOn;
        return cycle;
    }
}
