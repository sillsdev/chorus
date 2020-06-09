#!/bin/bash
cd "$(dirname "$0")/.."
if [[ $(mono --version 2>/dev/null | head -1 | cut -f 5 -d ' ') < 6 ]]; then
	. environ
fi
cd build
msbuild "/target:${2:-Clean;Build}" /property:Configuration="${1:-Debug}" Chorus.proj
