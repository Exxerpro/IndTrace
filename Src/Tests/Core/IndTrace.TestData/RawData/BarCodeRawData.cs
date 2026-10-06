// <copyright file="BarCodeRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities.BarCodes;
using IndTrace.Domain.Enum;
using IndTrace.Domain.ValueObjects;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for BarCode entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// IMPORTED: Contains all 189 entities from BarCodes.json
/// Generated on: 2025-09-03 06:01:53
/// </summary>
internal static class BarCodeRawData
{
    private static readonly ImmutableDictionary<int, BarCode> _barCodesDict =
        new Dictionary<int, BarCode>
        {
            [1] = Row(1, 508, 100, "L1AL100003232372501", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 00, 27, 24), new DateTime(2023, 08, 27, 00, 49, 12)),
            [2] = Row(2, 508, 100, "L1AL100003232372502", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 02, 54, 13), new DateTime(2023, 08, 27, 02, 54, 19)),
            [3] = Row(3, 508, 100, "L1AL100003232372503", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 07, 49, 43), new DateTime(2023, 08, 27, 08, 18, 00)),
            [4] = Row(4, 508, 100, "L1AL100003232372504", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 03, 18, 15), new DateTime(2023, 08, 27, 03, 41, 07)),
            [5] = Row(5, 508, 100, "L1AL100003232372505", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 18, 06), new DateTime(2023, 08, 27, 09, 27, 28)),
            [6] = Row(6, 508, 100, "L1AL100003232372506", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 23, 58), new DateTime(2023, 08, 27, 10, 01, 55)),
            [7] = Row(7, 508, 100, "L1AL100003232372507", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 11, 46, 55), new DateTime(2023, 08, 27, 11, 50, 57)),
            [8] = Row(8, 508, 100, "L1AL100003232372508", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 48, 11), new DateTime(2023, 08, 27, 12, 03, 04)),
            [9] = Row(9, 508, 100, "L1AL100003232372509", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 05, 30), new DateTime(2023, 08, 27, 12, 20, 08)),
            [10] = Row(10, 508, 100, "L1AL100003232372510", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 12, 19, 30), new DateTime(2023, 08, 27, 12, 19, 35)),
            [11] = Row(11, 508, 100, "L1AL100003232372511", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 08, 31, 01), new DateTime(2023, 08, 27, 09, 22, 03)),
            [12] = Row(12, 508, 100, "L1AL100003232372512", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 08, 31, 47), new DateTime(2023, 08, 27, 09, 25, 05)),
            [13] = Row(13, 508, 100, "L1AL100003232372513", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 11, 50, 42), new DateTime(2023, 08, 27, 12, 09, 01)),
            [14] = Row(14, 508, 100, "L1AL100003232372514", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 53, 28), new DateTime(2023, 08, 27, 12, 12, 01)),
            [15] = Row(15, 508, 100, "L1AL100003232372515", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 01, 49), new DateTime(2023, 08, 27, 12, 16, 03)),
            [16] = Row(16, 508, 300, "L1AL100003232372516", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 06, 30), new DateTime(2023, 08, 27, 12, 19, 31)),
            [17] = Row(17, 508, 300, "L1AL100003232372517", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 09, 13), new DateTime(2023, 08, 27, 12, 20, 56)),
            [18] = Row(18, 508, 300, "L1AL100003232372518", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 11, 01), new DateTime(2023, 08, 27, 12, 22, 36)),
            [19] = Row(19, 508, 300, "L1AL100003232372519", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 12, 15), new DateTime(2023, 08, 27, 12, 21, 23)),
            [20] = Row(20, 508, 300, "L1AL100003232372520", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 13, 21), new DateTime(2023, 08, 27, 12, 22, 16)),
            [21] = Row(21, 508, 300, "L1AL100003232372521", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 14, 53), new DateTime(2023, 08, 27, 12, 20, 02)),
            [22] = Row(22, 508, 300, "L1AL100003232372522", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 16, 43), new DateTime(2023, 08, 27, 12, 21, 58)),
            [23] = Row(23, 508, 300, "L1AL100003232372523", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 15, 45), new DateTime(2023, 08, 27, 13, 15, 45)),
            [24] = Row(24, 508, 300, "L1AL100003232372524", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 26, 29), new DateTime(2023, 08, 27, 13, 26, 29)),
            [25] = Row(25, 508, 300, "L1AL100003232372525", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 31, 55), new DateTime(2023, 08, 27, 13, 31, 55)),
            [26] = Row(26, 508, 300, "L1AL100003232372526", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 37, 13), new DateTime(2023, 08, 27, 13, 37, 13)),
            [27] = Row(27, 508, 300, "L1AL100003232372527", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 03, 53), new DateTime(2023, 08, 27, 15, 03, 53)),
            [28] = Row(28, 508, 300, "L1AL100003232372528", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 07, 23), new DateTime(2023, 08, 27, 16, 54, 58)),
            [29] = Row(29, 508, 300, "L1AL100003232372529", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 52, 09), new DateTime(2023, 08, 27, 09, 52, 09)),
            [30] = Row(30, 508, 300, "L1AL100003232372530", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 54, 09), new DateTime(2023, 08, 27, 09, 54, 09)),
            [31] = Row(31, 508, 500, "L1AL100003232372531", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 00, 17), new DateTime(2023, 08, 27, 10, 00, 17)),
            [32] = Row(32, 508, 500, "L1AL100003232372532", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 01, 14), new DateTime(2023, 08, 27, 10, 01, 14)),
            [33] = Row(33, 508, 500, "L1AL100003232372533", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 10, 01, 50), new DateTime(2023, 08, 27, 10, 01, 50)),
            [34] = Row(34, 508, 500, "L1AL100003232372534", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 03, 23), new DateTime(2023, 08, 27, 10, 03, 23)),
            [35] = Row(35, 508, 500, "L1AL100003232372535", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 05, 46), new DateTime(2023, 08, 27, 10, 05, 46)),
            [36] = Row(36, 508, 500, "L1AL100003232372536", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 10, 06, 13), new DateTime(2023, 08, 27, 10, 06, 13)),
            [37] = Row(37, 508, 500, "L1AL100003232372537", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 06, 27), new DateTime(2023, 08, 27, 10, 06, 27)),
            [38] = Row(38, 508, 500, "L1AL100003232372538", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 07, 47), new DateTime(2023, 08, 27, 10, 07, 47)),
            [39] = Row(39, 508, 500, "L1AL100003232372539", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 10, 13, 47), new DateTime(2023, 08, 27, 10, 13, 47)),
            [40] = Row(40, 508, 500, "L1AL100003232372540", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 13, 59), new DateTime(2023, 08, 27, 10, 13, 59)),
            [41] = Row(41, 508, 500, "L1AL100003232372541", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 16, 39), new DateTime(2023, 08, 27, 10, 16, 39)),
            [42] = Row(42, 508, 500, "L1AL100003232372542", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 10, 35, 21), new DateTime(2023, 08, 27, 10, 35, 21)),
            [43] = Row(43, 508, 500, "L1AL100003232372543", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 35, 49), new DateTime(2023, 08, 27, 10, 35, 49)),
            [44] = Row(44, 508, 500, "L1AL100003232372544", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 43, 32), new DateTime(2023, 08, 27, 10, 43, 32)),
            [45] = Row(45, 508, 500, "L1AL100003232372545", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 10, 49, 58), new DateTime(2023, 08, 27, 10, 49, 58)),
            [46] = Row(46, 629, 100, "L1AL90164629232372546", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 10, 50, 56), new DateTime(2023, 08, 27, 10, 50, 56)),
            [47] = Row(47, 629, 100, "L1AL90164629232372547", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 51, 25), new DateTime(2023, 08, 27, 10, 51, 25)),
            [48] = Row(48, 629, 100, "L1AL90164629232372548", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 53, 08), new DateTime(2023, 08, 27, 10, 53, 08)),
            [49] = Row(49, 629, 100, "L1AL90164629232372549", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 10, 53, 32), new DateTime(2023, 08, 27, 10, 53, 32)),
            [50] = Row(50, 629, 100, "L1AL90164629232372550", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 59, 49), new DateTime(2023, 08, 27, 10, 59, 49)),
            [51] = Row(51, 629, 100, "L1AL90164629232372551", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 01, 12), new DateTime(2023, 08, 27, 11, 01, 12)),
            [52] = Row(52, 629, 100, "L1AL90164629232372552", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 11, 01, 23), new DateTime(2023, 08, 27, 11, 01, 23)),
            [53] = Row(53, 629, 100, "L1AL90164629232372553", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 27, 18), new DateTime(2023, 08, 27, 12, 27, 18)),
            [54] = Row(54, 629, 100, "L1AL90164629232372554", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 36, 40), new DateTime(2023, 08, 27, 13, 36, 40)),
            [55] = Row(55, 629, 100, "L1AL90164629232372555", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 14, 01, 00), new DateTime(2023, 08, 27, 14, 01, 00)),
            [56] = Row(56, 629, 100, "L1AL90164629232372556", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 19, 04), new DateTime(2023, 08, 27, 16, 25, 12)),
            [57] = Row(57, 629, 100, "L1AL90164629232372557", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 04, 33), new DateTime(2023, 08, 27, 17, 04, 37)),
            [58] = Row(58, 629, 100, "L1AL90164629232372558", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 17, 08, 48), new DateTime(2023, 08, 27, 17, 16, 44)),
            [59] = Row(59, 629, 100, "L1AL90164629232372559", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 17, 18, 22), new DateTime(2023, 08, 27, 17, 19, 30)),
            [60] = Row(60, 629, 100, "L1AL90164629232372560", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [61] = Row(61, 629, 300, "L1AL90164629232372561", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 39, 12), new DateTime(2023, 08, 27, 15, 44, 41)),
            [62] = Row(62, 629, 300, "L1AL90164629232372562", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 40, 31), new DateTime(2023, 08, 27, 15, 45, 43)),
            [63] = Row(63, 629, 300, "L1AL90164629232372563", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 23, 47), new DateTime(2023, 08, 27, 16, 25, 05)),
            [64] = Row(64, 629, 300, "L1AL90164629232372564", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 00, 01), new DateTime(2023, 08, 27, 09, 02, 38)),
            [65] = Row(65, 629, 300, "L1AL90164629232372565", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 18, 14)),
            [66] = Row(66, 629, 300, "L1AL90164629232372566", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 56, 32), new DateTime(2023, 08, 27, 10, 58, 00)),
            [67] = Row(67, 629, 300, "L1AL90164629232372567", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 31, 52)),
            [68] = Row(68, 629, 300, "L1AL90164629232372568", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 37, 22), new DateTime(2023, 08, 27, 15, 46, 00)),
            [69] = Row(69, 629, 300, "L1AL90164629232372569", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 53, 32), new DateTime(2023, 08, 27, 10, 53, 32)),
            [70] = Row(70, 629, 300, "L1AL90164629232372570", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 59, 49), new DateTime(2023, 08, 27, 10, 59, 49)),
            [71] = Row(71, 629, 300, "L1AL90164629232372571", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 01, 12), new DateTime(2023, 08, 27, 11, 01, 12)),
            [72] = Row(72, 629, 300, "L1AL90164629232372572", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 01, 23), new DateTime(2023, 08, 27, 11, 01, 23)),
            [73] = Row(73, 629, 300, "L1AL90164629232372573", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 27, 18), new DateTime(2023, 08, 27, 12, 27, 18)),
            [74] = Row(74, 629, 300, "L1AL90164629232372574", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 36, 40), new DateTime(2023, 08, 27, 13, 36, 40)),
            [75] = Row(75, 629, 300, "L1AL90164629232372575", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 01, 00), new DateTime(2023, 08, 27, 14, 01, 00)),
            [76] = Row(76, 629, 500, "L1AL90164629232372576", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 19, 04), new DateTime(2023, 08, 27, 16, 25, 12)),
            [77] = Row(77, 629, 500, "L1AL90164629232372577", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 04, 33), new DateTime(2023, 08, 27, 17, 04, 37)),
            [78] = Row(78, 629, 500, "L1AL90164629232372578", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 17, 08, 48), new DateTime(2023, 08, 27, 17, 16, 44)),
            [79] = Row(79, 629, 500, "L1AL90164629232372579", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 17, 18, 22), new DateTime(2023, 08, 27, 17, 19, 30)),
            [80] = Row(80, 629, 500, "L1AL90164629232372580", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [81] = Row(81, 629, 500, "L1AL90164629232372581", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 15, 39, 12), new DateTime(2023, 08, 27, 15, 44, 41)),
            [82] = Row(82, 629, 500, "L1AL90164629232372582", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 40, 31), new DateTime(2023, 08, 27, 15, 45, 43)),
            [83] = Row(83, 629, 500, "L1AL90164629232372583", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 23, 47), new DateTime(2023, 08, 27, 16, 25, 05)),
            [84] = Row(84, 629, 500, "L1AL90164629232372584", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 09, 00, 01), new DateTime(2023, 08, 27, 09, 02, 38)),
            [85] = Row(85, 629, 500, "L1AL90164629232372585", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 18, 14)),
            [86] = Row(86, 629, 500, "L1AL90164629232372586", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 56, 32), new DateTime(2023, 08, 27, 10, 58, 00)),
            [87] = Row(87, 629, 500, "L1AL90164629232372587", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 31, 52)),
            [88] = Row(88, 629, 500, "L1AL90164629232372588", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 37, 22), new DateTime(2023, 08, 27, 15, 46, 00)),
            [89] = Row(89, 629, 500, "L1AL90164629232372589", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 31, 52)),
            [90] = Row(90, 629, 500, "L1AL90164629232372590", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 15, 37, 22), new DateTime(2023, 08, 27, 15, 46, 00)),
            [91] = Row(91, 508, 100, "L1AL90164629232372591", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 15, 39, 12), new DateTime(2023, 08, 27, 15, 44, 41)),
            [92] = Row(92, 508, 100, "L1AL90164629232372592", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 40, 31), new DateTime(2023, 08, 27, 15, 45, 43)),
            [93] = Row(93, 508, 100, "L1AL90164629232372593", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 23, 47), new DateTime(2023, 08, 27, 16, 25, 05)),
            [94] = Row(94, 508, 100, "L1AL90164629232372594", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 09, 00, 01), new DateTime(2023, 08, 27, 09, 02, 38)),
            [95] = Row(95, 508, 100, "L1AL90164629232372595", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 18, 14)),
            [96] = Row(96, 508, 100, "L1AL90164629232372596", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 56, 32), new DateTime(2023, 08, 27, 10, 58, 00)),
            [97] = Row(97, 508, 100, "L1AL90164629232372597", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 31, 52)),
            [98] = Row(98, 508, 100, "L1AL90164629232372598", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 37, 22), new DateTime(2023, 08, 27, 15, 46, 00)),
            [99] = Row(99, 508, 100, "L1AL90164629232372599", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 31, 52)),
            [100] = Row(100, 508, 100, "L1AL90164629232372600", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 15, 37, 22), new DateTime(2023, 08, 27, 15, 46, 00)),
            [101] = Row(101, 508, 100, "L1AL100003232372601", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 00, 27, 24), new DateTime(2023, 08, 27, 00, 49, 12)),
            [102] = Row(102, 508, 100, "L1AL100003232372602", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 02, 54, 13), new DateTime(2023, 08, 27, 02, 54, 19)),
            [103] = Row(103, 508, 100, "L1AL100003232372603", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 07, 49, 43), new DateTime(2023, 08, 27, 08, 18, 00)),
            [104] = Row(104, 508, 100, "L1AL100003232372604", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 03, 18, 15), new DateTime(2023, 08, 27, 03, 41, 07)),
            [105] = Row(105, 508, 100, "L1AL100003232372605", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 18, 06), new DateTime(2023, 08, 27, 09, 27, 28)),
            [106] = Row(106, 508, 300, "L1AL100003232372606", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 23, 58), new DateTime(2023, 08, 27, 10, 01, 55)),
            [107] = Row(107, 508, 300, "L1AL100003232372607", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 46, 55), new DateTime(2023, 08, 27, 11, 50, 57)),
            [108] = Row(108, 508, 300, "L1AL100003232372608", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 48, 11), new DateTime(2023, 08, 27, 12, 03, 04)),
            [109] = Row(109, 508, 300, "L1AL100003232372609", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 05, 30), new DateTime(2023, 08, 27, 12, 20, 08)),
            [110] = Row(110, 508, 300, "L1AL100003232372610", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 19, 30), new DateTime(2023, 08, 27, 12, 19, 35)),
            [111] = Row(111, 508, 300, "L1AL100003232372611", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 08, 31, 01), new DateTime(2023, 08, 27, 09, 22, 03)),
            [112] = Row(112, 508, 300, "L1AL100003232372612", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 08, 31, 47), new DateTime(2023, 08, 27, 09, 25, 05)),
            [113] = Row(113, 508, 300, "L1AL100003232372613", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 50, 42), new DateTime(2023, 08, 27, 12, 09, 01)),
            [114] = Row(114, 508, 300, "L1AL100003232372614", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 53, 28), new DateTime(2023, 08, 27, 12, 12, 01)),
            [115] = Row(115, 508, 300, "L1AL100003232372615", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 01, 49), new DateTime(2023, 08, 27, 12, 16, 03)),
            [116] = Row(116, 508, 300, "L1AL100003232372616", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 06, 30), new DateTime(2023, 08, 27, 12, 19, 31)),
            [117] = Row(117, 508, 300, "L1AL100003232372617", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 09, 13), new DateTime(2023, 08, 27, 12, 20, 56)),
            [118] = Row(118, 508, 300, "L1AL100003232372618", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 11, 01), new DateTime(2023, 08, 27, 12, 22, 36)),
            [119] = Row(119, 508, 300, "L1AL100003232372619", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 12, 15), new DateTime(2023, 08, 27, 12, 21, 23)),
            [120] = Row(120, 508, 300, "L1AL100003232372620", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 13, 21), new DateTime(2023, 08, 27, 12, 22, 16)),
            [121] = Row(121, 508, 500, "L1AL100003232372621", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 14, 53), new DateTime(2023, 08, 27, 12, 20, 02)),
            [122] = Row(122, 508, 500, "L1AL100003232372622", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 16, 43), new DateTime(2023, 08, 27, 12, 21, 58)),
            [123] = Row(123, 508, 500, "L1AL100003232372623", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 13, 15, 45), new DateTime(2023, 08, 27, 13, 15, 45)),
            [124] = Row(124, 508, 500, "L1AL100003232372624", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 26, 29), new DateTime(2023, 08, 27, 13, 26, 29)),
            [125] = Row(125, 508, 500, "L1AL100003232372625", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 31, 55), new DateTime(2023, 08, 27, 13, 31, 55)),
            [126] = Row(126, 508, 500, "L1AL100003232372626", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 13, 37, 13), new DateTime(2023, 08, 27, 13, 37, 13)),
            [127] = Row(127, 508, 500, "L1AL100003232372627", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 03, 53), new DateTime(2023, 08, 27, 15, 03, 53)),
            [128] = Row(128, 508, 500, "L1AL100003232372628", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 07, 23), new DateTime(2023, 08, 27, 16, 54, 58)),
            [129] = Row(129, 508, 500, "L1AL100003232372629", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 09, 52, 09), new DateTime(2023, 08, 27, 09, 52, 09)),
            [130] = Row(130, 508, 500, "L1AL100003232372630", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 54, 09), new DateTime(2023, 08, 27, 09, 54, 09)),
            [131] = Row(131, 508, 500, "L1AL100003232372631", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 00, 17), new DateTime(2023, 08, 27, 10, 00, 17)),
            [132] = Row(132, 508, 500, "L1AL100003232372632", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 10, 01, 14), new DateTime(2023, 08, 27, 10, 01, 14)),
            [133] = Row(133, 508, 500, "L1AL100003232372633", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 01, 50), new DateTime(2023, 08, 27, 10, 01, 50)),
            [134] = Row(134, 508, 500, "L1AL100003232372634", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 03, 23), new DateTime(2023, 08, 27, 10, 03, 23)),
            [135] = Row(135, 508, 500, "L1AL100003232372635", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 10, 05, 46), new DateTime(2023, 08, 27, 10, 05, 46)),
            [136] = Row(136, 629, 100, "L1AL100003232372636", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 10, 06, 13), new DateTime(2023, 08, 27, 10, 06, 13)),
            [137] = Row(137, 629, 100, "L1AL100003232372637", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 06, 27), new DateTime(2023, 08, 27, 10, 06, 27)),
            [138] = Row(138, 629, 100, "L1AL100003232372638", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 07, 47), new DateTime(2023, 08, 27, 10, 07, 47)),
            [139] = Row(139, 629, 100, "L1AL100003232372639", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 10, 13, 47), new DateTime(2023, 08, 27, 10, 13, 47)),
            [140] = Row(140, 629, 100, "L1AL100003232372640", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 13, 59), new DateTime(2023, 08, 27, 10, 13, 59)),
            [141] = Row(141, 629, 100, "L1AL100003232372641", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 16, 39), new DateTime(2023, 08, 27, 10, 16, 39)),
            [142] = Row(142, 629, 100, "L1AL100003232372642", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 10, 35, 21), new DateTime(2023, 08, 27, 10, 35, 21)),
            [143] = Row(143, 629, 100, "L1AL100003232372643", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 35, 49), new DateTime(2023, 08, 27, 10, 35, 49)),
            [144] = Row(144, 629, 100, "L1AL100003232372644", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 43, 32), new DateTime(2023, 08, 27, 10, 43, 32)),
            [145] = Row(145, 629, 100, "L1AL100003232372645", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 10, 49, 58), new DateTime(2023, 08, 27, 10, 49, 58)),
            [146] = Row(146, 629, 100, "L1AL90164629232372646", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 50, 56), new DateTime(2023, 08, 27, 10, 50, 56)),
            [147] = Row(147, 629, 100, "L1AL90164629232372647", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 51, 25), new DateTime(2023, 08, 27, 10, 51, 25)),
            [148] = Row(148, 629, 100, "L1AL90164629232372648", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 10, 53, 08), new DateTime(2023, 08, 27, 10, 53, 08)),
            [149] = Row(149, 629, 100, "L1AL90164629232372649", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 53, 32), new DateTime(2023, 08, 27, 10, 53, 32)),
            [150] = Row(150, 629, 100, "L1AL90164629232372650", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 59, 49), new DateTime(2023, 08, 27, 10, 59, 49)),
            [151] = Row(151, 629, 300, "L1AL90164629232372651", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 01, 12), new DateTime(2023, 08, 27, 11, 01, 12)),
            [152] = Row(152, 629, 300, "L1AL90164629232372652", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 01, 23), new DateTime(2023, 08, 27, 11, 01, 23)),
            [153] = Row(153, 629, 300, "L1AL90164629232372653", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 27, 18), new DateTime(2023, 08, 27, 12, 27, 18)),
            [154] = Row(154, 629, 300, "L1AL90164629232372654", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 13, 36, 40), new DateTime(2023, 08, 27, 13, 36, 40)),
            [155] = Row(155, 629, 300, "L1AL90164629232372655", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 01, 00), new DateTime(2023, 08, 27, 14, 01, 00)),
            [156] = Row(156, 629, 300, "L1AL90164629232372656", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 19, 04), new DateTime(2023, 08, 27, 16, 25, 12)),
            [157] = Row(157, 629, 300, "L1AL90164629232372657", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 04, 33), new DateTime(2023, 08, 27, 17, 04, 37)),
            [158] = Row(158, 629, 300, "L1AL90164629232372658", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 17, 08, 48), new DateTime(2023, 08, 27, 17, 16, 44)),
            [159] = Row(159, 629, 300, "L1AL90164629232372659", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 17, 18, 22), new DateTime(2023, 08, 27, 17, 19, 30)),
            [160] = Row(160, 629, 300, "L1AL90164629232372660", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [161] = Row(161, 629, 300, "L1AL90164629232372661", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 39, 12), new DateTime(2023, 08, 27, 15, 44, 41)),
            [162] = Row(162, 629, 300, "L1AL90164629232372662", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 40, 31), new DateTime(2023, 08, 27, 15, 45, 43)),
            [163] = Row(163, 629, 300, "L1AL90164629232372663", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 23, 47), new DateTime(2023, 08, 27, 16, 25, 05)),
            [164] = Row(164, 629, 300, "L1AL90164629232372664", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 00, 01), new DateTime(2023, 08, 27, 09, 02, 38)),
            [165] = Row(165, 629, 300, "L1AL90164629232372665", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 09, 18, 14), new DateTime(2023, 08, 27, 09, 18, 14)),
            [166] = Row(166, 629, 500, "L1AL90164629232372666", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 56, 32), new DateTime(2023, 08, 27, 10, 58, 00)),
            [167] = Row(167, 629, 500, "L1AL90164629232372667", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 15, 31, 52), new DateTime(2023, 08, 27, 15, 31, 52)),
            [168] = Row(168, 629, 500, "L1AL90164629232372668", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 15, 37, 22), new DateTime(2023, 08, 27, 15, 46, 00)),
            [169] = Row(169, 629, 500, "L1AL90164629232372669", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 53, 32), new DateTime(2023, 08, 27, 10, 53, 32)),
            [170] = Row(170, 629, 500, "L1AL90164629232372670", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 10, 59, 49), new DateTime(2023, 08, 27, 10, 59, 49)),
            [171] = Row(171, 629, 500, "L1AL90164629232372671", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 11, 01, 12), new DateTime(2023, 08, 27, 11, 01, 12)),
            [172] = Row(172, 629, 500, "L1AL90164629232372672", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 11, 01, 23), new DateTime(2023, 08, 27, 11, 01, 23)),
            [173] = Row(173, 629, 500, "L1AL90164629232372673", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 12, 27, 18), new DateTime(2023, 08, 27, 12, 27, 18)),
            [174] = Row(174, 629, 500, "L1AL90164629232372674", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 13, 36, 40), new DateTime(2023, 08, 27, 13, 36, 40)),
            [175] = Row(175, 629, 500, "L1AL90164629232372675", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 01, 00), new DateTime(2023, 08, 27, 14, 01, 00)),
            [176] = Row(176, 629, 500, "L1AL90164629232372676", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 19, 04), new DateTime(2023, 08, 27, 16, 25, 12)),
            [177] = Row(177, 629, 500, "L1AL90164629232372677", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 16, 04, 33), new DateTime(2023, 08, 27, 17, 04, 37)),
            [178] = Row(178, 629, 500, "L1AL90164629232372678", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 17, 08, 48), new DateTime(2023, 08, 27, 17, 16, 44)),
            [179] = Row(179, 629, 500, "L1AL90164629232372679", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 17, 18, 22), new DateTime(2023, 08, 27, 17, 19, 30)),
            [180] = Row(180, 629, 500, "L1AL90164629232372680", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [181] = Row(181, 508, 100, "L1AL90164629232372681", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 16, 19, 04), new DateTime(2023, 08, 27, 16, 25, 12)),
            [182] = Row(182, 508, 100, "L1AL90164629232372682", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 16, 04, 33), new DateTime(2023, 08, 27, 17, 04, 37)),
            [183] = Row(183, 508, 100, "L1AL90164629232372683", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 17, 08, 48), new DateTime(2023, 08, 27, 17, 16, 44)),
            [184] = Row(184, 508, 100, "L1AL100003232372684", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 17, 18, 22), new DateTime(2023, 08, 27, 17, 19, 30)),
            [185] = Row(185, 508, 100, "L1AL100003232372685", PartStatus.Ok, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [186] = Row(186, 629, 100, "L1AL90164629232372554", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [187] = Row(187, 629, 100, "L1AL90164629232372557", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [188] = Row(188, 629, 300, "L1AL90164629232372567", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [189] = Row(189, 629, 300, "L1AL90164629232372569", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [9996] = Row(9996, 629, 100, "L1AL90164629232379996", PartStatus.Ok, FlowStatus.Created, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [9997] = Row(9997, 629, 100, "L1AL90164629232379997", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [9998] = Row(9998, 629, 300, "L1AL90164629232379998", PartStatus.Ok, FlowStatus.Finished, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56)),
            [9999] = Row(9999, 629, 300, "L1AL90164629232379999", PartStatus.NOk, FlowStatus.InProcess, new DateTime(2023, 08, 27, 14, 22, 17), new DateTime(2023, 08, 27, 15, 06, 56))
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<BarCode>> _fixtureCache =
        new(() => _barCodesDict.Values.ToList());

    /// <summary>
    /// Get all BarCode entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<BarCode> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific BarCode by ID - O(1) lookup
    /// </summary>
    public static BarCode? GetById(int id) =>
        _barCodesDict.TryGetValue(id, out var barCode) ? barCode : null;

    /// <summary>
    /// Get a specific BarCode by ID - O(1) lookup (legacy method name)
    /// </summary>
    public static BarCode? GetBarCode(int id) => GetById(id);

    /// <summary>
    /// Direct dictionary access for advanced scenarios
    /// </summary>
    public static IImmutableDictionary<int, BarCode> Dictionary => _barCodesDict;

    /// <summary>
    /// Direct dictionary access (legacy property name)
    /// </summary>
    public static IImmutableDictionary<int, BarCode> BarCodes => _barCodesDict;

    /// <summary>
    /// Check if a BarCode exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _barCodesDict.ContainsKey(id);

    /// <summary>
    /// Get count of BarCodes - O(1) operation
    /// </summary>
    public static int Count => _barCodesDict.Count;

    /// <summary>
    /// Get BarCode by Label - O(n) operation
    /// </summary>
    public static BarCode? GetByLabel(string label) =>
        _barCodesDict.Values.FirstOrDefault(b => b.Label.Value == label);

    /// <summary>
    /// Get BarCodes by MachineId - O(n) operation
    /// </summary>
    public static IEnumerable<BarCode> GetByMachineId(int machineId) =>
        _barCodesDict.Values.Where(b => b.MachineId == new MachineId(machineId));

    /// <summary>
    /// Story 2.5: routes the persisted-row fixtures through the internal domain seam so the status fields
    /// (<see cref="FlowStatus"/>/<see cref="PartStatus"/>) survive Story 2.3b restricting their setters to
    /// <c>private set</c>. The status pair is seeded inside the <c>IndTrace.Domain</c> assembly via
    /// <see cref="BarCode.CreateFixture"/>; every other field is assigned here, preserving the exact original values.
    /// </summary>
    private static BarCode Row(int barCodeId, int productId, int machineId, string label,
        PartStatus partStatus, FlowStatus flowStatus, DateTime createdOn, DateTime modifiedOn)
    {
        var barCode = BarCode.CreateFixture(BarCodeLabel.FromPersisted(label), flowStatus, partStatus);
        barCode.BarCodeId = new BarCodeId(barCodeId);
        barCode.ProductId = new ProductId(productId);
        barCode.MachineId = new MachineId(machineId);
        barCode.Label = BarCodeLabel.FromPersisted(label);
        barCode.CreatedOn = createdOn;
        barCode.ModifiedOn = modifiedOn;
        return barCode;
    }
}
