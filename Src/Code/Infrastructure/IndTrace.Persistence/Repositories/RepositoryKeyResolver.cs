// <copyright file="RepositoryKeyResolver.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// Story 35.D1 (#35/F13): translates the raw <see cref="int"/> id accepted by the generic
/// <c>GetByIdAsync(int)</c> repository seam into the entity key's <b>model</b> value before it is handed to EF Core's
/// <c>DbSet.FindAsync</c>.
/// <para>
/// <c>FindAsync</c> matches on the key property's <b>CLR (model) type</b>, not its provider/column type. For the
/// overwhelming majority of entities the key is a plain <see cref="int"/> and this is an identity pass-through. For a
/// strongly-typed key behind a value converter (the <c>BarCode</c> entity's <c>BarCodeId</c> struct piloted here),
/// passing the raw <see cref="int"/> would never match, so the id is converted to the model value through the key's
/// registered value converter (<c>int -&gt; BarCodeId</c> via <c>ConvertFromProvider</c>). This keeps the generic
/// single-int-key seam working for both plain-int keys and value-converted keys without any per-entity branching.
/// </para>
/// </summary>
internal static class RepositoryKeyResolver
{
    /// <summary>
    /// Resolves the model-typed key value for a single-column primary key from a raw <see cref="int"/> id.
    /// </summary>
    /// <param name="model">The built EF Core model.</param>
    /// <param name="entityType">The entity CLR type being looked up.</param>
    /// <param name="id">The raw integer identifier supplied to the generic <c>GetByIdAsync(int)</c> seam.</param>
    /// <returns>
    /// The key value in the entity key's model type: the boxed <see cref="int"/> unchanged when the key has no value
    /// converter (the common case), or the converted model value (e.g. a <c>BarCodeId</c>) when it does.
    /// </returns>
    public static object ResolveKeyValue(IModel model, System.Type entityType, int id)
    {
        var key = model.FindEntityType(entityType)?.FindPrimaryKey();
        if (key is null || key.Properties.Count != 1)
        {
            return id;
        }

        var converter = key.Properties[0].GetValueConverter();
        if (converter is null)
        {
            return id;
        }

        return converter.ConvertFromProvider(id) ?? id;
    }
}
