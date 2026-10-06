// <copyright file="SqlScriptRunner.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace IndTrace.Persistence.Helper;

public static class SqlScriptRunner
{
    public static void ApplyScriptsFrom(string folderPath, DbContext context)
    {
        var files = Directory.GetFiles(folderPath, "*.sql", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var sql = File.ReadAllText(file);
            context.Database.ExecuteSqlRaw(sql); // or ExecuteSqlInterpolated if needed
            Console.WriteLine($"✔ Executed: {Path.GetFileName(file)}");
        }
    }
}