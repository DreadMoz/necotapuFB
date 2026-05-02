# デプロイ手順（メモ）

## 前提

- 大きなファイルは **R2**（`ASSET_BASE_URL` → `WebGL/Build/`）。
- 薄いファイルは **Firebase Hosting**（`WebGL/` のうち `Build/` 以外）。
- `firebase.json` で `Build/**` は Hosting に上げない。

## パターン別

### A. Unity で WebGL をビルドし直したとき

1. `./cloudflare/upload-build-to-r2.sh`（`wrangler login` 済みの環境で）
2. プレビュー確認するなら: `firebase hosting:channel:deploy <チャンネル名>`
3. 本番: `firebase deploy --only hosting`

### B. `index.html` / CSS / JSON などだけ変えたとき（ビルドはそのまま）

1. `firebase deploy --only hosting`  
   （プレビューなら `firebase hosting:channel:deploy <チャンネル名>`）

### C. プレビュー URL が新しくなったとき

- R2 の CORS `AllowedOrigins` に、そのプレビューの `https://…web.app` を 1 行追加（`r2-cors.example.json` を参考）
- Firebase Authentication の承認済みドメインに、そのホスト名を追加

## コマンド一覧（プロジェクトルート）

| 目的           | コマンド |
|----------------|----------|
| R2 に Build を載せる | `./cloudflare/upload-build-to-r2.sh` |
| 本番 Hosting     | `firebase deploy --only hosting` |
| プレビュー Hosting | `firebase hosting:channel:deploy <名前>` |

※ `firebase hosting:channel:deploy` に `--only hosting` は付けない（CLI 15 系でエラーになることがある）。

詳細は `R2-SETUP.md`。
