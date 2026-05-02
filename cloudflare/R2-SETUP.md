# Cloudflare R2 に WebGL の Build を載せる手順（ねこたぷ用）

## 何のためか

- **Firebase Hosting の転送量**は、WebGL の大きなファイル（`Build/` 以下）を何度も配ると増える。
- **R2** に `Build/` だけ置き、**Firebase には `index.html` や CSS・設定 JSON など薄いファイルだけ**残すと、**ゲームのバイト数の多くが Firebase の課金対象から外れる**（R2 は転送の課金モデルが別。詳細は Cloudflare の料金表を参照）。

## 切り替え後の役割分担

| 置き場所 | 載せるものの例 |
|----------|----------------|
| **Firebase Hosting**（いまの `necotapufb.web.app`） | `index.html`、`style.css`、画像、favicon、`nara_config.json`、`link.xml` など。**ページのオリジンはここ**のまま。 |
| **R2** | `WebGL/Build/` 以下の**すべて**（`WebGL.loader.js`、`*.unityweb`、`*.wasm.unityweb` など）。バケット内のパスは **`Build/ファイル名`** とする。 |

ブラウザは **`https://necotapufb.web.app/` でページを開き**、その中のスクリプトが **`https://pub-xxxx.r2.dev/Build/...` をクロスオリジンで取得**する。だから **CORS の設定が必須**。

## 前提（用意するもの）

- Cloudflare アカウント（無料で始められるが、**R2 の利用には支払い方法の登録**が必要なことが多い。公式の案内に従う）。
- このリポジトリの **`WebGL/Build/`** が、切り替え時点の**最新の Unity WebGL ビルド**になっていること。
- Firebase CLI でデプロイできる環境（`firebase deploy`）。

## 絶対に守る順番（ここを間違えると本番が 404）

1. **先に R2 側を完了する**（バケット・公開 URL・CORS・アップロード・ブラウザで 200 確認まで）。
2. 次に **`WebGL/index.html` の `ASSET_BASE_URL`** に R2 の **pub URL（末尾スラッシュなし）** を書く。
3. そのあと **`firebase.json` の `hosting.ignore` に `Build/**` を追加**する。
4. 最後に **`firebase deploy --only hosting`**。

**逆の順（先に `Build/**` を ignore してしまい、`ASSET_BASE_URL` が空のままデプロイ）**にすると、`Build/` が Firebase にも R2 にも無い状態になり、**ローダーが 404** になる。

**まだ R2 に切り替えないとき**は、`ASSET_BASE_URL` は `""` のまま、`firebase.json` に **`Build/**` を ignore に入れない**。この状態が「従来どおり Firebase だけに `Build/` を載せる」動作。

---

## 手順 1: R2 でバケットを作る

1. Cloudflare ダッシュボードにログインする。
2. 左メニュー **Build** → **Storage & databases** → **R2**。
3. **Create bucket**。名前は分かりやすければよい（例: `necotapu-webgl`）。
4. リージョンは**利用者に近い**ものを選ぶ（日本向けなら APAC 系が無難）。

---

## 手順 2: 公開 URL（r2.dev）を有効にする

1. 作ったバケットを開く。
2. **Settings** またはバケット概要にある **Public access** / **R2.dev subdomain** などの項目で、**パブリックアクセスを有効**にする。
3. 表示される **`https://pub-xxxxxxxx.r2.dev`** のような URL を**メモ**する。これが後で `ASSET_BASE_URL` に入る**ベース**（**末尾に `/` は付けない**）。

独自ドメインは**必須ではない**。まずはこの **pub URL** でよい。

---

## 手順 3: CORS を設定する（必須）

`necotapufb.web.app` でページを開いたまま `r2.dev` の URL からファイルを取ると、ブラウザは**別サイトへのリクエスト**として扱う。**CORS を設定しないと、セキュリティの仕組みでブロック**され、Network タブで赤くなったり、コンソールに `blocked by CORS policy` と出る。

### ダッシュボードでの操作（Objects タブではない）

