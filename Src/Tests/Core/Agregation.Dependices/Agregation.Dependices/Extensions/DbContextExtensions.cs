// <copyright file="DbContextExtensions.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Agregation.Dependices.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task UpsertRangeAsync<TEntity, TKey>(
            this Microsoft.EntityFrameworkCore.DbContext context,
            IEnumerable<TEntity> entities,
            Func<TEntity, TKey> keySelector)
            where TEntity : class
            where TKey : notnull
        {
            var enabled = false; //Because the data was cleansed we don't need to check for duplicates

            if (!enabled)
            {
                var dbFullSet = context.Set<TEntity>();
                await dbFullSet.AddRangeAsync(entities);
                return;
            }

            var dbSet = context.Set<TEntity>();
            var duplicateKeys = new List<TKey>();

            // Ensure only the first occurrence of each key is used
            var distinctEntities = entities
                .GroupBy(keySelector)
                .Select(g =>
                {
                    if (g.Count() > 1)
                        duplicateKeys.Add(g.Key);
                    return g.First();
                });
            foreach (var entity in distinctEntities)
            {
                var key = keySelector(entity);

                // Check if the entity with the same key already exists in the database

                var exists = await dbSet.FindAsync(new object[] { key });

                //var exists = await dbSet.FindAsync(entity.);
                if (exists is null)
                {
                    await dbSet.AddAsync(entity);
                }
                else
                {
                    //dbSet.Update(entity);
                }
            }

            //await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Log result
            if (duplicateKeys.Count > 0)
            {
                Console.WriteLine($"[Upsert Skipped Duplicates: {typeof(TEntity).Name}]");
                foreach (var key in duplicateKeys)
                    Console.WriteLine($" - Duplicate Key: {key}, ");
            }
        }

        public static async Task UpsertRangeAsync<TEntity, TKey1, TKey2>(
            this DbContext context,
            IEnumerable<TEntity> entities,
            Func<TEntity, (TKey1, TKey2)> keySelector)
            where TEntity : class
            where TKey1 : notnull
            where TKey2 : notnull
        {
            var dbSet = context.Set<TEntity>();
            var duplicateKeys = new List<(TKey1, TKey2)>();

            // Deduplicate on composite key, keeping the first instance
            var distinctEntities = entities
                .GroupBy(keySelector)
                .Select(g =>
                {
                    if (g.Count() > 1)
                        duplicateKeys.Add(g.Key);
                    return g.First();
                });

            foreach (var entity in distinctEntities)
            {
                var (key1, key2) = keySelector(entity);

                // Attempt to find an existing entity with the composite key
                var existing = await dbSet.FindAsync(new object[] { key1!, key2! });

                if (existing is null)
                {
                    await dbSet.AddAsync(entity);
                }
                else
                {
                    //  context.Entry(existing).CurrentValues.SetValues(entity);
                }
            }

            // await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Log result
            if (duplicateKeys.Count > 0)
            {
                Console.WriteLine($"[Upsert Skipped Duplicates: {typeof(TEntity).Name}]");
                foreach (var key in duplicateKeys)
                    Console.WriteLine($" - Duplicate Key: ({key.Item1}, {key.Item2})");
            }
        }
    }
}