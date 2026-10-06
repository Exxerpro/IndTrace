// <copyright file="ValidationHelper.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using ValidationException = IndTrace.Application.Models.Exceptions.ValidationException;

namespace IndTrace.Aggregation.BoundedTests.Services;

public static class ValidationHelper
{
    public static void Validate<T>(this T instance, IValidator<T> validator)
        where T : class
    {
        var requestInterface = typeof(T).GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IMonitorRequest<>) || i.GetGenericTypeDefinition() == typeof(IGatewayRequest<>)));

        if (requestInterface == null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} does not implement IMonitorRequest<TResponse>");
        }

        var result = validator.Validate(instance);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}