1. Cloudflare ダッシュボード → 左メニュー **R2**。
2. バケット一覧から **`necotapu-webgl`**（隊長のバケット名）をクリック。
3. 画面上部のタブで **Settings**（**Objects ではない**）を開く。
4. 下の方までスクロールし、**CORS Policy**（または **Cross-Origin Resource Sharing (CORS)** などの見出し）を探す。
5. **JSON を貼り付ける欄**（テキストエリア）がある場合は、**下の「貼り付け用 JSON」ブロックをそのまま全部コピー**して貼る。
6. **Save** / **Save CORS policy** などで保存する。

UI の文言は Cloudflare の更新で変わることがある。**バケット単位の Settings にある CORS** が対象。バケットの **Objects** 画面だけ見ていても CORS は出てこない。

### 貼り付け用 JSON（そのままコピー）

`cloudflare/r2-cors.example.json` と同じ内容:

```json
[
  {
    "AllowedOrigins": [
      "https://necotapufb.web.app",
      "https://necotapufb.firebaseapp.com",
      "https://necotapufb--preview-tvyvb7nk.web.app"
    ],
    "AllowedMethods": ["GET", "HEAD"],
    "AllowedHeaders": ["*"],
    "ExposeHeaders": ["Content-Length", "Content-Type", "ETag"],
    "MaxAgeSeconds": 3600
  }
]
```

- **AllowedOrigins**: ゲームを開くページのオリジン。本番 2 ドメインに加え、**使っているプレビューチャンネルのオリジン**を 1 行ずつ足す（`r2-cors.example.json` と同じ）。
- ローカルで `http://127.0.0.1:5500` などから試すときは、配列に `"http://127.0.0.1:5500"` を**追加**して保存し直す（ポートまで一致させる）。

#### WebGL を R2 から読む構成で、Firebase Hosting のプレビュー URL から開くとき

ページのオリジンが本番（`necotapufb.web.app`）ではなくプレビュー（`https://necotapufb--<チャンネル>-<ランダム>.web.app`）になる。**R2 の CORS と Firebase Authentication の承認済みドメインの両方に、プレビュー用ホストを足す。**

