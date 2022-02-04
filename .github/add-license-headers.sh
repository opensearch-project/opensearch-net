#!/bin/bash
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

script_path=$(dirname $(realpath -s $0))/../

LEN=$(wc -c .github/license-header.txt | awk '{print $1}')

function add_license () {
    (find "$script_path" -name $1 | grep -v "/bin/" | grep -v "/obj/" )|while read fname; do
        if ! [[ $(head -c $LEN $fname) == $(cat $script_path.github/license-header.txt) ]] ; then
            # awk joins the header with the existing file, inserting a newline between them
            awk '(NR>1 && FNR==1){print ""}1' "${script_path}.github/license-header.txt" "$fname" > "${fname}.new"
            mv "${fname}.new" "$fname"
        fi
    done
}

add_license "*.cs"
add_license "*.fs"
