const functions = require("firebase-functions");
const admin = require("firebase-admin");

admin.initializeApp();
const db = admin.firestore();

// ステージごとのランキング人数制限
const RANKING_LIMIT = 150;

// 奈良県e-net用の市町村グルーピング定義
const NARA_CITY_GROUPS = {
  "City_MY": [
    "hdm", "htz", "rmj", "fgn", "gwg", 
    "phd", "gtg", "bkm", "gnr", "msg", 
    "mnw", "hjn", "nkf", "vbw", "gdw", 
    "jcs", "rvy", "wfs", "cfr", "kht"
  ],
};

// メールアドレスからランキンググループ名（キー）を決定する関数
function getRankingGroupKey(email) {
  if (!email) return "unknown";

  const lowerEmail = email.toLowerCase();

  // 1. 奈良県e-netドメインの場合
  if (lowerEmail.endsWith("@e-net.nara.jp")) {
    const prefix = lowerEmail.substring(0, 3); // 先頭3文字

    // 定義表から検索
    for (const [cityName, prefixes] of Object.entries(NARA_CITY_GROUPS)) {
      if (prefixes.includes(prefix)) {
        return `Nara_${cityName}`; // 例: Nara_City_MY
      }
    }
    // 定義にない場合は "Nara_Other"
    return "Nara_Other";
  }

  // 2. それ以外の場合：ドメイン名をそのままグループキーにする
  const parts = lowerEmail.split("@");
  if (parts.length === 2) {
    return parts[1]; // 例: gmail.com
  }

  return "unknown";
}

// ランキングエントリーデータを作成するヘルパー関数
function createRankEntry(doc) {
  const userData = doc.data();
  const data = userData.data; // SaveData構造
  if (!data || !data.Status) return null;

  return {
    Uid: doc.id,
    FirstName: data.UserName || "名無し",
    Kpm: data.Status[3] || 0,
    NicknameNo: (data.Equipment && data.Equipment.length > 6) ? data.Equipment[6] : 0,
    RightHand: (data.Equipment && data.Equipment.length > 0) ? data.Equipment[0] : 0,
    Head: (data.Equipment && data.Equipment.length > 1) ? data.Equipment[1] : 0,
    Glasses: (data.Equipment && data.Equipment.length > 2) ? data.Equipment[2] : 0,
    LeftHand: (data.Equipment && data.Equipment.length > 3) ? data.Equipment[3] : 0,
    CatBody: (data.Equipment && data.Equipment.length > 4) ? data.Equipment[4] : 0,
    CatFace: (data.Equipment && data.Equipment.length > 5) ? data.Equipment[5] : 0,
    
    // マージ判定用
    UpdatedAt: userData.updatedAt ? userData.updatedAt.toDate() : new Date(0)
  };
}

// テスト用：ブラウザから叩ける手動実行関数
exports.testDailyRanking = functions.https.onRequest(async (req, res) => {
  try {
    // テスト時は「昨日以降」ではなく「全期間」を対象にするオプションなどを渡せますが、
    // 今回はロジック通り「昨日以降の更新」のみ処理します。
    // ※もし強制的に全再構築したい場合はロジック内のフラグをいじってください。
    await runDailyRankingLogic();
    res.send("バッチ実行完了！Firestoreを確認してください。");
  } catch (e) {
    console.error(e);
    res.status(500).send(`Error: ${e.message}`);
  }
});

// 本番用：毎日AM4時に実行されるスケジューラー
exports.updateDailyRanking = functions.pubsub
  .schedule("0 4 * * *")
  .timeZone("Asia/Tokyo")
  .onRun(async (context) => {
    await runDailyRankingLogic();
    return null;
  });

