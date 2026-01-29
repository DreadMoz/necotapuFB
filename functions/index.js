const functions = require("firebase-functions");
const admin = require("firebase-admin");

// レガシーデータマージ用
let legacyData = null;
try {
  legacyData = require('./legacy_data.json');
} catch (e) {
  console.warn("legacy_data.json not found. Merge function will be disabled.");
}

admin.initializeApp();
const db = admin.firestore();

// ... (existing code) ...

// ユーザーデータバージョン比較関数
// ... (existing code) ...

/**
 * 旧バージョン（スプレッドシート版）からのデータマージ
 */
exports.mergeLegacyData = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'ログインが必要です。');
  }

  if (!legacyData) {
    return { status: 'skipped', reason: 'no_legacy_data' };
  }

  const uid = context.auth.uid;
  const email = context.auth.token.email ? context.auth.token.email.toLowerCase() : null;

  if (!email || !legacyData[email]) {
    return { status: 'skipped', reason: 'user_not_found_in_legacy' };
  }

  const userDocRef = db.collection('users').doc(uid);
  const userDoc = await userDocRef.get();

  if (!userDoc.exists) {
    return { status: 'error', reason: 'user_doc_not_found' };
  }

  const userData = userDoc.data();
  if (userData.isLegacyMerged) {
    return { status: 'already_merged' };
  }

  const legacy = legacyData[email];
  const currentData = userData.data || {};

  // 1. シーカー（ゴールド）のマージ
  if (!currentData.Status) currentData.Status = [0, 0, 0, 0];
  currentData.Status[0] = (currentData.Status[0] || 0) + legacy.s;

  // 2. アイテムのマージ
  if (!currentData.Items) currentData.Items = new Array(256).fill(false);
  
  // legacy.i は [Item1, Item2, Item3, Item4] (文字列化されたulong)
  for (let i = 0; i < legacy.i.length; i++) {
    const val = BigInt(legacy.i[i]);
    for (let bit = 0; bit < 64; bit++) {
      if ((val & (1n << BigInt(bit))) !== 0n) {
        const itemIdx = i * 64 + bit;
        if (itemIdx < currentData.Items.length) {
          currentData.Items[itemIdx] = true;
        }
      }
    }
  }

  // 保存
  await userDocRef.set({
    data: currentData,
    isLegacyMerged: true,
    updatedAt: admin.firestore.FieldValue.serverTimestamp()
  }, { merge: true });

  console.log(`✅ User ${email} (${uid}) merged legacy data: +${legacy.s} gold`);
  return { status: 'success', mergedGold: legacy.s };
});


// ステージごとのランキング人数制限
const RANKING_LIMIT = 150;

// 奈良県e-net用の市町村グルーピング定義（nara_config.jsonから読み込む）
let NARA_CITY_GROUPS = {};
let TRANSFER_STUDENTS = {};
let TEACHERS = {};

/**
 * 設定ファイルを読み込む
 */
async function loadNaraConfig() {
  try {
    // Functions環境ではファイルシステムから直接読み込む
    const fs = require('fs');
    const path = require('path');
    const configPath = path.join(__dirname, 'nara_config.json');
    if (fs.existsSync(configPath)) {
      const config = JSON.parse(fs.readFileSync(configPath, 'utf8'));
      NARA_CITY_GROUPS = config.NARA_CITY_GROUPS;
      TRANSFER_STUDENTS = config.TRANSFER_STUDENTS;
      TEACHERS = config.TEACHERS;
      console.log("Nara config loaded successfully from file");
    } else {
      console.warn("nara_config.json not found, using empty config");
    }
  } catch (e) {
    console.error("Failed to load Nara config:", e);
  }
}

// 初期化時に読み込み
loadNaraConfig();

