#!/usr/bin/env bash
# SPDX-License-Identifier: Apache-2.0
#
# The OpenSearch Contributors require contributions made to
# this file be licensed under the Apache-2.0 license or a
# compatible open source license.
#
# Modifications Copyright OpenSearch Contributors. See
# GitHub history for details.
#
#  Licensed to Elasticsearch B.V. under one or more contributor
#  license agreements. See the NOTICE file distributed with
#  this work for additional information regarding copyright
#  ownership. Elasticsearch B.V. licenses this file to you under
#  the Apache License, Version 2.0 (the "License"); you may
#  not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#
# 	http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing,
#  software distributed under the License is distributed on an
#  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
#  KIND, either express or implied.  See the License for the
#  specific language governing permissions and limitations
#  under the License.

# Check that source code files in this repo have the appropriate license
# header.

if [ "$TRACE" != "" ]; then
    export PS4='${BASH_SOURCE}:${LINENO}: ${FUNCNAME[0]:+${FUNCNAME[0]}(): }'
    set -o xtrace
fi
set -o errexit
set -o pipefail

TOP=$(cd "$(dirname "$0")/.." >/dev/null && pwd)
NLINES_CS=$(wc -l .github/license-header.txt | awk '{print $1}')
NLINES_FS=$(wc -l .github/license-header-fs.txt | awk '{print $1}')

function check_license_header {
    local f
    f=$1
    if [[ $f == *.fs ]] && ! diff -a --strip-trailing-cr .github/license-header-fs.txt <(head -$NLINES_FS "$f") >/dev/null; then
        echo $f
        echo "check-license-headers: error: '$f' does not have required license header, see 'diff -u .github/license-header-fs.txt <(head -$NLINES_FS $f)'"
        return 1
    elif [[ $f != *.fs ]] && ! diff -a --strip-trailing-cr .github/license-header.txt <(head -$NLINES_CS "$f") >/dev/null; then
        echo "check-license-headers: error: '$f' does not have required license header, see 'diff -u .github/license-header.txt <(head -$NLINES_CS $f)'"
        return 1
    else
        return 0
    fi
}

cd "$TOP"
nErrors=0
for f in $(git ls-files | grep '\.cs$'); do
    if ! check_license_header $f; then
        nErrors=$((nErrors+1))
    fi
done

for f in $(git ls-files | grep '\.fs$'); do
    if ! check_license_header $f; then
        nErrors=$((nErrors+1))
    fi
done

if [[ $nErrors -eq 0 ]]; then
    exit 0
else
    exit 1
fi
