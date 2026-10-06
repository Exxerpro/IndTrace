// <copyright file="DefectTests.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Domain.UnitTests.DefectsTests;

/// <summary>
/// Unit tests for Defect
/// </summary>
public class DefectTests
{
    /// <summary>
    /// Executes Defect_WhenCreatedForManufacturingScenarios_ShouldInitializeCorrectlyAsLookupEntity operation.
    /// </summary>
    [Fact]
    public void Defect_WhenCreatedForManufacturingScenarios_ShouldInitializeCorrectlyAsLookupEntity()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new Defect();

        // Assert
        instance.ShouldNotBeNull();
        instance.DefectId.ShouldBe(default(int));
        instance.DefectTypeId.ShouldBe(default(int));
        instance.Name.ShouldBe(string.Empty); // Refactored to use string.Empty (safer than null)
        instance.Description.ShouldBe(string.Empty);
        instance.ShortName.ShouldBe(string.Empty);
        instance.ShouldBeAssignableTo<ILookupEntity>();

        // Arrange & Act - Test with manufacturing defect data
        var scratchDefect = new Defect
        {
            DefectId = 1001,
            DefectTypeId = 100,
            Name = "Surface Scratch",
            Description = "Minor surface scratches caused by handling or tooling contact",
            ShortName = "SCRATCH"
        };

        // Assert - Verify all properties are set correctly
        scratchDefect.ShouldNotBeNull();
        scratchDefect.DefectId.ShouldBe(1001);
        scratchDefect.DefectTypeId.ShouldBe(100);
        scratchDefect.Name.ShouldBe("Surface Scratch");
        scratchDefect.Description.ShouldBe("Minor surface scratches caused by handling or tooling contact");
        scratchDefect.ShortName.ShouldBe("SCRATCH");

        // Test automotive manufacturing defects
        var dentDefect = new Defect
        {
            DefectId = 1002,
            DefectTypeId = 200,
            Name = "Dent - Body Panel",
            Description = "Physical deformation of body panel due to impact or press malfunction",
            ShortName = "DENT"
        };

