# Firestore保存関数一覧表

## 概要
Firestoreにデータを保存する関数の呼び出しフローと役割を整理した一覧表です。

---

## 保存関数の分類

### 1. 初期データ保存（制限なし）
**用途**: 初回ログイン時や新規ユーザー作成時に使用。23時間制限を無視して保存。

#### 呼び出しフロー
```
TitleSky.confirmNeco()
  └─> SaveData.saveInitialDataToFirebase()
      └─> FirestoreConnection.SaveInitialData()
          └─> [JSLib] SaveToFirestoreJslib()
              └─> [JS] window.saveToFirestoreFromUnity()
                  └─> Firestore: users/{uid} に保存
```

#### 関数詳細

| レイヤー | 関数名 | ファイル | 行番号 | 説明 |
|---------|--------|----------|--------|------|
| **Unity C#** | `TitleSky.confirmNeco()` | `Assets/Script/TitleSky.cs` | 721 | ねこ選択確定時に呼び出し |
| **Unity C#** | `TitleSky.OnAuthSuccess()` | `Assets/Script/TitleSky.cs` | 519 | ユーザー情報更新時に呼び出し |
| **Unity C#** | `SaveData.saveInitialDataToFirebase()` | `Assets/Script/SaveData.cs` | 334 | 初期データをJSON化してFirestoreConnectionに渡す |
| **Unity C#** | `FirestoreConnection.SaveInitialData()` | `Assets/Script/FirestoreConnection.cs` | 348 | 制限なしでJSLibに保存要求 |
| **JSLib** | `SaveToFirestoreJslib()` | `Assets/Plugins/jslib/FirebaseBridge.jslib` | 98 | JavaScript側の`saveToFirestoreFromUnity`を呼び出し |
| **JavaScript** | `window.saveToFirestoreFromUnity()` | `WebGL/index.html` | 843 | Firestoreに`users/{uid}`ドキュメントを保存 |

---

### 2. 統合データ保存（制限付き・従量課金対策）
**用途**: ゲーム中のデータ保存。**従量課金対策で統合保存パスに統一済み**。ユーザーデータとランキングデータをまとめて保存。23時間制限あり。

#### 呼び出しフロー
```
GameManager.saveGameData()
  └─> FirestoreConnection.SaveCombinedDataToFB()
      └─> [JSLib] SaveCombinedDataToFBJslib()
          └─> [JS] window.saveCombinedDataToFBFromUnity()
              └─> Firestore: users/{uid} にバッチ書き込み
```

#### 関数詳細

| レイヤー | 関数名 | ファイル | 行番号 | 説明 |
|---------|--------|----------|--------|------|
| **Unity C#** | `GameManager.saveGameData()` | `Assets/Script/GameManager.cs` | 712 | タイピング後、インベントリ変更後、設定変更後などに呼び出し |
| **Unity C#** | `FirestoreConnection.SaveCombinedDataToFB()` | `Assets/Script/FirestoreConnection.cs` | 398 | 統合データをJSLibに保存要求 |
| **JSLib** | `SaveCombinedDataToFBJslib()` | `Assets/Plugins/jslib/FirebaseBridge.jslib` | 501 | 23時間制限をチェックしてからJavaScript側を呼び出し |
| **JavaScript** | `window.saveCombinedDataToFBFromUnity()` | `WebGL/index.html` | 767 | Firestoreに`users/{uid}`ドキュメントをバッチ書き込み |

**呼び出し箇所**:
- `Player.cs` (146行目): タイピング後のデータ保存
- `TypingSoft.cs` (1094行目): タイピング終了時
- `TypingRoom.cs` (133行目): ゆびモード開始時
- `GameManager.cs` (594行目): インベントリ変更後
- `GameManager.cs` (891行目): ゆびモード回数リセット時
- `Setting.cs` (102行目): 設定変更時

---

### 3. 廃止予定の関数（参考用）

#### `SaveToFirestore()` - 使用されていない
**理由**: 従量課金対策で統合保存パス（`SaveCombinedDataToFB()`）に統一されたため、現在は使用されていません。

