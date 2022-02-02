#!/bin/bash
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