// メールアドレスからランキンググループ名（キー）を決定する関数
function getRankingGroupKey(email) {
  if (!email) return "unknown";

  const lowerEmail = email.toLowerCase();

  // 0. 先生リストのチェック
  if (TEACHERS[lowerEmail]) {
    return `Nara_${TEACHERS[lowerEmail]}`;
  }

  // 0.5 転校生（例外リスト）のチェック
  if (TRANSFER_STUDENTS[lowerEmail]) {
    return `Nara_${TRANSFER_STUDENTS[lowerEmail]}`;
  }

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

  // 1.5 奈良県e-netドメイン（サブドメインあり）の場合
  if (lowerEmail.endsWith(".e-net.nara.jp")) {
    const parts = lowerEmail.split("@");
    if (parts.length === 2) {
      const domainParts = parts[1].split(".");
      // [prefix].e-net.nara.jp の形式を想定
      if (domainParts.length >= 4) {
        const prefix = domainParts[0];
        for (const [cityName, prefixes] of Object.entries(NARA_CITY_GROUPS)) {
          if (prefixes.includes(prefix)) {
            return `Nara_${cityName}`;
          }
        }
      }
    }
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

// ランキング集計ロジック本体（曜日による分岐あり）
async function runDailyRankingLogic() {
  const now = new Date();
  // JSTにするために9時間足す（Firebase Functionsのサーバー時間はUTC）
  // タイムゾーン考慮が必要だが、簡易的にUTC+9hの曜日で判定
  const jstNow = new Date(now.getTime() + 9 * 60 * 60 * 1000);
  const dayOfWeek = jstNow.getUTCDay(); // 0:Sun, 1:Mon, ..., 6:Sat

  // 月曜日(1)ならWeekly組み換え、それ以外はDaily更新
  const isWeeklyRebuild = (dayOfWeek === 1);
  const modeName = isWeeklyRebuild ? "Weekly組み換え（フル再構築）" : "Daily更新（情報のみ更新）";
  
  console.log(`ランキング集計バッチ開始: ${modeName} - 曜日:${dayOfWeek}`);

  // 1. 既存の全ランキングリストを取得 (rankingsコレクション全件)
  // ※ステージ数分 (M Read)
  const rankingsSnapshot = await db.collection("rankings").get();
  
  // 既存リストをメモリに展開
  // Map<DocID, { list: entry[], groupKey: string, stageId: number }>
  const existingRankings = new Map();

  // ユーザーがどのステージにいるかを高速検索するためのマップ
  // Map<Uid, { groupKey: string, stageId: number, docId: string }>
  const userLocationMap = new Map();

  rankingsSnapshot.forEach(doc => {
    const data = doc.data();
    if (data.rankingList) {
      existingRankings.set(doc.id, {
        list: data.rankingList,
        groupKey: data.groupKey,
        stageId: data.stageId
      });

      // ユーザー位置情報を記録
      data.rankingList.forEach(entry => {
        userLocationMap.set(entry.Uid, {
          groupKey: data.groupKey,
          stageId: data.stageId,
          docId: doc.id
        });
      });
    }
  });

  console.log(`既存ランキング取得: ${existingRankings.size} ステージ分`);

  // 2. 「昨日更新されたユーザー」を取得 (Active User Only)
  // 基準: 現在時刻 - 24時間 (余裕を見て25時間)
  const yesterday = new Date();
  yesterday.setHours(yesterday.getHours() - 25); 

  // 初回起動判定: もし既存ランキングが0件なら、強制的にWeeklyモード（全取得）にする
  let isFullBuild = (existingRankings.size === 0);
  if (isFullBuild) {
    console.log("初回構築のため、強制的にフル再構築モードで実行します。");
  }

  let usersQuery = db.collection("users");
  
  if (!isFullBuild) {
    console.log(`差分取得: ${yesterday.toISOString()} 以降の更新データを取得`);
    usersQuery = usersQuery.where('updatedAt', '>', yesterday);
  } else {
    console.log("全ユーザーを取得します");
  }

  const usersSnapshot = await usersQuery.get();
  console.log(`更新対象ユーザー数: ${usersSnapshot.size}件`);

  if (usersSnapshot.empty && !isFullBuild) {
    console.log("更新対象なし。終了します。");
    return;
  }

  const batch = db.batch();
  let writeCount = 0;
  const allBorderlines = {}; // ボーダーライン情報

  if (isWeeklyRebuild || isFullBuild) {
    // ==========================================
    // A. Weekly Mode: 全員まとめて再ソート＆再分配
    // ==========================================
    
    // Map<GroupKey, Map<Uid, Entry>>
    const mergedEntriesByGroup = new Map();

    // 1. 既存データをグループごとに展開
    existingRankings.forEach((val) => {
      if (!mergedEntriesByGroup.has(val.groupKey)) {
        mergedEntriesByGroup.set(val.groupKey, new Map());
      }
        val.list.forEach(entry => {
        mergedEntriesByGroup.get(val.groupKey).set(entry.Uid, entry);
        });
    });

    // 2. 更新データをマージ（グループ移動対応）
    usersSnapshot.forEach(doc => {
      const entry = createRankEntry(doc);
      if (!entry) return;
      
      const userData = doc.data().data || {};
      const newGroupKey = getRankingGroupKey(userData.Email);
      
      // 既存の場所を探す
      const oldLocation = userLocationMap.get(entry.Uid);
      
      if (oldLocation && oldLocation.groupKey !== newGroupKey) {
        // グループが変わった場合、古いグループから削除
        console.log(`Weekly: ユーザー ${entry.Uid} がグループ移動: ${oldLocation.groupKey} -> ${newGroupKey}`);
        const oldGroupEntries = mergedEntriesByGroup.get(oldLocation.groupKey);
        if (oldGroupEntries) {
          oldGroupEntries.delete(entry.Uid);
        }
      }
      
      // 新しいグループに追加/更新
      if (!mergedEntriesByGroup.has(newGroupKey)) {
        mergedEntriesByGroup.set(newGroupKey, new Map());
      }
      mergedEntriesByGroup.get(newGroupKey).set(entry.Uid, entry);
    });

    // 3. マージ処理と保存
    const allGroupKeys = Array.from(mergedEntriesByGroup.keys());

    for (const groupKey of allGroupKeys) {
      const groupEntries = mergedEntriesByGroup.get(groupKey);
      if (!groupEntries || groupEntries.size === 0) continue;

      // 配列化＆ソート
      let entries = Array.from(groupEntries.values());
      entries.sort((a, b) => b.Kpm - a.Kpm); // KPM降順

      // ステージ分割保存
      let stageIndex = 0;
      const groupBorders = {};

      for (let i = 0; i < entries.length; i += RANKING_LIMIT) {
        const stageEntries = entries.slice(i, i + RANKING_LIMIT);
        
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

        const lastEntry = stageEntries[stageEntries.length - 1];
        groupBorders[stageIndex] = lastEntry ? lastEntry.Kpm : 0;
        stageIndex++;
      }
      allBorderlines[groupKey] = groupBorders;
    }

  } else {
    // ==========================================
    // B. Daily Mode: 情報更新のみ（グループ移動対応）
    // ==========================================
    
    usersSnapshot.forEach(doc => {
      const newEntry = createRankEntry(doc);
      if (!newEntry) return;

      const userData = doc.data().data || {};
      const newGroupKey = getRankingGroupKey(userData.Email);
      const location = userLocationMap.get(newEntry.Uid);
      
      if (location) {
        // 既存ユーザー
        if (location.groupKey !== newGroupKey) {
          // 【重要】グループが変わった場合、古いグループから削除し、新しいグループの末尾に追加
          console.log(`Daily: ユーザー ${newEntry.Uid} がグループ移動を検知: ${location.groupKey} -> ${newGroupKey}`);
          
          // 旧グループから削除
          const oldStageData = existingRankings.get(location.docId);
          if (oldStageData) {
            oldStageData.list = oldStageData.list.filter(e => e.Uid !== newEntry.Uid);
            oldStageData.isDirty = true;
          }
          
          // 新グループの最終ステージを探す
          let targetDocId = null;
          let maxStageId = -1;
          existingRankings.forEach((val, docId) => {
            if (val.groupKey === newGroupKey && val.stageId > maxStageId) {
              maxStageId = val.stageId;
              targetDocId = docId;
            }
          });

          if (targetDocId) {
            const newStageData = existingRankings.get(targetDocId);
            newEntry.Ranking = 9999;
            newStageData.list.push(newEntry);
            newStageData.isDirty = true;
          } else {
            // 新グループがまだ存在しない場合
            const newDocId = `${newGroupKey}_stage_0`;
            newEntry.Ranking = 1;
            existingRankings.set(newDocId, {
              list: [newEntry],
              groupKey: newGroupKey,
              stageId: 0,
              isDirty: true
            });
          }
        } else {
          // 同一グループ内の更新
          const stageData = existingRankings.get(location.docId);
          if (stageData) {
            const list = stageData.list;
            const idx = list.findIndex(e => e.Uid === newEntry.Uid);
            if (idx !== -1) {
              const originalRanking = list[idx].Ranking;
              list[idx] = newEntry;
              list[idx].Ranking = originalRanking;
              stageData.isDirty = true;
            }
          }
        }
      } else {
        // 完全な新規ユーザー（一番下のステージに追加）
        let targetDocId = null;
        let maxStageId = -1;
        existingRankings.forEach((val, docId) => {
          if (val.groupKey === newGroupKey && val.stageId > maxStageId) {
            maxStageId = val.stageId;
            targetDocId = docId;
          }
        });

        if (targetDocId) {
          const stageData = existingRankings.get(targetDocId);
          newEntry.Ranking = 9999;
          stageData.list.push(newEntry);
          stageData.isDirty = true;
        } else {
          const newDocId = `${newGroupKey}_stage_0`;
          newEntry.Ranking = 1;
          existingRankings.set(newDocId, {
            list: [newEntry],
            groupKey: newGroupKey,
            stageId: 0,
            isDirty: true
          });
        }
      }
    });

    // 変更があったステージのみ保存
    existingRankings.forEach((val, docId) => {
      if (val.isDirty) {
        const docRef = db.collection("rankings").doc(docId);
        batch.set(docRef, {
          rankingList: val.list,
          updatedAt: admin.firestore.FieldValue.serverTimestamp(),
          groupKey: val.groupKey,
          stageId: val.stageId
        });
        writeCount++;
        
        if (!allBorderlines[val.groupKey]) allBorderlines[val.groupKey] = {};
        const lastEntry = val.list[val.list.length - 1];
        allBorderlines[val.groupKey][val.stageId] = lastEntry ? lastEntry.Kpm : 0;
      }
    });
  }

  // 5. ボーダーライン情報の保存
  for (const [groupKey, borders] of Object.entries(allBorderlines)) {
    const borderDocRef = db.collection("system").doc(`ranking_borders_${groupKey}`);
    batch.set(borderDocRef, {
      borders: borders,
      updatedAt: admin.firestore.FieldValue.serverTimestamp()
    }, { merge: true });
    writeCount++;
  }

  // コミット
  await batch.commit();
  console.log(`ランキング集計完了(${modeName}): ${writeCount}件の書き込みを実行しました。`);
}
