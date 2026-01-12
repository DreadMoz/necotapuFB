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
  
  // FullBuild(初回) または WeeklyRebuild の場合は全件取得のほうが安全だが、
  // コスト削減のためWeeklyでも「差分更新＋再ソート」で対応可能か検討。
  // しかし「1週間ログインしていないが、先週KPMを更新した人」などが漏れると困る。
  // → ここではシンプルに「Active Userのみ更新」を貫く。
  // 引退ユーザーは順位が下がっていくだけ（Weeklyで再ソートされるため）
  
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

  // 3. 更新データの処理
  const updatedEntries = [];
  usersSnapshot.forEach(doc => {
    const entry = createRankEntry(doc);
    if (entry) updatedEntries.push(entry);
  });

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

    // 2. 更新データをマージ（グループキーも最新情報から再判定）
    updatedEntries.forEach(entry => {
      // ユーザーの最新グループキーを取得（メアドから判定）
      // ※注意: createRankEntryではemailが含まれていないため、docから再取得必要
      // ここでは簡易的に、更新データのentryにはemailが含まれていないため、
      // usersSnapshotループ内でグループキー判定を行う必要があります。
      // リファクタリング: createRankEntryにEmailを含めるか、ここでdocを参照する。
      // 今回はusersSnapshotループ内で処理済みとして、updatedEntriesをMapにするのが良いが、
      // 既存コードの流れを変えるため、updatedEntries作成時にグループキーも持たせる形に微修正します。
    });
    
    // ※上記のループ修正のため、ロジックを少し戻します。
    // Map<GroupKey, Map<Uid, Entry>>
    const updatesByGroup = new Map();
    usersSnapshot.forEach(doc => {
      const entry = createRankEntry(doc);
      if (!entry) return;
      const userData = doc.data().data || {};
      const groupKey = getRankingGroupKey(userData.Email);
      if (!updatesByGroup.has(groupKey)) updatesByGroup.set(groupKey, new Map());
      updatesByGroup.get(groupKey).set(entry.Uid, entry);
    });

    // マージ処理
    // 全グループキーを収集
    const allGroupKeys = new Set([...mergedEntriesByGroup.keys(), ...updatesByGroup.keys()]);

    for (const groupKey of allGroupKeys) {
      const groupEntries = mergedEntriesByGroup.get(groupKey) || new Map();
      const updates = updatesByGroup.get(groupKey);

      // 更新分で上書き
      if (updates) {
        updates.forEach((entry, uid) => {
          groupEntries.set(uid, entry);
      });
    }

      // 配列化＆ソート
      let entries = Array.from(groupEntries.values());
    entries.sort((a, b) => b.Kpm - a.Kpm); // KPM降順

    // ステージ分割保存
    let stageIndex = 0;
    const groupBorders = {};

    if (entries.length === 0) continue;

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
    // B. Daily Mode: 情報更新のみ（ステージ移動なし）
    // ==========================================
    
    // 更新があったドキュメント（ステージ）のみを記録
    // Map<DocID, { list: entry[], groupKey: string, stageId: number, isDirty: boolean }>
    
    // まず更新データをループして、既存データの場所を探す
    usersSnapshot.forEach(doc => {
      const newEntry = createRankEntry(doc);
      if (!newEntry) return;

      const location = userLocationMap.get(newEntry.Uid);
      
      if (location) {
        // 既存ユーザー: そのステージのリスト内のデータを更新
        const stageData = existingRankings.get(location.docId);
        if (stageData) {
          const list = stageData.list;
          const idx = list.findIndex(e => e.Uid === newEntry.Uid);
          if (idx !== -1) {
            // ランキング順位は維持したまま、データだけ更新
            // ※KPMが変わってもDailyでは順位入れ替えすらしない（クライアント側でソートする運用）
            const originalRanking = list[idx].Ranking;
            list[idx] = newEntry;
            list[idx].Ranking = originalRanking; // 順位を戻す
            stageData.isDirty = true;
          }
        }
      } else {
        // 新規ユーザー（またはランキング圏外からの復帰）
        // Dailyモードでは「一番下のステージ」に追加する
        const userData = doc.data().data || {};
        const groupKey = getRankingGroupKey(userData.Email);
        
        // そのグループの最終ステージを探す
        // Mapだと順序がないので、existingRankingsから検索
        let targetDocId = null;
        let maxStageId = -1;

        existingRankings.forEach((val, docId) => {
          if (val.groupKey === groupKey) {
            if (val.stageId > maxStageId) {
              maxStageId = val.stageId;
              targetDocId = docId;
            }
          }
        });

        if (targetDocId) {
          // 既存グループあり: 最終ステージに追加
          const stageData = existingRankings.get(targetDocId);
          // もし最終ステージが満員(150人)なら、新規ステージを作るべきだが、
          // 簡易的に「溢れても追加」するか、「あきらめる」か。
          // ここでは「溢れても追加」します（次のWeeklyで整理される）
          newEntry.Ranking = 9999; // 暫定順位
          stageData.list.push(newEntry);
          stageData.isDirty = true;
        } else {
          // 完全に新規のグループ: 新規ステージ作成
          // existingRankingsに追加してしまう
          const newDocId = `${groupKey}_stage_0`;
          newEntry.Ranking = 1;
          existingRankings.set(newDocId, {
            list: [newEntry],
            groupKey: groupKey,
            stageId: 0,
            isDirty: true
          });
        }
      }
    });

    // 変更があったステージのみ保存
    existingRankings.forEach((val, docId) => {
      if (val.isDirty) {
        // Dailyモードではリスト内のソートもしない（順位固定）ならそのままで良いが、
        // 「順位はローカルでKPMで並び替えるだけでいい」とのことなので、
        // DB上のリスト順序はバラバラでもOK？ 
        // 念のためKPM順にソートだけはしておく（Ranking番号は変えない）とクライアントが見やすいかもですが、
        // 「Ranking番号とリスト順序が不一致」は混乱の元。
        // ここでは「Ranking番号固定、リスト順序もRanking番号順（＝変更なし）」とします。
        
        // 保存
        const docRef = db.collection("rankings").doc(docId);
        batch.set(docRef, {
          rankingList: val.list,
          updatedAt: admin.firestore.FieldValue.serverTimestamp(),
          groupKey: val.groupKey,
          stageId: val.stageId
        });
        writeCount++;
        
        // ボーダーライン情報の更新（末尾が変わった可能性があるため）
        if (!allBorderlines[val.groupKey]) allBorderlines[val.groupKey] = {};
        // 現在のリストの末尾のKPMを取得（ソートされていないと不正確だが、Dailyは近似値で許容）
        const lastEntry = val.list[val.list.length - 1];
        allBorderlines[val.groupKey][val.stageId] = lastEntry ? lastEntry.Kpm : 0;
      }
    });
  }

  // 5. ボーダーライン情報の保存
  for (const [groupKey, borders] of Object.entries(allBorderlines)) {
    // 既存のボーダー情報を取得していないので、Dailyモードだと
    // 「更新があったステージのボーダー」しか書き込まれない（他ステージが消える）恐れがある。
    // → systemドキュメントは { merge: true } で書き込むべき。
    const borderDocRef = db.collection("system").doc(`ranking_borders_${groupKey}`);
    batch.set(borderDocRef, {
      borders: borders,
      updatedAt: admin.firestore.FieldValue.serverTimestamp()
    }, { merge: true }); // merge追加
    writeCount++;
  }

  // コミット
  await batch.commit();
  console.log(`ランキング集計完了(${modeName}): ${writeCount}件の書き込みを実行しました。`);
}
