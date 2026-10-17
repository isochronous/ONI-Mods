#!/usr/bin/env bash
# finish.sh <mod-name> [dry|write]: close every open pass of the node, validate each, then dry-run or write round r1.
set -u
cd /c/dev/ONI-Mods || exit 1
W=.context/tasks/OM-002-store-backfills/work/tags
n=$1; mode=${2:-dry}; R=${ROUND:+-$ROUND}; RT=${ROUND:-r1}
for c in $(node -e 'console.log(require(process.argv[1]).map(c=>c.id).join(" "))' "$PWD/$W/chunks-$n$R.json"); do
  if [ -f "$W/tagout-$c.partial" ]; then node $W/tagwork.mjs complete tagout-$c > /dev/null || exit 1; fi
  node $W/tagwork.mjs check mods/$n $c | grep -q '"ok": true' || { echo "CHECK FAILED $c"; node $W/tagwork.mjs check mods/$n $c; exit 1; }
done
node $W/tagwork.mjs $mode mods/$n $RT
