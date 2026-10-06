// <copyright file="ConfigLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Interceptors
{
    /// <summary>
    /// Provides methods to load application configuration from JSON files and environment variables.
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// Loads the application configuration, including environment-specific overrides if present.
        /// </summary>
        /// <returns>The loaded <see cref="IConfiguration"/> instance.</returns>
        public static IConfiguration Load()
        {
            var baseDir = AppContext.BaseDirectory;
            var settingsPath = Path.Combine(baseDir, "..", "settings");

            if (!Directory.Exists(settingsPath))
            {
                // Fallback for Debug mode
                settingsPath = Directory.GetCurrentDirectory();
            }

            Console.WriteLine($"[ConfigLoader] Using settings directory: {settingsPath}");

            // reloadOnChange:false — this throwaway builder only reads the active-profile key once. With
            // reloadOnChange:true its IConfigurationRoot (and the FileSystemWatcher it owns) is discarded
            // un-disposed on every Load() call, leaking a watcher each time. No code consumes reload here.
            var preliminaryConfig = new ConfigurationBuilder()
                .SetBasePath(settingsPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            // The profile is selected from the committed appsettings.json by default, but a host may
            // override it without editing that shared file via the INDTRACE_ACTIVE_CONFIG environment
            // variable. This lets a Linux/container host pick its own profile (e.g. "Docker") while the
            // committed default stays targeted at the Windows deploy. The override is read here (before
            // the profile file is composed) because AddEnvironmentVariables() only feeds the FINAL
            // configuration and cannot influence WHICH appsettings.<profile>.json is loaded.
            var activeProfile = Environment.GetEnvironmentVariable("INDTRACE_ACTIVE_CONFIG")
                ?? preliminaryConfig["AppSettings:ActiveConfig"];
            Console.WriteLine($"[ConfigLoader] Active profile: {activeProfile}");

            // reloadOnChange:false here too — nothing in the app subscribes to configuration-reload change
            // tokens, so hot-reload watchers add no value and only leak OS file handles.
            var finalConfig = new ConfigurationBuilder()
                .SetBasePath(settingsPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{activeProfile}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            return finalConfig;
        }
    }
}