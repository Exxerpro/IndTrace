// <copyright file="RecipeRawData.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IndTrace.Domain.Entities;

namespace IndTrace.TestData.RawData;

/// <summary>
/// Static test data for Recipe entities with O(1) lookup.
/// Generated with ImmutableDictionary for thread-safety and performance.
/// Implements lazy-loaded Dict for best of both worlds: O(1) lookups + List compatibility.
/// Contains all 234 recipes from Recipes.json for complete test coverage.
/// </summary>
internal static class RecipeRawData
{
    /// <summary>
    /// Recipe test data - complete set of 234 recipes
    /// </summary>
    private static readonly ImmutableDictionary<int, Recipe> _recipesDict =
        new Dictionary<int, Recipe>
        {
            [1] = Recipe.CreateFixture(1, 566, 100, 30, 1512000, 20, 20, 1),
            [2] = Recipe.CreateFixture(2, 566, 200, 30, 1512000, 20, 20, 1),
            [3] = Recipe.CreateFixture(3, 566, 300, 30, 1512000, 20, 20, 1),
            [4] = Recipe.CreateFixture(4, 566, 400, 30, 1512000, 20, 20, 1),
            [5] = Recipe.CreateFixture(5, 566, 500, 30, 1512000, 20, 20, 1),
            [6] = Recipe.CreateFixture(6, 566, 600, 30, 1512000, 20, 20, 1),
            [7] = Recipe.CreateFixture(7, 566, 700, 30, 1512000, 20, 20, 1),
            [8] = Recipe.CreateFixture(8, 566, 800, 30, 1512000, 20, 20, 1),
            [9] = Recipe.CreateFixture(9, 566, 900, 30, 1512000, 20, 20, 1),
            [10] = Recipe.CreateFixture(10, 581, 100, 30, 1512000, 20, 20, 1),
            [11] = Recipe.CreateFixture(11, 581, 200, 30, 1512000, 20, 20, 1),
            [12] = Recipe.CreateFixture(12, 581, 300, 30, 1512000, 20, 20, 1),
            [13] = Recipe.CreateFixture(13, 581, 400, 30, 1512000, 20, 20, 1),
            [14] = Recipe.CreateFixture(14, 581, 500, 30, 1512000, 20, 20, 1),
            [15] = Recipe.CreateFixture(15, 581, 600, 30, 1512000, 20, 20, 1),
            [16] = Recipe.CreateFixture(16, 581, 700, 30, 1512000, 20, 20, 1),
            [17] = Recipe.CreateFixture(17, 581, 800, 30, 1512000, 20, 20, 1),
            [18] = Recipe.CreateFixture(18, 581, 900, 30, 1512000, 20, 20, 1),
            [19] = Recipe.CreateFixture(19, 508, 100, 30, 1512000, 20, 20, 1),
            [20] = Recipe.CreateFixture(20, 508, 200, 30, 1512000, 20, 20, 1),
            [21] = Recipe.CreateFixture(21, 508, 300, 30, 1512000, 20, 20, 1),
            [22] = Recipe.CreateFixture(22, 508, 400, 30, 1512000, 20, 20, 1),
            [23] = Recipe.CreateFixture(23, 508, 500, 30, 1512000, 20, 20, 1),
            [24] = Recipe.CreateFixture(24, 508, 600, 30, 1512000, 20, 20, 1),
            [25] = Recipe.CreateFixture(25, 508, 700, 30, 1512000, 20, 20, 1),
            [26] = Recipe.CreateFixture(26, 508, 800, 30, 1512000, 20, 20, 1),
            [27] = Recipe.CreateFixture(27, 508, 900, 30, 1512000, 20, 20, 1),
            [28] = Recipe.CreateFixture(28, 629, 100, 30, 1512000, 20, 20, 1),
            [29] = Recipe.CreateFixture(29, 629, 200, 30, 1512000, 20, 20, 1),
            [30] = Recipe.CreateFixture(30, 629, 300, 30, 1512000, 20, 20, 1),
            [31] = Recipe.CreateFixture(31, 629, 400, 30, 1512000, 20, 20, 1),
            [32] = Recipe.CreateFixture(32, 629, 500, 30, 1512000, 20, 20, 1),
            [33] = Recipe.CreateFixture(33, 629, 600, 30, 1512000, 20, 20, 1),
            [34] = Recipe.CreateFixture(34, 629, 700, 30, 1512000, 20, 20, 1),
            [35] = Recipe.CreateFixture(35, 629, 800, 30, 1512000, 20, 20, 1),
            [36] = Recipe.CreateFixture(36, 629, 900, 30, 1512000, 20, 20, 1),
            [37] = Recipe.CreateFixture(37, 630, 100, 30, 1512000, 20, 20, 1),
            [38] = Recipe.CreateFixture(38, 630, 200, 30, 1512000, 20, 20, 1),
            [39] = Recipe.CreateFixture(39, 630, 300, 30, 1512000, 20, 20, 1),
            [40] = Recipe.CreateFixture(40, 630, 400, 30, 1512000, 20, 20, 1),
            [41] = Recipe.CreateFixture(41, 630, 500, 30, 1512000, 20, 20, 1),
            [42] = Recipe.CreateFixture(42, 630, 600, 30, 1512000, 20, 20, 1),
            [43] = Recipe.CreateFixture(43, 630, 700, 30, 1512000, 20, 20, 1),
            [44] = Recipe.CreateFixture(44, 630, 800, 30, 1512000, 20, 20, 1),
            [45] = Recipe.CreateFixture(45, 630, 900, 30, 1512000, 20, 20, 1),
            [46] = Recipe.CreateFixture(46, 631, 100, 30, 1512000, 20, 20, 1),
            [47] = Recipe.CreateFixture(47, 631, 200, 30, 1512000, 20, 20, 1),
            [48] = Recipe.CreateFixture(48, 631, 300, 30, 1512000, 20, 20, 1),
            [49] = Recipe.CreateFixture(49, 631, 400, 30, 1512000, 20, 20, 1),
            [50] = Recipe.CreateFixture(50, 631, 500, 30, 1512000, 20, 20, 1),
            [51] = Recipe.CreateFixture(51, 631, 600, 30, 1512000, 20, 20, 1),
            [52] = Recipe.CreateFixture(52, 631, 700, 30, 1512000, 20, 20, 1),
            [53] = Recipe.CreateFixture(53, 631, 800, 30, 1512000, 20, 20, 1),
            [54] = Recipe.CreateFixture(54, 631, 900, 30, 1512000, 20, 20, 1),
            [55] = Recipe.CreateFixture(55, 632, 100, 30, 1512000, 20, 20, 1),
            [56] = Recipe.CreateFixture(56, 632, 200, 30, 1512000, 20, 20, 1),
            [57] = Recipe.CreateFixture(57, 632, 300, 30, 1512000, 20, 20, 1),
            [58] = Recipe.CreateFixture(58, 632, 400, 30, 1512000, 20, 20, 1),
            [59] = Recipe.CreateFixture(59, 632, 500, 30, 1512000, 20, 20, 1),
            [60] = Recipe.CreateFixture(60, 632, 600, 30, 1512000, 20, 20, 1),
            [61] = Recipe.CreateFixture(61, 632, 700, 30, 1512000, 20, 20, 1),
            [62] = Recipe.CreateFixture(62, 632, 800, 30, 1512000, 20, 20, 1),
            [63] = Recipe.CreateFixture(63, 632, 900, 30, 1512000, 20, 20, 1),
            [64] = Recipe.CreateFixture(64, 633, 100, 30, 1512000, 20, 20, 1),
            [65] = Recipe.CreateFixture(65, 633, 200, 30, 1512000, 20, 20, 1),
            [66] = Recipe.CreateFixture(66, 633, 300, 30, 1512000, 20, 20, 1),
            [67] = Recipe.CreateFixture(67, 633, 400, 30, 1512000, 20, 20, 1),
            [68] = Recipe.CreateFixture(68, 633, 500, 30, 1512000, 20, 20, 1),
            [69] = Recipe.CreateFixture(69, 633, 600, 30, 1512000, 20, 20, 1),
            [70] = Recipe.CreateFixture(70, 633, 700, 30, 1512000, 20, 20, 1),
            [71] = Recipe.CreateFixture(71, 633, 800, 30, 1512000, 20, 20, 1),
            [72] = Recipe.CreateFixture(72, 633, 900, 30, 1512000, 20, 20, 1),
            [73] = Recipe.CreateFixture(73, 634, 100, 30, 1512000, 20, 20, 1),
            [74] = Recipe.CreateFixture(74, 634, 200, 30, 1512000, 20, 20, 1),
            [75] = Recipe.CreateFixture(75, 634, 300, 30, 1512000, 20, 20, 1),
            [76] = Recipe.CreateFixture(76, 634, 400, 30, 1512000, 20, 20, 1),
            [77] = Recipe.CreateFixture(77, 634, 500, 30, 1512000, 20, 20, 1),
            [78] = Recipe.CreateFixture(78, 634, 600, 30, 1512000, 20, 20, 1),
            [79] = Recipe.CreateFixture(79, 634, 700, 30, 1512000, 20, 20, 1),
            [80] = Recipe.CreateFixture(80, 634, 800, 30, 1512000, 20, 20, 1),
            [81] = Recipe.CreateFixture(81, 634, 900, 30, 1512000, 20, 20, 1),
            [82] = Recipe.CreateFixture(82, 635, 100, 30, 1512000, 20, 20, 1),
            [83] = Recipe.CreateFixture(83, 635, 200, 30, 1512000, 20, 20, 1),
            [84] = Recipe.CreateFixture(84, 635, 300, 30, 1512000, 20, 20, 1),
            [85] = Recipe.CreateFixture(85, 635, 400, 30, 1512000, 20, 20, 1),
            [86] = Recipe.CreateFixture(86, 635, 500, 30, 1512000, 20, 20, 1),
            [87] = Recipe.CreateFixture(87, 635, 600, 30, 1512000, 20, 20, 1),
            [88] = Recipe.CreateFixture(88, 635, 700, 30, 1512000, 20, 20, 1),
            [89] = Recipe.CreateFixture(89, 635, 800, 30, 1512000, 20, 20, 1),
            [90] = Recipe.CreateFixture(90, 635, 900, 30, 1512000, 20, 20, 1),
            [91] = Recipe.CreateFixture(91, 636, 100, 30, 1512000, 20, 20, 1),
            [92] = Recipe.CreateFixture(92, 636, 200, 30, 1512000, 20, 20, 1),
            [93] = Recipe.CreateFixture(93, 636, 300, 30, 1512000, 20, 20, 1),
            [94] = Recipe.CreateFixture(94, 636, 400, 30, 1512000, 20, 20, 1),
            [95] = Recipe.CreateFixture(95, 636, 500, 30, 1512000, 20, 20, 1),
            [96] = Recipe.CreateFixture(96, 636, 600, 30, 1512000, 20, 20, 1),
            [97] = Recipe.CreateFixture(97, 636, 700, 30, 1512000, 20, 20, 1),
            [98] = Recipe.CreateFixture(98, 636, 800, 30, 1512000, 20, 20, 1),
            [99] = Recipe.CreateFixture(99, 636, 900, 30, 1512000, 20, 20, 1),
            [100] = Recipe.CreateFixture(100, 637, 100, 30, 1512000, 20, 20, 1),
            [101] = Recipe.CreateFixture(101, 637, 200, 30, 1512000, 20, 20, 1),
            [102] = Recipe.CreateFixture(102, 637, 300, 30, 1512000, 20, 20, 1),
            [103] = Recipe.CreateFixture(103, 637, 400, 30, 1512000, 20, 20, 1),
            [104] = Recipe.CreateFixture(104, 637, 500, 30, 1512000, 20, 20, 1),
            [105] = Recipe.CreateFixture(105, 637, 600, 30, 1512000, 20, 20, 1),
            [106] = Recipe.CreateFixture(106, 637, 700, 30, 1512000, 20, 20, 1),
            [107] = Recipe.CreateFixture(107, 637, 800, 30, 1512000, 20, 20, 1),
            [108] = Recipe.CreateFixture(108, 637, 900, 30, 1512000, 20, 20, 1),
            [109] = Recipe.CreateFixture(109, 638, 100, 30, 1512000, 20, 20, 1),
            [110] = Recipe.CreateFixture(110, 638, 200, 30, 1512000, 20, 20, 1),
            [111] = Recipe.CreateFixture(111, 638, 300, 30, 1512000, 20, 20, 1),
            [112] = Recipe.CreateFixture(112, 638, 400, 30, 1512000, 20, 20, 1),
            [113] = Recipe.CreateFixture(113, 638, 500, 30, 1512000, 20, 20, 1),
            [114] = Recipe.CreateFixture(114, 638, 600, 30, 1512000, 20, 20, 1),
            [115] = Recipe.CreateFixture(115, 638, 700, 30, 1512000, 20, 20, 1),
            [116] = Recipe.CreateFixture(116, 638, 800, 30, 1512000, 20, 20, 1),
            [117] = Recipe.CreateFixture(117, 638, 900, 30, 1512000, 20, 20, 1),
            [118] = Recipe.CreateFixture(118, 639, 100, 30, 1512000, 20, 20, 1),
            [119] = Recipe.CreateFixture(119, 639, 200, 30, 1512000, 20, 20, 1),
            [120] = Recipe.CreateFixture(120, 639, 300, 30, 1512000, 20, 20, 1),
            [121] = Recipe.CreateFixture(121, 639, 400, 30, 1512000, 20, 20, 1),
            [122] = Recipe.CreateFixture(122, 639, 500, 30, 1512000, 20, 20, 1),
            [123] = Recipe.CreateFixture(123, 639, 600, 30, 1512000, 20, 20, 1),
            [124] = Recipe.CreateFixture(124, 639, 700, 30, 1512000, 20, 20, 1),
            [125] = Recipe.CreateFixture(125, 639, 800, 30, 1512000, 20, 20, 1),
            [126] = Recipe.CreateFixture(126, 639, 900, 30, 1512000, 20, 20, 1),
            [127] = Recipe.CreateFixture(127, 640, 100, 30, 1512000, 20, 20, 1),
            [128] = Recipe.CreateFixture(128, 640, 200, 30, 1512000, 20, 20, 1),
            [129] = Recipe.CreateFixture(129, 640, 300, 30, 1512000, 20, 20, 1),
            [130] = Recipe.CreateFixture(130, 640, 400, 30, 1512000, 20, 20, 1),
            [131] = Recipe.CreateFixture(131, 640, 500, 30, 1512000, 20, 20, 1),
            [132] = Recipe.CreateFixture(132, 640, 600, 30, 1512000, 20, 20, 1),
            [133] = Recipe.CreateFixture(133, 640, 700, 30, 1512000, 20, 20, 1),
            [134] = Recipe.CreateFixture(134, 640, 800, 30, 1512000, 20, 20, 1),
            [135] = Recipe.CreateFixture(135, 640, 900, 30, 1512000, 20, 20, 1),
            [136] = Recipe.CreateFixture(136, 643, 100, 30, 1512000, 20, 20, 1),
            [137] = Recipe.CreateFixture(137, 643, 200, 30, 1512000, 20, 20, 1),
            [138] = Recipe.CreateFixture(138, 643, 300, 30, 1512000, 20, 20, 1),
            [139] = Recipe.CreateFixture(139, 643, 400, 30, 1512000, 20, 20, 1),
            [140] = Recipe.CreateFixture(140, 643, 500, 30, 1512000, 20, 20, 1),
            [141] = Recipe.CreateFixture(141, 643, 600, 30, 1512000, 20, 20, 1),
            [142] = Recipe.CreateFixture(142, 643, 700, 30, 1512000, 20, 20, 1),
            [143] = Recipe.CreateFixture(143, 643, 800, 30, 1512000, 20, 20, 1),
            [144] = Recipe.CreateFixture(144, 643, 900, 30, 1512000, 20, 20, 1),
            [145] = Recipe.CreateFixture(145, 644, 100, 30, 1512000, 20, 20, 1),
            [146] = Recipe.CreateFixture(146, 644, 200, 30, 1512000, 20, 20, 1),
            [147] = Recipe.CreateFixture(147, 644, 300, 30, 1512000, 20, 20, 1),
            [148] = Recipe.CreateFixture(148, 644, 400, 30, 1512000, 20, 20, 1),
            [149] = Recipe.CreateFixture(149, 644, 500, 30, 1512000, 20, 20, 1),
            [150] = Recipe.CreateFixture(150, 644, 600, 30, 1512000, 20, 20, 1),
            [151] = Recipe.CreateFixture(151, 644, 700, 30, 1512000, 20, 20, 1),
            [152] = Recipe.CreateFixture(152, 644, 800, 30, 1512000, 20, 20, 1),
            [153] = Recipe.CreateFixture(153, 644, 900, 30, 1512000, 20, 20, 1),
            [154] = Recipe.CreateFixture(154, 645, 100, 30, 1512000, 20, 20, 1),
            [155] = Recipe.CreateFixture(155, 645, 200, 30, 1512000, 20, 20, 1),
            [156] = Recipe.CreateFixture(156, 645, 300, 30, 1512000, 20, 20, 1),
            [157] = Recipe.CreateFixture(157, 645, 400, 30, 1512000, 20, 20, 1),
            [158] = Recipe.CreateFixture(158, 645, 500, 30, 1512000, 20, 20, 1),
            [159] = Recipe.CreateFixture(159, 645, 600, 30, 1512000, 20, 20, 1),
            [160] = Recipe.CreateFixture(160, 645, 700, 30, 1512000, 20, 20, 1),
            [161] = Recipe.CreateFixture(161, 645, 800, 30, 1512000, 20, 20, 1),
            [162] = Recipe.CreateFixture(162, 645, 900, 30, 1512000, 20, 20, 1),
            [163] = Recipe.CreateFixture(163, 646, 100, 30, 1512000, 20, 20, 1),
            [164] = Recipe.CreateFixture(164, 646, 200, 30, 1512000, 20, 20, 1),
            [165] = Recipe.CreateFixture(165, 646, 300, 30, 1512000, 20, 20, 1),
            [166] = Recipe.CreateFixture(166, 646, 400, 30, 1512000, 20, 20, 1),
            [167] = Recipe.CreateFixture(167, 646, 500, 30, 1512000, 20, 20, 1),
            [168] = Recipe.CreateFixture(168, 646, 600, 30, 1512000, 20, 20, 1),
            [169] = Recipe.CreateFixture(169, 646, 700, 30, 1512000, 20, 20, 1),
            [170] = Recipe.CreateFixture(170, 646, 800, 30, 1512000, 20, 20, 1),
            [171] = Recipe.CreateFixture(171, 646, 900, 30, 1512000, 20, 20, 1),
            [172] = Recipe.CreateFixture(172, 647, 100, 30, 1512000, 20, 20, 1),
            [173] = Recipe.CreateFixture(173, 647, 200, 30, 1512000, 20, 20, 1),
            [174] = Recipe.CreateFixture(174, 647, 300, 30, 1512000, 20, 20, 1),
            [175] = Recipe.CreateFixture(175, 647, 400, 30, 1512000, 20, 20, 1),
            [176] = Recipe.CreateFixture(176, 647, 500, 30, 1512000, 20, 20, 1),
            [177] = Recipe.CreateFixture(177, 647, 600, 30, 1512000, 20, 20, 1),
            [178] = Recipe.CreateFixture(178, 647, 700, 30, 1512000, 20, 20, 1),
            [179] = Recipe.CreateFixture(179, 647, 800, 30, 1512000, 20, 20, 1),
            [180] = Recipe.CreateFixture(180, 647, 900, 30, 1512000, 20, 20, 1),
            [181] = Recipe.CreateFixture(181, 648, 100, 30, 1512000, 20, 20, 1),
            [182] = Recipe.CreateFixture(182, 648, 200, 30, 1512000, 20, 20, 1),
            [183] = Recipe.CreateFixture(183, 648, 300, 30, 1512000, 20, 20, 1),
            [184] = Recipe.CreateFixture(184, 648, 400, 30, 1512000, 20, 20, 1),
            [185] = Recipe.CreateFixture(185, 648, 500, 30, 1512000, 20, 20, 1),
            [186] = Recipe.CreateFixture(186, 648, 600, 30, 1512000, 20, 20, 1),
            [187] = Recipe.CreateFixture(187, 648, 700, 30, 1512000, 20, 20, 1),
            [188] = Recipe.CreateFixture(188, 648, 800, 30, 1512000, 20, 20, 1),
            [189] = Recipe.CreateFixture(189, 648, 900, 30, 1512000, 20, 20, 1),
            [190] = Recipe.CreateFixture(190, 649, 100, 30, 1512000, 20, 20, 1),
            [191] = Recipe.CreateFixture(191, 649, 200, 30, 1512000, 20, 20, 1),
            [192] = Recipe.CreateFixture(192, 649, 300, 30, 1512000, 20, 20, 1),
            [193] = Recipe.CreateFixture(193, 649, 400, 30, 1512000, 20, 20, 1),
            [194] = Recipe.CreateFixture(194, 649, 500, 30, 1512000, 20, 20, 1),
            [195] = Recipe.CreateFixture(195, 649, 600, 30, 1512000, 20, 20, 1),
            [196] = Recipe.CreateFixture(196, 649, 700, 30, 1512000, 20, 20, 1),
            [197] = Recipe.CreateFixture(197, 649, 800, 30, 1512000, 20, 20, 1),
            [198] = Recipe.CreateFixture(198, 649, 900, 30, 1512000, 20, 20, 1),
            [199] = Recipe.CreateFixture(199, 650, 100, 30, 1512000, 20, 20, 1),
            [200] = Recipe.CreateFixture(200, 650, 200, 30, 1512000, 20, 20, 1),
            [201] = Recipe.CreateFixture(201, 650, 300, 30, 1512000, 20, 20, 1),
            [202] = Recipe.CreateFixture(202, 650, 400, 30, 1512000, 20, 20, 1),
            [203] = Recipe.CreateFixture(203, 650, 500, 30, 1512000, 20, 20, 1),
            [204] = Recipe.CreateFixture(204, 650, 600, 30, 1512000, 20, 20, 1),
            [205] = Recipe.CreateFixture(205, 650, 700, 30, 1512000, 20, 20, 1),
            [206] = Recipe.CreateFixture(206, 650, 800, 30, 1512000, 20, 20, 1),
            [207] = Recipe.CreateFixture(207, 650, 900, 30, 1512000, 20, 20, 1),
            [208] = Recipe.CreateFixture(208, 651, 100, 30, 1512000, 20, 20, 1),
            [209] = Recipe.CreateFixture(209, 651, 200, 30, 1512000, 20, 20, 1),
            [210] = Recipe.CreateFixture(210, 651, 300, 30, 1512000, 20, 20, 1),
            [211] = Recipe.CreateFixture(211, 651, 400, 30, 1512000, 20, 20, 1),
            [212] = Recipe.CreateFixture(212, 651, 500, 30, 1512000, 20, 20, 1),
            [213] = Recipe.CreateFixture(213, 651, 600, 30, 1512000, 20, 20, 1),
            [214] = Recipe.CreateFixture(214, 651, 700, 30, 1512000, 20, 20, 1),
            [215] = Recipe.CreateFixture(215, 651, 800, 30, 1512000, 20, 20, 1),
            [216] = Recipe.CreateFixture(216, 651, 900, 30, 1512000, 20, 20, 1),
            [217] = Recipe.CreateFixture(217, 652, 100, 30, 1512000, 20, 20, 1),
            [218] = Recipe.CreateFixture(218, 652, 200, 30, 1512000, 20, 20, 1),
            [219] = Recipe.CreateFixture(219, 652, 300, 30, 1512000, 20, 20, 1),
            [220] = Recipe.CreateFixture(220, 652, 400, 30, 1512000, 20, 20, 1),
            [221] = Recipe.CreateFixture(221, 652, 500, 30, 1512000, 20, 20, 1),
            [222] = Recipe.CreateFixture(222, 652, 600, 30, 1512000, 20, 20, 1),
            [223] = Recipe.CreateFixture(223, 652, 700, 30, 1512000, 20, 20, 1),
            [224] = Recipe.CreateFixture(224, 652, 800, 30, 1512000, 20, 20, 1),
            [225] = Recipe.CreateFixture(225, 652, 900, 30, 1512000, 20, 20, 1),
            [226] = Recipe.CreateFixture(226, 653, 100, 30, 1512000, 20, 20, 1),
            [227] = Recipe.CreateFixture(227, 653, 200, 30, 1512000, 20, 20, 1),
            [228] = Recipe.CreateFixture(228, 653, 300, 30, 1512000, 20, 20, 1),
            [229] = Recipe.CreateFixture(229, 653, 400, 30, 1512000, 20, 20, 1),
            [230] = Recipe.CreateFixture(230, 653, 500, 30, 1512000, 20, 20, 1),
            [231] = Recipe.CreateFixture(231, 653, 600, 30, 1512000, 20, 20, 1),
            [232] = Recipe.CreateFixture(232, 653, 700, 30, 1512000, 20, 20, 1),
            [233] = Recipe.CreateFixture(233, 653, 800, 30, 1512000, 20, 20, 1),
            [234] = Recipe.CreateFixture(234, 653, 900, 30, 1512000, 20, 20, 1)
        }.ToImmutableDictionary();

    /// <summary>
    /// Lazy-loaded cached list for maximum performance - best of both worlds
    /// </summary>
    private static readonly Lazy<IReadOnlyList<Recipe>> _fixtureCache =
        new(() => _recipesDict.Values.ToList());

    /// <summary>
    /// Get all Recipe entities (cached List from dictionary for backward compatibility)
    /// </summary>
    public static IReadOnlyList<Recipe> Fixture => _fixtureCache.Value;

    /// <summary>
    /// Get a specific Recipe by ID - O(1) lookup (standardized pattern)
    /// </summary>
    public static Recipe? GetById(int id) =>
        _recipesDict.TryGetValue(id, out var recipe) ? recipe : null;

    /// <summary>
    /// Direct dictionary access for advanced scenarios (standardized pattern)
    /// </summary>
    public static IImmutableDictionary<int, Recipe> Dictionary => _recipesDict;

    /// <summary>
    /// Check if a Recipe exists by ID - O(1) lookup
    /// </summary>
    public static bool Contains(int id) => _recipesDict.ContainsKey(id);

    /// <summary>
    /// Get count of Recipes - O(1) operation
    /// </summary>
    public static int Count => _recipesDict.Count;
}
