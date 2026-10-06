// <copyright file="ReflectionExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IndTrace.CognexCommTests")]

namespace IndTrace.Application.Models.Extensions;

/// <summary>
/// Provides extension methods for invoking private members and manipulating fields/properties using reflection.
/// </summary>
internal static class ReflectionExtensions
{
    /// <summary>
    /// Invokes a private method from a given object using reflection.
    /// Throws an ArgumentOutOfRangeException if the method is not found.
    /// </summary>
    /// <typeparam name="T">Type of the method's declaring class.</typeparam>
    /// <typeparam name="TU">Return type of the method.</typeparam>
    /// <param name="obj">Object from which the method is invoked.</param>
    /// <param name="methodName">Method name as string.</param>
    /// <param name="args">Arguments for invoking the method.</param>
    /// <returns>The result of the invoked method.</returns>
    public static IndQuestResults.Result<TU> InvokeMethod<T, TU>(this T obj, string methodName, params object[] args)
    {
        var declaredMethod = typeof(T).GetTypeInfo().GetDeclaredMethod(methodName);
        if (declaredMethod == null)
        {
            return IndQuestResults.Result<TU>.WithFailure($"Method {methodName} not found on type {typeof(T).Name}");
        }

        var result = declaredMethod.Invoke(obj, args);
        return result is not null ? IndQuestResults.Result<TU>.Success((TU)result) : IndQuestResults.Result<TU>.WithFailure("Method invocation returned null");
    }

    /// <summary>
    /// Invokes a private method from a given object using reflection.
    /// Throws an ArgumentOutOfRangeException if the method is not found.
    /// </summary>
    /// <typeparam name="T">Type of the method's declaring class.</typeparam>
    /// <param name="obj">Object from which the method is invoked.</param>
    /// <param name="methodName">Method name as string.</param>
    /// <param name="args">Arguments for invoking the method.</param>
    /// <returns>The result of the invoked method as an object.</returns>
    public static IndQuestResults.Result<object> InvokeMethod<T>(this T obj, string methodName, params object[] args)
    {
        var declaredMethod = typeof(T).GetTypeInfo().GetDeclaredMethod(methodName);
        if (declaredMethod == null)
        {
            return IndQuestResults.Result<object>.WithFailure($"Method {methodName} not found on type {typeof(T).Name}");
        }

        var result = declaredMethod.Invoke(obj, args);
        return result is not null ? IndQuestResults.Result<object>.Success(result) : IndQuestResults.Result<object>.WithFailure("Method invocation returned null");
    }

    /// <summary>
    /// Returns a private property value from a given object using reflection.
    /// Throws an ArgumentOutOfRangeException if the property is not found.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="obj">Object from which the property value is returned.</param>
    /// <param name="propName">Property name as string.</param>
    /// <returns>The value of the property.</returns>
    public static IndQuestResults.Result<T> GetPrivatePropertyValue<T>(this object obj, string propName)
    {
        if (obj == null)
        {
            return IndQuestResults.Result<T>.WithFailure("obj cannot be null");
        }

        var property = obj.GetType().GetProperty(
            propName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property == null)
        {
            return IndQuestResults.Result<T>.WithFailure($"Property {propName} was not found in Type {obj.GetType().FullName}");
        }

        var value = property.GetValue(obj, null);
        return value is not null ? IndQuestResults.Result<T>.Success((T)value) : IndQuestResults.Result<T>.WithFailure("Property value is null");
    }

    /// <summary>
    /// Returns a private field value from a given object using reflection.
    /// Throws an ArgumentOutOfRangeException if the field is not found.
    /// </summary>
    /// <typeparam name="T">Type of the field.</typeparam>
    /// <param name="obj">Object from which the field value is returned.</param>
    /// <param name="propName">Field name as string.</param>
    /// <returns>The value of the field.</returns>
    public static IndQuestResults.Result<T> GetPrivateFieldValue<T>(this object obj, string propName)
    {
        if (obj == null)
        {
            return IndQuestResults.Result<T>.WithFailure("obj cannot be null");
        }

        var type = obj.GetType();
        FieldInfo? fieldInfo = null;
        while (fieldInfo == null && type != null)
        {
            fieldInfo = type.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            type = type.BaseType;
        }

        if (fieldInfo == null)
        {
            return IndQuestResults.Result<T>.WithFailure($"Field {propName} was not found in Type {obj.GetType().FullName}");
        }

        var value = fieldInfo.GetValue(obj);
        return value is not null ? IndQuestResults.Result<T>.Success((T)value) : IndQuestResults.Result<T>.WithFailure("Field value is null");
    }

    /// <summary>
    /// Sets a private property value on a given object using reflection.
    /// Throws an ArgumentOutOfRangeException if the property is not found.
    /// </summary>
    /// <typeparam name="T">Type of the property.</typeparam>
    /// <param name="obj">Object on which the property value is set.</param>
    /// <param name="propName">Property name as string.</param>
    /// <param name="val">Value to set.</param>
    /// <returns></returns>
    public static IndQuestResults.Result SetPrivatePropertyValue<T>(this object obj, string propName, T val)
    {
        var type = obj.GetType();
        if (type.GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) == null)
        {
            return IndQuestResults.Result.WithFailure($"Property {propName} was not found in Type {obj.GetType().FullName}");
        }

        type.InvokeMember(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance, null, obj, new object?[] { val });
        return IndQuestResults.Result.Success();
    }

    /// <summary>
    /// Sets a private field value on a given object using reflection.
    /// Throws an ArgumentOutOfRangeException if the field is not found.
    /// </summary>
    /// <typeparam name="T">Type of the field.</typeparam>
    /// <param name="obj">Object on which the field value is set.</param>
    /// <param name="propName">Field name as string.</param>
    /// <param name="val">Value to set.</param>
    /// <returns></returns>
    public static IndQuestResults.Result SetPrivateFieldValue<T>(this object obj, string propName, T val)
    {
        if (obj == null)
        {
            return IndQuestResults.Result.WithFailure("obj cannot be null");
        }

        var type = obj.GetType();
        FieldInfo? fieldInfo = null;
        while (fieldInfo == null && type != null)
        {
            fieldInfo = type.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            type = type.BaseType;
        }

        if (fieldInfo == null)
        {
            return IndQuestResults.Result.WithFailure($"Field {propName} was not found in Type {obj.GetType().FullName}");
        }

        fieldInfo.SetValue(obj, val);
        return IndQuestResults.Result.Success();
    }
}