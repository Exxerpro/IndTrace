// <copyright file="ReflectionHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.UI.Models.Products;

using System.Reflection;
using MudBlazor;

/// <summary>
/// Provides reflection utilities for working with MudBlazor drop container components.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Gets the drop zones from a MudDropContainer using reflection to access private fields.
    /// </summary>
    /// <typeparam name="T">The type of items in the drop zones.</typeparam>
    /// <param name="container">The MudDropContainer instance.</param>
    /// <returns>A dictionary mapping drop zone identifiers to drop zone instances.</returns>
    /// <exception cref="ArgumentException">Thrown when the expected field is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the field is not of the expected type.</exception>
    public static Dictionary<string, MudDropZone<T>> GetDropZones<T>(this MudDropContainer<ProductMachineItem> container)
        where T : notnull
    {
        var fieldName = "_mudDropZones";

        // Get the type of the container object
        Type containerType = container.GetType();

        // Get the FieldInfo for the private field
        FieldInfo? fieldInfo = containerType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        // Check if the field was found
        if (fieldInfo == null)
        {
            throw new ArgumentException($"Field '{fieldName}' not found in type '{containerType.FullName}'.");
        }

        // Get the value of the field
        var fieldValue = fieldInfo.GetValue(container);

        // Cast the field value to the expected type
        if (fieldValue is Dictionary<string, MudDropZone<T>> dictionary)
        {
            return dictionary;
        }
        else
        {
            throw new InvalidCastException($"Field '{fieldName}' is not of the expected type 'Dictionary<string, MudDropZone<{typeof(T).Name}>>'.");
        }
    }

    /// <summary>
    /// Gets the item indices from a MudDropZone using reflection to access private fields.
    /// </summary>
    /// <typeparam name="T">The type of items in the drop zone.</typeparam>
    /// <param name="dropZone">The MudDropZone instance.</param>
    /// <returns>A dictionary mapping items to their indices within the drop zone.</returns>
    /// <exception cref="ArgumentException">Thrown when the expected field is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the field is not of the expected type.</exception>
    public static Dictionary<T, int> GetIndexs<T>(this MudDropZone<T> dropZone)
        where T : notnull
    {
        // Get the type of the MudDropZone instance
        Type dropZoneType = dropZone.GetType();

        // Get the FieldInfo for the private _indices field
        FieldInfo? fieldInfo = dropZoneType.GetField("_indices", BindingFlags.NonPublic | BindingFlags.Instance);

        // Check if the field was found
        if (fieldInfo == null)
        {
            throw new ArgumentException($"Field '_indices' not found in type '{dropZoneType.FullName}'.");
        }

        // Get the value of the field
        var fieldValue = fieldInfo.GetValue(dropZone);

        // Cast the field value to the expected type
        if (fieldValue is Dictionary<T, int> dictionary)
        {
            return dictionary;
        }
        else
        {
            throw new InvalidCastException($"Field '_indices' is not of the expected type 'Dictionary<{typeof(T).Name}, int>'.");
        }
    }
}

// class