| どこに足すか | 画面の場所 | 入れる値 |
|--------------|------------|----------|
| **R2 の CORS** | Cloudflare ダッシュボード → **R2** → 対象バケット → **Settings** → **CORS Policy** の JSON → 配列 **`AllowedOrigins`** に 1 要素追加 | **オリジン**＝`https://` ＋ホスト名まで（パスなし、末尾 `/` なし）。`firebase hosting:channel:deploy` 実行後の出力 **`Channel URL (necotapufb):` の行に出る `https://…web.app` と文字列完全一致**。 |
| **Firebase Authentication** | [Firebase Console](https://console.firebase.google.com/) → プロジェクト **necotapufb** → **Authentication** → **Settings** タブ → **Authorized domains** → **Add domain** | **ドメイン名のみ**（`https://` なし、`/` なし）。上のオリジンと同じホスト（例: オリジンが `https://necotapufb--preview-abc12345.web.app` なら `necotapufb--preview-abc12345.web.app`）。 |

`Channel URL` が変わったら、上記のプレビュー用の値を新しいものに合わせる。

### 動いたかの見方

`https://necotapufb.web.app` でゲームを開き、開発者ツールの **Console** に **CORS 関連の赤エラーが出ない**こと。出る場合は JSON の typo、保存忘れ、`AllowedOrigins` の URL ミスを疑う。

---

## 手順 4: `Build/` を R2 にアップロードする

### フォルダ構造（重要）

ローカルでは:

```text
WebGL/Build/WebGL.loader.js
WebGL/Build/WebGL.data.unityweb
…
```

R2 上では、**必ず `Build/` というプレフィックス付き**にする:

```text
Build/WebGL.loader.js
Build/WebGL.data.unityweb
…
```

**バケットのルートに `WebGL.loader.js` だけ置く**と、`index.html` が組み立てる URL（`…/Build/WebGL.loader.js`）とずれて **404** になる。

### 圧縮と `Content-Encoding`

いまのビルドは拡張子 **`.unityweb`** で、中身は Unity の設定どおり **Brotli 等で圧縮**されている。ブラウザが正しく展開するには、オブジェクトに応じて **HTTP レスポンスに `Content-Encoding: br`** が付く必要がある（付かないと「壊れたファイル」「圧縮の誤設定」系のエラーになりやすい）。

- **S3 互換 API**や **Wrangler** でアップロードする場合: オブジェクトのメタデータに **`ContentEncoding`（または相当する項目）を `br`** と指定できるかドキュメントを確認する。
- **ダッシュボードからアップロード**する場合: オブジェクトごとに **Custom metadata / HTTP ヘッダ**を設定できる UI があれば同様に設定する。

アップロード方法は環境によるため、「自分流のツールで **キーが `Build/...` で、必要なら `Content-Encoding: br`**」が満たせればよい。

**このリポジトリでは**、リポジトリルートから **`./cloudflare/upload-build-to-r2.sh`** を実行すると、Wrangler で `Build/` 以下 4 ファイルを上げ、`*.unityweb` に `Content-Encoding: br` を付ける（事前に `wrangler login` などで認証できること）。

### 動作確認（Firebase をまだ変える前に）

ブラウザの**別タブ**で、次のような URL を直接開く（`pub-…` とファイル名は実際の値に合わせる）:

- `https://pub-xxxxxxxx.r2.dev/Build/WebGL.loader.js`

**200** で中身が返れば、まずはパスと公開設定は通っている。  
`.unityweb` を開いたとき、開発者ツールの **Network** で **Response Headers** に `content-encoding: br` が付いているかも確認する。

---

## 手順 5: `index.html` の `ASSET_BASE_URL` を設定する

ファイル: **`WebGL/index.html`**

次の変数を、手順 2 でメモした **pub URL（末尾スラッシュなし）** に変更する。

```javascript
var ASSET_BASE_URL = "https://pub-xxxxxxxx.r2.dev";
```

- 空文字 `""` のままだと、**同一オリジン**の `Build/` を見に行く（= Firebase Hosting 上の `Build/`）。R2 だけに載せる運用では**空のままにしない**。

コミット・デプロイ前に、**誤字がないか**（`https://`、ホスト名、`r2.dev`）を確認する。

---

## 手順 6: `firebase.json` に `Build/**` を ignore に追加する

ファイル: **`firebase.json`**

`hosting.ignore` の配列に **`"Build/**"`** を含める（このリポジトリでは既に含めてある）。

これで **`firebase deploy` 時に `WebGL/Build/` が Hosting に上がらなくなる**。  
**手順 5 が終わってから**追加すること（順番の節参照）。

---

## 手順 7: Firebase にデプロイする

プロジェクトルートで:

```bash
firebase deploy --only hosting
```

デプロイ後、**本番 URL**（`https://necotapufb.web.app/` など）で開き、開発者ツールの **Network** で:

- `WebGL.loader.js` や `*.unityweb` の **Request URL が `r2.dev` 向き**になっているか
- **ステータスが 200** か
- **CORS エラーがない**か

を確認する。

---

## 成功の目安

- ゲームが**これまで通りタイトルまで起動**する。
- Firebase コンソールの **Hosting の転送量**が、切り替え前より**大きく減る**（利用状況による）。

---

## 失敗したときの典型と見る場所

| 現象 | よくある原因 |
|------|----------------|
| `Build/...` が **404** | R2 上のパスが `Build/` 付きでない、`ASSET_BASE_URL` の typo、まだ `Build/**` を ignore しただけで URL 未設定。 |
| **CORS エラー** | R2 の CORS に `web.app` / `firebaseapp.com` が無い、HTTPS と HTTP の取り違え。 |
| **`Invalid or unexpected token`（.unityweb 読み込み時）** | `Content-Encoding: br` が付いていない、または誤った圧縮指定。 |
| 画面は出るが重い／おかしい | 古い `Build/` を R2 に上げている、キャッシュ。`cacheBust` のバージョンを上げる・ハードリロード。 |

---

## 元に戻す（ロールバック）

1. `firebase.json` から **`Build/**` の ignore 行を削除**する。
2. `WebGL/index.html` の **`ASSET_BASE_URL` を `""` に戻す**。
3. ローカルの **`WebGL/Build/`** を最新にしたうえで **`firebase deploy --only hosting`**（`Build/` が再び Hosting に載る）。

---

## 参考ファイル

- CORS の例: **`cloudflare/r2-cors.example.json`**
- Cloudflare R2 の料金・無料枠: **Cloudflare 公式ドキュメント**（変更されうるため、都度確認）。
