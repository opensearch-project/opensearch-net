#!/usr/bin/env bash

set -ex

IFS='.' read -r -a VERSION_COMPONENTS <<< "$1"
MAJOR="${VERSION_COMPONENTS[0]}"
MINOR="${VERSION_COMPONENTS[1]}"
PATCH="${VERSION_COMPONENTS[2]}"

if [[ -z "$MAJOR" || -z "$MINOR" || -z "$PATCH" ]]; then
  echo "Usage: $0 <major>.<minor>.<patch>"
  exit 1
fi

VERSION="$MAJOR.$MINOR.$PATCH"

AUTO_LABEL_JSON=$(jq \
	--arg v "$VERSION" \
	'.rules |= with_entries(if .key | test("^v\\d+\\.\\d+\\.\\d+$") then .key |= "v\($v)" else . end)' \
	.github/auto-label.json)
echo "$AUTO_LABEL_JSON" > .github/auto-label.json

GLOBAL_JSON=$(jq \
	--arg v "$VERSION" \
	--arg maj "$MAJOR" \
	--arg min "$MINOR" \
	'.version = "\($v)" | .doc_current = "\($maj).\($min)" | .doc_branch = "\($maj).x"' \
	global.json)
echo "$GLOBAL_JSON" > global.json

sed -i '' -E "s/^([[:space:]]+VERSION: )([0-9]+\.){2}[0-9]+$/\1$VERSION/" .github/workflows/release.yml
sed -i '' -E "s/(<Current(Assembly(File)?)?Version>)([0-9]+\.){2}[0-9]+<\//\1$VERSION<\//" Directory.Build.props
