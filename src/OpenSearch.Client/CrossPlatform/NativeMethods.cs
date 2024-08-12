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
// https://raw.githubusercontent.com/dotnet/core-setup/master/src/managed/Microsoft.DotNet.PlatformAbstractions/Native/NativeMethods.Windows.cs

using System.Runtime.InteropServices;

namespace OpenSearch.Client;

internal static class NativeMethods
{
    public static class Windows
    {
        // This call avoids the shimming Windows does to report old versions
        [DllImport("ntdll")]
        private static extern int RtlGetVersion(out RTL_OSVERSIONINFOEX lpVersionInformation);

        internal static string RtlGetVersion()
        {
            var osvi = new RTL_OSVERSIONINFOEX();
            osvi.dwOSVersionInfoSize = (uint)Marshal.SizeOf(osvi);
            if (RtlGetVersion(out osvi) == 0)
                return $"Microsoft Windows {osvi.dwMajorVersion}.{osvi.dwMinorVersion}.{osvi.dwBuildNumber}";

            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct RTL_OSVERSIONINFOEX
        {
            internal uint dwOSVersionInfoSize;
            internal uint dwMajorVersion;
            internal uint dwMinorVersion;
            internal uint dwBuildNumber;
            internal uint dwPlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            internal string szCSDVersion;
        }
    }
}
