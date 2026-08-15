#!/usr/bin/env bash
#
# Populates Dependencies/ with the assemblies the mod compiles against.
#
# BepInEx comes from the pack pinned in manifest.json -- the same pack the mod manager installs
# for players -- so the build cannot resolve an API that the shipped BepInEx does not have.
# The game's own assemblies are copied from an Inscryption install, which has to be supplied.
#
#   ./tools/fetch-dependencies.sh                     # BepInEx only
#   ./tools/fetch-dependencies.sh /path/to/Inscryption  # BepInEx and the game's assemblies
#
# Dependencies/ is gitignored, so this is what a fresh clone runs instead of a checkout.

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
deps="$repo_root/Dependencies"
csproj="$repo_root/Archipelago_Inscryption/Archipelago_Inscryption.csproj"
game_dir="${1:-}"

mkdir -p "$deps"

# The dependency string is "Namespace-Name-Version", which is also how the download url is built.
dep="$(grep -oE '"[A-Za-z0-9_]+-[A-Za-z0-9_]+-[0-9]+(\.[0-9]+)+"' "$repo_root/manifest.json" | tr -d '"' | head -1)"
if [ -z "$dep" ]; then
    echo "No BepInEx pack pinned in manifest.json; nothing to fetch." >&2
    exit 1
fi

namespace="${dep%%-*}"
rest="${dep#*-}"
pack_name="${rest%-*}"
pack_version="${rest##*-}"

echo "Pinned pack: $namespace/$pack_name $pack_version"

tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT

curl -sSL --fail -o "$tmp/pack.zip" \
    "https://thunderstore.io/package/download/$namespace/$pack_name/$pack_version/"
unzip -q -o "$tmp/pack.zip" -d "$tmp/pack"

core="$(find "$tmp/pack" -type d -name core -path '*BepInEx*' | head -1)"
if [ -z "$core" ]; then
    echo "The pack has no BepInEx/core folder; its layout must have changed." >&2
    exit 1
fi

# Only what the project references, so an assembly the build does not use is not vendored for it.
# The csproj writes its HintPaths with backslashes, hence either separator here.
wanted="$(grep -oE '<HintPath>[^<]*Dependencies[\\/][^<]+</HintPath>' "$csproj" \
    | sed -E 's|.*Dependencies[\\/]([^<]+)</HintPath>|\1|')"

if [ -z "$wanted" ]; then
    echo "No Dependencies references found in $csproj; its reference style must have changed." >&2
    exit 1
fi

copied_bepinex=0
kept=0
missing=""

for dll in $wanted; do
    if [ -f "$core/$dll" ]; then
        cp "$core/$dll" "$deps/$dll"
        copied_bepinex=$((copied_bepinex + 1))
    elif [ -f "$deps/$dll" ]; then
        # Left alone: a store's build can ship a stub where another ships the real assembly, and
        # the one already vendored is the one this checkout is known to compile against.
        kept=$((kept + 1))
    elif [ -n "$game_dir" ]; then
        # Same install, two layouts: the app bundle on macOS, a data folder everywhere else.
        managed=""
        for candidate in \
            "$game_dir/Inscryption_Data/Managed" \
            "$game_dir/Inscryption.app/Contents/Resources/Data/Managed"; do
            if [ -d "$candidate" ]; then
                managed="$candidate"
                break
            fi
        done

        if [ -z "$managed" ]; then
            echo "No Managed folder under $game_dir; is that an Inscryption install?" >&2
            exit 1
        fi

        if [ -f "$managed/$dll" ]; then
            cp "$managed/$dll" "$deps/$dll"
        else
            missing="$missing $dll"
        fi
    elif [ ! -f "$deps/$dll" ]; then
        missing="$missing $dll"
    fi
done

echo "Vendored $copied_bepinex assemblies from the pack into Dependencies/."
if [ "$kept" -gt 0 ]; then
    echo "Kept $kept game assemblies already there; delete one to have it recopied."
fi

if [ -n "$missing" ]; then
    echo
    echo "Still missing, copy these from your Inscryption install's Managed folder:" >&2
    for dll in $missing; do echo "  $dll" >&2; done
    [ -z "$game_dir" ] && echo "(or rerun with the install path: $0 /path/to/Inscryption)" >&2
    exit 1
fi