// ランキング集計ロジック本体（差分更新方式）
async function runDailyRankingLogic() {
  console.log("ランキング集計バッチ開始（差分更新モード）");

  // 1. 既存の全ランキングリストを取得 (rankingsコレクション全件)
  // ※ステージ数分 (M Read)
  const rankingsSnapshot = await db.collection("rankings").get();
  
  // 既存リストをメモリに展開
  // Map<DocID, { list: entry[], groupKey: string, stageId: number }>
  const existingRankings = new Map();

  rankingsSnapshot.forEach(doc => {
    const data = doc.data();
    if (data.rankingList) {
      existingRankings.set(doc.id, {
        list: data.rankingList,
        groupKey: data.groupKey,
        stageId: data.stageId
      });
    }
  });

  console.log(`既存ランキング取得: ${existingRankings.size} ステージ分`);

  // 2. 「昨日更新されたユーザー」を取得 (Active User Only)
  // 基準: 現在時刻 - 24時間 (余裕を見て25時間とかでも良い)
  const yesterday = new Date();
  yesterday.setHours(yesterday.getHours() - 25); 

  // 初回起動判定: もし既存ランキングが0件なら、全ユーザーを取得する (Full Build)
  let isFullBuild = (existingRankings.size === 0);
  let usersQuery = db.collection("users");
  
  if (!isFullBuild) {
    console.log(`差分更新モード: ${yesterday.toISOString()} 以降の更新データを取得`);
    usersQuery = usersQuery.where('updatedAt', '>', yesterday);
  } else {
    console.log("初回構築モード: 全ユーザーを取得");
  }

  const usersSnapshot = await usersQuery.get();
  console.log(`更新対象ユーザー数: ${usersSnapshot.size}件`);

  if (usersSnapshot.empty && !isFullBuild) {
    console.log("更新対象なし。終了します。");
    return;
  }

  // 3. 取得したユーザーデータをエントリー形式に変換し、グループごとに分類
  // Map<GroupKey, Map<Uid, Entry>>
  const updatedEntriesByGroup = new Map();

  usersSnapshot.forEach(doc => {
    const entry = createRankEntry(doc);
    if (!entry) return;

    // グループキー判定 (SaveData内のEmailから)
    const userData = doc.data().data || {};
    const groupKey = getRankingGroupKey(userData.Email);

    if (!updatedEntriesByGroup.has(groupKey)) {
      updatedEntriesByGroup.set(groupKey, new Map());
    }
    updatedEntriesByGroup.get(groupKey).set(entry.Uid, entry);
  });

  // 4. マージ処理 & 保存
  const batch = db.batch();
  let writeCount = 0;
  // ※500件超え対策は簡易的に省略しています。運用時は注意。

  // A. 既存リストがあるグループ: リスト内を更新
  // B. 新規グループ: 新しくリスト作成

  // まず、影響を受ける可能性のある全てのグループキーをリストアップ
  const allGroupKeys = new Set([...updatedEntriesByGroup.keys()]);
  
  // 既存ランキングからもグループキーを収集
  existingRankings.forEach((val) => allGroupKeys.add(val.groupKey));

  const allBorderlines = {}; // ボーダーライン情報

  for (const groupKey of allGroupKeys) {
    // このグループの全エントリーを収集するためのマップ
    // Map<Uid, Entry>
    const groupAllEntries = new Map();

    // a) 既存リストからエントリーを展開 (古いデータ)
    existingRankings.forEach((val, docId) => {
      if (val.groupKey === groupKey) {
        val.list.forEach(entry => {
          groupAllEntries.set(entry.Uid, entry);
        });
      }
    });

    // b) 今回更新されたエントリーで上書き (新しいデータ)
    if (updatedEntriesByGroup.has(groupKey)) {
      updatedEntriesByGroup.get(groupKey).forEach((entry, uid) => {
        groupAllEntries.set(uid, entry);
      });
    }

    // c) 配列に戻してソート & ステージ分割
    let entries = Array.from(groupAllEntries.values());
    entries.sort((a, b) => b.Kpm - a.Kpm); // KPM降順

    // ステージ分割保存
    let stageIndex = 0;
    const groupBorders = {};

    // もしエントリーが空ならスキップ
    if (entries.length === 0) continue;

    for (let i = 0; i < entries.length; i += RANKING_LIMIT) {
      const stageEntries = entries.slice(i, i + RANKING_LIMIT);
      
      // 順位更新
      stageEntries.forEach((entry, idx) => {
        entry.Ranking = i + idx + 1;
      });

      const docId = `${groupKey}_stage_${stageIndex}`;
      const docRef = db.collection("rankings").doc(docId);

      batch.set(docRef, {
        rankingList: stageEntries,
        updatedAt: admin.firestore.FieldValue.serverTimestamp(),
        groupKey: groupKey,
        stageId: stageIndex
      });
      writeCount++;

      // ボーダーライン更新
      const lastEntry = stageEntries[stageEntries.length - 1];
      groupBorders[stageIndex] = lastEntry ? lastEntry.Kpm : 0;

      stageIndex++;
    }

    allBorderlines[groupKey] = groupBorders;
  }

  // 5. ボーダーライン情報の保存
  for (const [groupKey, borders] of Object.entries(allBorderlines)) {
    const borderDocRef = db.collection("system").doc(`ranking_borders_${groupKey}`);
    batch.set(borderDocRef, {
      borders: borders,
      updatedAt: admin.firestore.FieldValue.serverTimestamp()
    });
    writeCount++;
  }

  // コミット
  await batch.commit();
  console.log(`ランキング集計完了: ${writeCount}件の書き込みを実行しました。`);
}
