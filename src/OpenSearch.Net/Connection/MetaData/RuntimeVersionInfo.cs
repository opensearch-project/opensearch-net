/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*/
/*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

// Adapted from BenchmarkDotNet source https://github.com/dotnet/BenchmarkDotNet/blob/master/src/BenchmarkDotNet/Environments/Runtimes/CoreRuntime.cs
#region BenchmarkDotNet License https://github.com/dotnet/BenchmarkDotNet/blob/master/LICENSE.md
// The MIT License
// Copyright (c) 2013–2020.NET Foundation and contributors

// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all copies or substantial
// portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OpenSearch.Net
{
    /// <summary>
    /// Represents the current .NET Runtime version.
    /// </summary>
    internal sealed class RuntimeVersionInfo : VersionInfo
    {
        public static readonly RuntimeVersionInfo Default = new RuntimeVersionInfo { Version = new Version(0, 0, 0), IsPrerelease = false };

        public RuntimeVersionInfo() => StoreVersion(GetRuntimeVersion());

        private static string GetRuntimeVersion() =>
            GetNetCoreVersion();

        private static string GetNetCoreVersion()
        {
            // for .NET 5+ we can use Environment.Version
            if (Environment.Version.Major >= 5)
            {
                const string dotNet = ".NET ";
                var index = RuntimeInformation.FrameworkDescription.IndexOf(dotNet, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    return RuntimeInformation.FrameworkDescription.Substring(dotNet.Length);
                }
            }

            // next, try using file version info
            var systemPrivateCoreLib = FileVersionInfo.GetVersionInfo(typeof(object).Assembly.Location);
            if (TryGetVersionFromProductInfo(systemPrivateCoreLib.ProductVersion, systemPrivateCoreLib.ProductName, out var runtimeVersion))
            {
                return runtimeVersion;
            }

            var assembly = typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly;
            if (TryGetVersionFromAssemblyPath(assembly, out runtimeVersion))
            {
                return runtimeVersion;
            }

            //At this point, we can't identify whether this is a prerelease, but a version is better than nothing!

            var frameworkName = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            if (TryGetVersionFromFrameworkName(frameworkName, out runtimeVersion))
            {
                return runtimeVersion;
            }

            if (IsRunningInContainer)
            {
                var dotNetVersion = Environment.GetEnvironmentVariable("DOTNET_VERSION");
                var aspNetCoreVersion = Environment.GetEnvironmentVariable("ASPNETCORE_VERSION");

                return dotNetVersion ?? aspNetCoreVersion;
            }

            return null;
        }

        private static bool TryGetVersionFromAssemblyPath(Assembly assembly, out string runtimeVersion)
        {
#if NET6_0_OR_GREATER
			var assemblyPath = assembly.Location.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
#else
            var assemblyPath = assembly.CodeBase.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
#endif
            var netCoreAppIndex = Array.IndexOf(assemblyPath, "Microsoft.NETCore.App");
            if (netCoreAppIndex > 0 && netCoreAppIndex < assemblyPath.Length - 2)
            {
                runtimeVersion = assemblyPath[netCoreAppIndex + 1];
                return true;
            }

            runtimeVersion = null;
            return false;
        }

        // NOTE: 5.0.1 FrameworkDescription returns .NET 5.0.1-servicing.20575.16, so we special case servicing as NOT prerelease
        protected override bool ContainsPrerelease(string version) => base.ContainsPrerelease(version) && !version.Contains("-servicing");

        // sample input:
        // 2.0: 4.6.26614.01 @BuiltBy: dlab14-DDVSOWINAGE018 @Commit: a536e7eec55c538c94639cefe295aa672996bf9b, Microsoft .NET Framework
        // 2.1: 4.6.27817.01 @BuiltBy: dlab14-DDVSOWINAGE101 @Branch: release/2.1 @SrcCode: https://github.com/dotnet/coreclr/tree/6f78fbb3f964b4f407a2efb713a186384a167e5c, Microsoft .NET Framework
        // 2.2: 4.6.27817.03 @BuiltBy: dlab14-DDVSOWINAGE101 @Branch: release/2.2 @SrcCode: https://github.com/dotnet/coreclr/tree/ce1d090d33b400a25620c0145046471495067cc7, Microsoft .NET Framework
        // 3.0: 3.0.0-preview8.19379.2+ac25be694a5385a6a1496db40de932df0689b742, Microsoft .NET Core
        // 5.0: 5.0.0-alpha1.19413.7+0ecefa44c9d66adb8a997d5778dc6c246ad393a7, Microsoft .NET Core
        private static bool TryGetVersionFromProductInfo(string productVersion, string productName, out string version)
        {
            if (string.IsNullOrEmpty(productVersion) || string.IsNullOrEmpty(productName))
            {
                version = null;
                return false;
            }

            // yes, .NET Core 2.X has a product name == .NET Framework...
            if (productName.IndexOf(".NET Framework", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                const string releaseVersionPrefix = "release/";
                var releaseVersionIndex = productVersion.IndexOf(releaseVersionPrefix);
                if (releaseVersionIndex > 0)
                {
                    version = productVersion.Substring(releaseVersionIndex + releaseVersionPrefix.Length);
                    return true;
                }
            }

            // matches .NET Core and also .NET 5+
            if (productName.IndexOf(".NET", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                version = productVersion;
                return true;
            }

            version = null;
            return false;
        }

        // sample input:
        // .NETCoreApp,Version=v2.0
        // .NETCoreApp,Version=v2.1
        private static bool TryGetVersionFromFrameworkName(string frameworkName, out string runtimeVersion)
        {
            const string versionPrefix = ".NETCoreApp,Version=v";
            if (!string.IsNullOrEmpty(frameworkName) && frameworkName.StartsWith(versionPrefix))
            {
                runtimeVersion = frameworkName.Substring(versionPrefix.Length);
                return true;
            }

            runtimeVersion = null;
            return false;
        }

        private static bool IsRunningInContainer => string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true");
    }
}
