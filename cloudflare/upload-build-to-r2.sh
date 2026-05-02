#!/usr/bin/env bash
# WebGL/Build を R2 に載せる。*.unityweb は Content-Encoding: br が必須（Unity の Brotli 圧縮と一致させる）。
# 使い方: リポジトリルートで ./cloudflare/upload-build-to-r2.sh
# 事前: wrangler login（または CLOUDFLARE_API_TOKEN）。バケット名は環境変数 R2_BUCKET で上書き可。

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
BUILD="$REPO_ROOT/WebGL/Build"
BUCKET="${R2_BUCKET:-necotapu-webgl}"

for f in WebGL.loader.js WebGL.data.unityweb WebGL.framework.js.unityweb WebGL.wasm.unityweb; do
  if [[ ! -f "$BUILD/$f" ]]; then
    echo "missing: $BUILD/$f" >&2
    exit 1
  fi
done

WR=(npx wrangler r2 object put)

echo "Uploading to r2://$BUCKET/Build/ ..."

"${WR[@]}" "$BUCKET/Build/WebGL.loader.js" \
  --file="$BUILD/WebGL.loader.js" \
  --content-type application/javascript \
  --remote

for f in WebGL.data.unityweb WebGL.framework.js.unityweb WebGL.wasm.unityweb; do
  "${WR[@]}" "$BUCKET/Build/$f" \
    --file="$BUILD/$f" \
    --content-encoding br \
    --remote
done

echo "Done. CORS の AllowedOrigins にページのオリジン（本番・プレビュー）が含まれているか確認すること。"