| レイヤー | 関数名 | ファイル | 行番号 | 説明 |
|---------|--------|----------|--------|------|
| **Unity C#** | `FirestoreConnection.SaveToFirestore()` | `Assets/Script/FirestoreConnection.cs` | 90 | 定義されているが、呼び出し箇所なし |
| **JSLib** | `SaveToFirestoreWithLimitJslib()` | `Assets/Plugins/jslib/FirebaseBridge.jslib` | 460 | 定義されているが、呼び出し箇所なし |

---

## 各レイヤーの役割

### Unity C# レイヤー
- データのシリアライズ（JSON化）
- 保存タイミングの制御
- エラーハンドリング

### JSLib レイヤー（ブリッジ）
- UnityとJavaScript間のデータ受け渡し
- 23時間制限のチェック（`SaveToFirestoreWithLimitJslib`、`SaveCombinedDataToFBJslib`）
- エラー時のUnityへの通知

### JavaScript レイヤー
- Firebase SDKを使用したFirestoreへの実際の書き込み
- 認証状態の確認
- バッチ書き込みの実行（統合データ保存の場合）

---

## 保存先

すべての保存関数は、Firestoreの以下のパスに保存します：
```
users/{user.uid}
```

保存されるデータ構造：
```json
{
  "data": {
    // SaveDataの全フィールド（Email, UserName, Gold, Stage, etc.）
  },
  "updatedAt": serverTimestamp(),
  "createdAt": serverTimestamp() // 新規作成時のみ
}
```

---

## 制限時間の設定

| 関数 | 制限時間の設定場所 | デフォルト値 |
|------|-------------------|------------|
| `SaveInitialData()` | **制限なし** | - |
| `SaveToFirestore()` | `FirestoreConnection.firebaseAccessLimitHoursLoad` | 1時間 |
| `SaveCombinedDataToFB()` | `FirestoreConnection.firebaseAccessLimitHoursSave` | 1時間 |

**注意**: `SaveToFirestore()`は`firebaseAccessLimitHoursLoad`を使用していますが、これはバグの可能性があります。

---

## 呼び出し箇所のまとめ

### 初期データ保存（`saveInitialDataToFirebase`）
1. **`TitleSky.confirmNeco()`** (721行目)
   - ねこ選択確定時
   - 新規ユーザー作成時

2. **`TitleSky.OnAuthSuccess()`** (519行目)
   - ユーザー情報（Email, FirstName, LastName）が更新された時

### 統合データ保存（`SaveCombinedDataToFB`）
1. **`GameManager.saveGameData()`** (712行目)
   - タイピング後のデータ保存（`Player.cs` 146行目）
   - タイピング終了時（`TypingSoft.cs` 1094行目）
   - ゆびモード開始時（`TypingRoom.cs` 133行目）
   - インベントリ変更後（`GameManager.cs` 594行目）
   - ゆびモード回数リセット時（`GameManager.cs` 891行目）
   - 設定変更時（`Setting.cs` 102行目）
   - Firebaseから正常にロードできた場合のみ実行（`GameManager.cs` 747行目でチェック）

---

## 問題点と注意事項

1. **従量課金対策による統合保存への統一**
   - ✅ 通常のゲームデータ保存は`SaveCombinedDataToFB()`に統一済み
   - ⚠️ `SaveToFirestore()`は定義されているが使用されていない（統合保存に統一されたため）
   - ⚠️ 初期データ保存（`SaveInitialData()`）はまだ古いパスを使用（制限なしで保存）

2. **初期データ保存のタイミング**
   - `confirmNeco()`で保存されるが、その時点で`ouText.text`が空の可能性がある
   - `authInfo.email`や`authInfo.displayName`が`null`の場合、空のデータが保存される可能性

3. **重複保存の可能性**
   - `OnAuthSuccess()`と`confirmNeco()`の両方で保存される可能性がある

---

## 関連ファイル

- `Assets/Script/SaveData.cs` - データ構造とシリアライズ
- `Assets/Script/FirestoreConnection.cs` - Firebase接続管理
- `Assets/Script/TitleSky.cs` - タイトル画面と認証処理
- `Assets/Script/GameManager.cs` - ゲーム管理と保存処理
- `Assets/Plugins/jslib/FirebaseBridge.jslib` - Unity-JavaScriptブリッジ
- `WebGL/index.html` - JavaScript側のFirestore操作