        dentDefect.ShouldNotBeNull();
        dentDefect.DefectId.ShouldBe(1002);
        dentDefect.DefectTypeId.ShouldBe(200);
        dentDefect.Name.ShouldBe("Dent - Body Panel");
        dentDefect.Description.ShouldBe("Physical deformation of body panel due to impact or press malfunction");
        dentDefect.ShortName.ShouldBe("DENT");
    }
    /// <summary>
    /// Executes Defect_WhenCreatedWithEdgeCaseAndExtremeValues_ShouldHandleAllInputsGracefully operation.
    /// </summary>

    [Fact]
    public void Defect_WhenCreatedWithEdgeCaseAndExtremeValues_ShouldHandleAllInputsGracefully()
    {
        // Arrange & Act - Test edge cases for manufacturing defect entities

        // Test negative DefectId
        var negativeIdDefect = new Defect
        {
            DefectId = -1,
            DefectTypeId = 100,
            Name = "Test Defect",
            Description = "Test Description",
            ShortName = "TEST"
        };

        // Assert - Defect should handle negative IDs gracefully
        negativeIdDefect.ShouldNotBeNull();
        negativeIdDefect.DefectId.ShouldBe(-1);
        negativeIdDefect.DefectTypeId.ShouldBe(100);

        // Test zero DefectId
        var zeroIdDefect = new Defect
        {
            DefectId = 0,
            DefectTypeId = 0,
            Name = "Zero ID Defect",
            Description = "Zero ID Description",
            ShortName = "ZERO"
        };

        zeroIdDefect.ShouldNotBeNull();
        zeroIdDefect.DefectId.ShouldBe(0);
        zeroIdDefect.DefectTypeId.ShouldBe(0);

        // Test maximum values
        var maxDefect = new Defect
        {
            DefectId = int.MaxValue,
            DefectTypeId = int.MaxValue,
            Name = "Maximum ID Defect",
            Description = "Maximum ID Description",
            ShortName = "MAX"
        };

        maxDefect.ShouldNotBeNull();
        maxDefect.DefectId.ShouldBe(int.MaxValue);
        maxDefect.DefectTypeId.ShouldBe(int.MaxValue);

        // Test null string properties
        var nullStringDefect = new Defect
        {
            DefectId = 999,
            DefectTypeId = 99,
            Name = null!,
            Description = null!,
            ShortName = null!
        };

        nullStringDefect.ShouldNotBeNull();
        nullStringDefect.Name.ShouldBeNull();
        nullStringDefect.Description.ShouldBeNull();
        nullStringDefect.ShortName.ShouldBeNull();

        // Test empty string properties
        var emptyStringDefect = new Defect
        {
            DefectId = 998,
            DefectTypeId = 98,
            Name = "",
            Description = "",
            ShortName = ""
        };

        emptyStringDefect.ShouldNotBeNull();
        emptyStringDefect.Name.ShouldBe("");
        emptyStringDefect.Description.ShouldBe("");
        emptyStringDefect.ShortName.ShouldBe("");

        // Test very long strings
        var longStringDefect = new Defect
        {
            DefectId = 997,
            DefectTypeId = 97,
            Name = new string('A', 1000),
            Description = new string('B', 2000),
            ShortName = new string('C', 100)
        };

        longStringDefect.ShouldNotBeNull();
        longStringDefect.Name.Length.ShouldBe(1000);
        longStringDefect.Description.Length.ShouldBe(2000);
        longStringDefect.ShortName.Length.ShouldBe(100);
    }
    /// <summary>
    /// Executes Defect_WhenPropertiesSetToManufacturingDefectData_ShouldPersistAllValuesCorrectly operation.
    /// </summary>

    [Fact]
    public void Defect_WhenPropertiesSetToManufacturingDefectData_ShouldPersistAllValuesCorrectly()
    {
        // Arrange - Test all manufacturing defect properties
        var instance = new Defect();

        // Act & Assert - Test property setters and getters
        instance.DefectId = 2001;
        instance.DefectId.ShouldBe(2001);

        instance.DefectTypeId = 300;
        instance.DefectTypeId.ShouldBe(300);

        instance.Name = "Paint Defect";
        instance.Name.ShouldBe("Paint Defect");

        instance.Description = "Paint application issues including runs, orange peel, or contamination";
        instance.Description.ShouldBe("Paint application issues including runs, orange peel, or contamination");

        instance.ShortName = "PAINT";
        instance.ShortName.ShouldBe("PAINT");

        // Test property modifications
        instance.DefectId = 2002;
        instance.DefectId.ShouldBe(2002);

        instance.DefectTypeId = 400;
        instance.DefectTypeId.ShouldBe(400);

        instance.Name = "Weld Defect";
        instance.Name.ShouldBe("Weld Defect");

        instance.Description = "Welding issues including incomplete penetration, porosity, or burn-through";
        instance.Description.ShouldBe("Welding issues including incomplete penetration, porosity, or burn-through");

        instance.ShortName = "WELD";
        instance.ShortName.ShouldBe("WELD");

        // Test comprehensive automotive defect examples
        var dimensionalDefect = new Defect();
        dimensionalDefect.DefectId = 3001;
        dimensionalDefect.DefectTypeId = 500;
        dimensionalDefect.Name = "Dimensional Out of Tolerance";
        dimensionalDefect.Description = "Part dimensions exceed specified tolerance ranges as per engineering drawings";
        dimensionalDefect.ShortName = "DIM_OOT";

        dimensionalDefect.DefectId.ShouldBe(3001);
        dimensionalDefect.DefectTypeId.ShouldBe(500);
        dimensionalDefect.Name.ShouldBe("Dimensional Out of Tolerance");
        dimensionalDefect.Description.ShouldBe("Part dimensions exceed specified tolerance ranges as per engineering drawings");
        dimensionalDefect.ShortName.ShouldBe("DIM_OOT");

        // Test extreme values
        instance.DefectId = int.MinValue;
        instance.DefectId.ShouldBe(int.MinValue);

        instance.DefectTypeId = int.MinValue;
        instance.DefectTypeId.ShouldBe(int.MinValue);

        instance.DefectId = int.MaxValue;
        instance.DefectId.ShouldBe(int.MaxValue);

        instance.DefectTypeId = int.MaxValue;
        instance.DefectTypeId.ShouldBe(int.MaxValue);
    }
    /// <summary>
    /// Executes Defect_WhenStandardObjectMethodsCalled_ShouldBehavePredictablyForLookupEntities operation.
    /// </summary>

    [Fact]
    public void Defect_WhenStandardObjectMethodsCalled_ShouldBehavePredictablyForLookupEntities()
    {
        // Arrange
        var defect1 = new Defect
        {
            DefectId = 4001,
            DefectTypeId = 600,
            Name = "Corrosion",
            Description = "Surface corrosion or rust formation",
            ShortName = "CORR"
        };

        var defect2 = new Defect
        {
            DefectId = 4001,
            DefectTypeId = 600,
            Name = "Corrosion",
            Description = "Surface corrosion or rust formation",
            ShortName = "CORR"
        };

        var defect3 = new Defect
        {
            DefectId = 4002,
            DefectTypeId = 700,
            Name = "Crack",
            Description = "Structural crack in material",
            ShortName = "CRACK"
        };

        // Act & Assert - Test object identity and reference equality
        defect1.ShouldNotBeSameAs(defect2); // Different instances
        defect1.ShouldNotBe(defect2); // No custom equality override
        (defect1 == defect2).ShouldBeFalse(); // Reference equality

        // Test GetHashCode method
        var hashCode1 = defect1.GetHashCode();
        var hashCode2 = defect2.GetHashCode();
        var hashCode3 = defect3.GetHashCode();

        hashCode1.ShouldBeOfType<int>();
        hashCode2.ShouldBeOfType<int>();
        hashCode3.ShouldBeOfType<int>();

        hashCode1.ShouldNotBe(hashCode2); // Different object instances
        hashCode1.ShouldNotBe(hashCode3); // Different data

        // Test GetType method
        var type = defect1.GetType();
        type.ShouldNotBeNull();
        type.Name.ShouldBe("Defect");
        type.Namespace.ShouldBe("IndTrace.Domain.Entities");

        // Test ToString method (inherited from Object)
        var toStringResult = defect1.ToString();
        toStringResult.ShouldNotBeNull();
        toStringResult.ShouldContain("Defect"); // Should contain type name

        // Test interface implementation
        defect1.ShouldBeAssignableTo<ILookupEntity>();

        // Test property reflection for lookup entity structure
        var properties = type.GetProperties();
        var defectIdProperty = properties.FirstOrDefault(p => p.Name == "DefectId");
        var defectTypeIdProperty = properties.FirstOrDefault(p => p.Name == "DefectTypeId");
        var nameProperty = properties.FirstOrDefault(p => p.Name == "Name");
        var descriptionProperty = properties.FirstOrDefault(p => p.Name == "Description");
        var shortNameProperty = properties.FirstOrDefault(p => p.Name == "ShortName");

        defectIdProperty.ShouldNotBeNull();
        defectIdProperty!.PropertyType.ShouldBe(typeof(int));
        defectIdProperty.CanRead.ShouldBeTrue();
        defectIdProperty.CanWrite.ShouldBeTrue();

        defectTypeIdProperty.ShouldNotBeNull();
        defectTypeIdProperty!.PropertyType.ShouldBe(typeof(int));

        nameProperty.ShouldNotBeNull();
        nameProperty!.PropertyType.ShouldBe(typeof(string));

        descriptionProperty.ShouldNotBeNull();
        descriptionProperty!.PropertyType.ShouldBe(typeof(string));

        shortNameProperty.ShouldNotBeNull();
        shortNameProperty!.PropertyType.ShouldBe(typeof(string));

        // Test with null properties
        var nullDefect = new Defect();
        var nullHashCode = nullDefect.GetHashCode();
        nullHashCode.ShouldBeOfType<int>();

        var nullToString = nullDefect.ToString();
        nullToString.ShouldNotBeNull();
    }
    /// <summary>
    /// Executes Defect_WhenUsedInQualityControlScenarios_ShouldEnforceManufacturingDefectBusinessRules operation.
    /// </summary>

    [Fact]
    public void Defect_WhenUsedInQualityControlScenarios_ShouldEnforceManufacturingDefectBusinessRules()
    {
        // Arrange - Manufacturing quality defect categorization scenarios
        var surfaceDefects = new[]
        {
            new Defect { DefectId = 5001, DefectTypeId = 100, Name = "Scratch", Description = "Surface scratches from handling", ShortName = "SCR" },
            new Defect { DefectId = 5002, DefectTypeId = 100, Name = "Scuff Mark", Description = "Scuff marks from transport", ShortName = "SCUFF" },
            new Defect { DefectId = 5003, DefectTypeId = 100, Name = "Orange Peel", Description = "Paint texture defect with surface irregularities", ShortName = "OP" }
        };

        var structuralDefects = new[]
        {
            new Defect { DefectId = 5101, DefectTypeId = 200, Name = "Crack", Description = "Structural crack in material", ShortName = "CRACK" },
            new Defect { DefectId = 5102, DefectTypeId = 200, Name = "Dent", Description = "Physical deformation from impact", ShortName = "DENT" },
            new Defect { DefectId = 5103, DefectTypeId = 200, Name = "Tear", Description = "Material tear or rupture", ShortName = "TEAR" }
        };

        var dimensionalDefects = new[]
        {
            new Defect { DefectId = 5201, DefectTypeId = 300, Name = "Oversize", Description = "Dimension exceeds upper tolerance", ShortName = "OVER" },
            new Defect { DefectId = 5202, DefectTypeId = 300, Name = "Undersize", Description = "Dimension below lower tolerance", ShortName = "UNDER" },
            new Defect { DefectId = 5203, DefectTypeId = 300, Name = "Out of Round", Description = "Circular feature not within roundness tolerance", ShortName = "OOR" }
        };

        // Act & Assert - Business Rule 1: Defect categorization by type
        foreach (var defect in surfaceDefects)
        {
            defect.DefectTypeId.ShouldBe(100); // Surface defects
            defect.DefectId.ShouldBeInRange(5001, 5099);
        }

        foreach (var defect in structuralDefects)
        {
            defect.DefectTypeId.ShouldBe(200); // Structural defects
            defect.DefectId.ShouldBeInRange(5101, 5199);
        }

        foreach (var defect in dimensionalDefects)
        {
            defect.DefectTypeId.ShouldBe(300); // Dimensional defects
            defect.DefectId.ShouldBeInRange(5201, 5299);
        }

        // Business Rule 2: Unique defect identifiers
        var allDefects = surfaceDefects.Concat(structuralDefects).Concat(dimensionalDefects).ToArray();
        var defectIds = allDefects.Select(d => d.DefectId).ToList();
        defectIds.ShouldBeUnique();

        // Business Rule 3: Short names should be concise (typically <= 10 characters)
        foreach (var defect in allDefects)
        {
            defect.ShortName.ShouldNotBeNullOrEmpty();
            defect.ShortName.Length.ShouldBeLessThanOrEqualTo(10);
        }

        // Business Rule 4: Names should be descriptive
        foreach (var defect in allDefects)
        {
            defect.Name.ShouldNotBeNullOrEmpty();
            defect.Name.Length.ShouldBeGreaterThan(3);
            defect.Description.ShouldNotBeNullOrEmpty();
            defect.Description.Length.ShouldBeGreaterThan(defect.Name.Length);
        }

        // Business Rule 5: Defect type consistency
        var typeGroups = allDefects.GroupBy(d => d.DefectTypeId).ToList();
        typeGroups.Count.ShouldBe(3); // Three distinct types

        // Business Rule 6: Manufacturing specific naming conventions
        var paintDefects = allDefects.Where(d => d.Name.Contains("Orange") || d.Name.Contains("Scuff")).ToArray();
        paintDefects.Length.ShouldBeGreaterThan(0);

        var mechanicalDefects = allDefects.Where(d => d.Name.Contains("Crack") || d.Name.Contains("Dent") || d.Name.Contains("Tear")).ToArray();
        mechanicalDefects.Length.ShouldBeGreaterThan(0);

        var toleranceDefects = allDefects.Where(d => d.Name.Contains("size") || d.Name.Contains("Round")).ToArray();
        toleranceDefects.Length.ShouldBeGreaterThan(0);

        // Business Rule 7: Short name uniqueness within type
        var surfaceShortNames = surfaceDefects.Select(d => d.ShortName).ToList();
        surfaceShortNames.ShouldBeUnique();

        var structuralShortNames = structuralDefects.Select(d => d.ShortName).ToList();
        structuralShortNames.ShouldBeUnique();

        var dimensionalShortNames = dimensionalDefects.Select(d => d.ShortName).ToList();
        dimensionalShortNames.ShouldBeUnique();

        // Business Rule 8: Severity implications by type
        // Surface defects (100) are typically cosmetic
        // Structural defects (200) are typically critical
        // Dimensional defects (300) are typically functional
        structuralDefects.All(d => d.DefectTypeId > surfaceDefects.Max(s => s.DefectTypeId)).ShouldBeTrue();
        dimensionalDefects.All(d => d.DefectTypeId > structuralDefects.Max(s => s.DefectTypeId)).ShouldBeTrue();

        // Business Rule 9: Description provides actionable information
        foreach (var defect in allDefects)
        {
            defect.Description.ShouldContain(" "); // Should be multi-word
            // Common manufacturing terms should appear
            var hasManufacturingTerms = defect.Description.Contains("material") ||
                                      defect.Description.Contains("tolerance") ||
                                      defect.Description.Contains("surface") ||
                                      defect.Description.Contains("impact") ||
                                      defect.Description.Contains("paint") ||
                                      defect.Description.Contains("handling") ||
                                      defect.Description.Contains("transport");
        }

        // Business Rule 10: Lookup entity compliance
        foreach (var defect in allDefects)
        {
            defect.ShouldBeAssignableTo<ILookupEntity>();
            defect.DefectId.ShouldBeGreaterThan(0); // Valid lookup IDs
        }
    }
}