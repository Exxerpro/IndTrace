// <copyright file="ResilientJsonConverters.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.TestData.Converters;

/// <summary>
/// Factory for creating resilient enum converters.
/// </summary>
internal static class ResilientJsonConverters
{
    /// <summary>
    /// Creates JSON serializer options with resilient enum handling.
    /// </summary>
    public static JsonSerializerOptions CreateResilientOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        // #39: Register is locked down to factory-only construction, so System.Text.Json cannot materialize it
        // through the default ctor/setter path — route it through the CreateFixture-based converter.
        options.Converters.Add(new ResilientRegisterConverter());

        // Story 35.D2 (#35): BarCode.BarCodeId (and every future IIntId struct) authors the id as a bare number
        // (e.g. "BarCodeId": 555), which STJ cannot materialize into the struct without a converter. One factory
        // serves all strongly-typed ids as bare ints, byte-identical to the retired per-id BarCodeIdJsonConverter.
        options.Converters.Add(new IndTrace.Domain.ValueObjects.StronglyTypedIdJsonConverterFactory());

        // Story 27.2b-2 (#27/F4): BarCode.Label was retyped from string to the BarCodeLabel value object (private
        // ctor, no parameterless ctor), which STJ cannot reconstruct from the bare string the embedded JSON authors
        // (e.g. "Label": "L1AL..."). Registered so the loader reads the label back via the total FromPersisted seam.
        options.Converters.Add(new IndTrace.Domain.ValueObjects.BarCodeLabelJsonConverter());

        // Add resilient converters for common domain enums
        try
        {
            // Dynamically add converters for enum types in the Domain assembly
            var domainAssembly = typeof(IndTrace.Domain.Entities.Machine).Assembly;

            // Add converters for regular .NET enums
            var enumTypes = domainAssembly.GetTypes()
                .Where(t => t.IsEnum)
                .ToList();

            foreach (var enumType in enumTypes)
            {
                var converterType = typeof(ResilientEnumConverter<>).MakeGenericType(enumType);
                var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
                options.Converters.Add(converter);
            }

            //[Fix] CLAUDE - Date: 26/08/2025 
            //Reason: Add resilient converters for smart enums (EnumModel-based classes) 
            // Smart enums like MachineType need special handling as they are classes, not structs

            // Add converters for smart enums (EnumModel-based classes)
            var smartEnumTypes = domainAssembly.GetTypes()
                .Where(t => t.IsClass && t.IsSubclassOf(typeof(EnumModel)) && !t.IsAbstract)
                .ToList();

            foreach (var smartEnumType in smartEnumTypes)
            {
                var converterType = typeof(ResilientSmartEnumConverter<>).MakeGenericType(smartEnumType);
                var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
                options.Converters.Add(converter);
            }

            System.Diagnostics.Debug.WriteLine($"Added resilient converters for {enumTypes.Count} regular enums and {smartEnumTypes.Count} smart enums");
        }
        catch (Exception ex)
        {
            // If reflection fails, just log and continue with basic options
            System.Diagnostics.Debug.WriteLine($"Failed to add resilient enum converters: {ex.Message}");
        }

        return options;
    }
}
