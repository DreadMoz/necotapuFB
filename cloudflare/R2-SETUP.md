# Cloudflare R2 で WebGL Build を配信する手順

Firebase Hosting には `index.html` など薄いシェルのみ載せ、`WebGL/Build/` の大きなファイルは R2 に置く構成です。`firebase.json` の `ignore` により `Build/` は Firebase に上がりません。

## 1. R2 バケットを作成

Cloudflare ダッシュボード → R2 → Create bucket。

## 2. 公開アクセス（r2.dev）

バケット設定で **Public access** を有効にし、表示される **`https://pub-….r2.dev`** のようなベース URL を控える。独自ドメインは不要。

## 3. CORS

バケットの **CORS Policy** に、`cloudflare/r2-cors.example.json` と同様のルールを設定する。オリジンは実際の Firebase URL に合わせてよい。

ローカル検証では `http://localhost:8080` などを `AllowedOrigins` に一時追加する。

## 4. アップロード

`WebGL/Build/` 以下を、バケット上で **`Build/` というプレフィックス付き**で置く（`Build/WebGL.loader.js` など）。ルート直下に置かないこと。

アップロード時、**Brotli 圧縮ファイル（`.br`）には `Content-Encoding: br` が付く**ようにする。付かないと Unity が起動しない。

- AWS CLI（R2 の S3 互換エンドポイント）や Wrangler でアップロードする場合、オブジェクトのメタデータに `ContentEncoding: br` を指定する。
- ダッシュボードからアップロードする場合は、オブジェクトごとの HTTP メタデータを確認する。

## 5. `index.html` の `ASSET_BASE_URL`

`WebGL/index.html` の次の行を、手順 2 の **公開 URL（末尾スラッシュなし）** に変更する。

```javascript
var ASSET_BASE_URL = "https://pub-xxxxxxxx.r2.dev";
```

`buildUrl` は自動的に `…/Build` になる。

## 6. Firebase にデプロイ

```bash
firebase deploy --only hosting
```

`Build/` は含まれない。ゲーム本体は R2 から読み込まれる。

## 7. 成功判定

- ブラウザのネットワークで `…r2.dev/Build/WebGL.wasm.br` などが **200**。
- レスポンスヘッダに **`Content-Encoding: br`**（`.br` ファイル）。
- コンソールに CORS エラーがない。
- ゲームがタイトルまで起動する。

## ローカルで同一リポジトリを試す

`ASSET_BASE_URL = ""` のまま、`WebGL` フォルダに `Build/` がある状態で静的サーバー（例: `npx serve WebGL`）を使うと、同一オリジンでロードできる。`firebase serve` は `Build/` をホスティング対象外にしているため、フルビルドの確認には向かない。
