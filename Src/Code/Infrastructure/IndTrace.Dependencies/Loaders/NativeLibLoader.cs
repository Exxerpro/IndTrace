// <copyright file="NativeLibLoader.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Dependencies.Loaders;

/// <summary>
/// Provides methods to load native libraries dynamically based on the operating system.
/// </summary>
public static class NativeLibLoader
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("libdl.so")]
    private static extern IntPtr Dlopen(string fileName, int flags);

    /// <summary>
    /// Attempts to load a native library with the specified base name and logs the process.
    /// </summary>
    /// <param name="baseName">The base name of the native library (without extension or prefix).</param>
    /// <param name="logger">The logger to use for logging information and errors.</param>
    public static void LoadNativeLibrary(string baseName, ILogger logger)
    {
        string[] searchPaths = {
            AppContext.BaseDirectory,
            Path.Combine(AppContext.BaseDirectory, "native", "win"),
            Path.Combine(AppContext.BaseDirectory, "native", "linux"),
        };

        string extension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".dll"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? ".so"
            : throw new PlatformNotSupportedException("Unsupported OS");

        string fileName = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? string.Empty : "lib") + baseName + extension;

        foreach (var path in searchPaths)
        {
            string fullPath = Path.Combine(path, fileName);

            if (File.Exists(fullPath))
            {
                logger.LogInformation("Trying to load native library from: {Path}", fullPath);

                IntPtr handle = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? LoadLibrary(fullPath)
                    : Dlopen(fullPath, 2); // RTLD_NOW

                if (handle != IntPtr.Zero)
                {
                    logger.LogInformation("Successfully loaded native library from: {Path}", fullPath);
                    return;
                }
                else
                {
                    logger.LogWarning("Library found but failed to load: {Path}", fullPath);
                }
            }
            else
            {
                logger.LogDebug("Library not found at: {Path}", fullPath);
            }
        }

        logger.LogError("Failed to locate and load '{Library}' in any known paths.", fileName);
    }
}

