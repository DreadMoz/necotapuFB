using System;
using System.Runtime.InteropServices;
using UnityEngine;
using necotapuFB; // AppVersionManagerへのアクセスを許可
using UnityEngine.SceneManagement;
using System.Collections.Generic; // 追加：List<ExRank>を使用するために必要ですにゃん

public class FirestoreConnection : MonoBehaviour
{
    public static FirestoreConnection Instance { get; private set; }

    // Firebaseアクセス制限の設定（時間単位）
    [SerializeField] private int firebaseAccessLimitHoursLoad = 1;
    [SerializeField] private int firebaseAccessLimitHoursSave = 1;
    // Firebaseからのデータロード成功フラグ
    public bool IsFirebaseLoadedSuccessfully { get; private set; }
    
    [SerializeField] private GameManager gm;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"FirestoreConnection: Awakeで設定されているLoad制限時間は {firebaseAccessLimitHoursLoad} 時間です"); // ここにログを追加
        Debug.Log($"FirestoreConnection: Awakeで設定されているSave制限時間は {firebaseAccessLimitHoursSave} 時間です"); // ここにログを追加
    }

    // JSLibから受け取る統合データ構造
    [System.Serializable]
    public class UnifiedLoadData
    {
        public object statusData;
        public List<ExRank> rankingData; // ここを ExRank から List<ExRank> に戻します
        public string appVersion;
        public string source; // データロード元 ('firebase' or 'local')
        public bool isFirebaseAccessed; // Firebaseへのアクセスが成功したかどうかのフラグ
    }

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void LoadFromFirestoreJslib(); 
    [DllImport("__Internal")]
    private static extern void SaveToFirestoreJslib(string dataPointer);
    [DllImport("__Internal")]
    private static extern void setFirebaseConfigJslib(string config);
    
    // 23時間制限付きの新しい関数
    [DllImport("__Internal")]
    private static extern void LoadAllDataFromFirestoreWithLimitJslib(string limitHours);
    [DllImport("__Internal")]
    private static extern void SaveToFirestoreWithLimitJslib(string dataPointer, string limitHours);
    
    // 制限時間を設定
    [DllImport("__Internal")]
    private static extern void SetFirebaseAccessLimitHoursJslib(int loadHours, int saveHours);

    // 新しい統合データロード完了コールバック
    [DllImport("__Internal")]
    private static extern void OnAllDataLoadCompleteJslib(string allDataJson);

    // 追加：統合データ保存のためのJSLib関数
    [DllImport("__Internal")]
    private static extern void SaveCombinedDataToFBJslib(string combinedDataJson, string limitHours);

#endif

    /// <summary>
    /// Firestoreからデータを読み込み（制限付き）
    /// </summary>
    public void LoadFromFirestore()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log($"FirestoreConnection: {firebaseAccessLimitHoursLoad}時間制限付きでFirestoreからデータを読み込み開始");
        // 制限時間を引数で渡して実行
        LoadAllDataFromFirestoreWithLimitJslib(firebaseAccessLimitHoursLoad.ToString());
#else
        Debug.Log("FirestoreConnection: エディタ環境 - ダミーデータを使用");
        GetDummyFirestoreData();
#endif
    }

    /// <summary>
    /// Firestoreにデータを保存（制限付き）
    /// </summary>
    public void SaveToFirestore(string dataPointer)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log($"FirestoreConnection: {firebaseAccessLimitHoursLoad}時間制限付きでFirestoreにデータを保存");
        // 制限時間を引数で渡して実行
        SaveToFirestoreWithLimitJslib(dataPointer, firebaseAccessLimitHoursLoad.ToString());
#else
        Debug.Log("FirestoreConnection: エディタ環境 - 保存をスキップ");
#endif
    }

    /// <summary>
    /// Firebase設定をHTML側に送信
    /// </summary>
    public void SendFirebaseConfig()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (gm != null && gm.firebaseConfig != null)
        {
            string configJson = gm.firebaseConfig.GetFirebaseConfigJson();

            // configJsonが空文字列でないことを確認
            if (!string.IsNullOrEmpty(configJson) && configJson != "{}")
            {
            Debug.Log($"Firebase設定を送信: {configJson}");
                setFirebaseConfigJslib(configJson);
                
                // 制限時間も設定
                SetFirebaseAccessLimitHoursJslib(firebaseAccessLimitHoursLoad,firebaseAccessLimitHoursSave);
            }
            else
            {
                Debug.LogError("FirestoreConnection: FirebaseConfigのJSONが空または不正です");
            }
        }
        else
        {
            Debug.LogError("FirestoreConnection: FirebaseConfigが見つかりません");
        }
#else
        Debug.Log("FirestoreConnection: エディタ環境 - Firebase設定送信をスキップ");
#endif
    }

    /// <summary>
    /// エディタ環境用のダミーデータ
    /// </summary>
    private void GetDummyFirestoreData()
    {
        Debug.Log("FirestoreConnection: ダミーデータを生成");
        
        // ダミーのステータスデータ
        string statusJson = @"{
                ""Email"": ""rochy2moo@gmail.com"",
            ""Ou"": ""テスト"",
                ""LastName"": ""Mori"",
            ""Gold"": 99999,
            ""Stage"": 0,
            ""Ranking"": 0,
            ""Name"": ""Demo"",
            ""RightHand"": 1,
                ""Glasses"": 0,
                ""Head"": 0,
                ""LeftHand"": 0,
                ""CatBody"": 0,
                ""CatFace"": 0,
                ""NickName"": 0,
            ""Kpm"": 10,
            ""Inventory"": [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
            ""Items"": [0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
            ""Medals"": [1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],
            ""Kpms"": [10,10,10,10,10,10,10,10],
            ""Settings"": [4, 20, 10, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
        }";
        
        if (FindObjectOfType<TitleSky>() != null)
        {
            FindObjectOfType<TitleSky>().SetUserData(statusJson);
        }
        else
        {
            Debug.LogError("FirestoreConnection: TitleSkyが見つかりません");
        }
    }
    
    /// <summary>
    /// ユーザーデータがFirebaseに存在するかチェック
    /// </summary>
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool CheckFirestoreUserDataExistsJslib();

    public bool HasUserData()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("FirestoreConnection: WebGL環境 - Firebaseデータ存在チェック");
        
        try
        {
            // JavaScript側の関数を呼び出し
            bool hasData = CheckFirestoreUserDataExistsJslib();
            Debug.Log($"FirestoreConnection: データ存在チェック結果: {hasData}");
            return hasData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FirestoreConnection: データ存在チェックエラー: {e.Message}");
            return false;
        }
#else
        Debug.Log("FirestoreConnection: エディタ環境 - ダミーデータ存在チェック");
        // エディタ環境では常にfalseを返す（初期値設定を実行）
        return false;
#endif
    }

    /// <summary>
    /// Firestoreからユーザーデータをロード
    /// </summary>
    public void LoadUserData()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("FirestoreConnection: WebGL環境 - Firestoreデータロード開始");
        
        try
        {
            // 全データ一括ロード関数を呼び出し、23時間制限を適用します。
            LoadAllDataFromFirestoreWithLimitJslib(firebaseAccessLimitHoursLoad.ToString());
            Debug.Log("FirestoreConnection: データロード要求を送信");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FirestoreConnection: データロード要求エラー: {e.Message}");
        }
#else
        Debug.Log("FirestoreConnection: エディタ環境 - ダミーデータロード");
        // エディタ環境ではダミーデータを使用
        GetDummyFirestoreData();
#endif
    }

    /// <summary>
    /// Firestoreでユーザーデータの存在をチェック（廃止予定）
    /// </summary>
    public void CheckUserDataExists()
    {
        Debug.LogWarning("CheckUserDataExists: このメソッドは廃止予定です。LoadUserData()を使用してください。");
        // 直接データロードを実行
        LoadUserData();
    }
    
    /// <summary>
    /// JavaScript側からの統合データロード完了コールバック
    /// </summary>
    /// <param name="allDataJson">ロードされた全データとロード元を含むJSON文字列</param>
    public void OnAllDataLoadComplete(string allDataJson)
    {
        Debug.Log($"FirestoreConnection: 統合データロード完了 - {allDataJson}");
        Debug.Log($"FirestoreConnection: Firebaseアクセス制限時間は {firebaseAccessLimitHoursLoad} 時間です。");

        try
        {
            var allData = Newtonsoft.Json.JsonConvert.DeserializeObject<UnifiedLoadData>(allDataJson);
            
            if (allData == null)
            {
                Debug.LogError("FirestoreConnection: 統合データがnullです");
                return;
            }
            
            // ユーザーデータ（statusData）の処理
            if (allData.statusData != null)
            {
                if (FindObjectOfType<TitleSky>() != null)
                {
                    FindObjectOfType<TitleSky>().SetUserData(Newtonsoft.Json.JsonConvert.SerializeObject(allData.statusData));
                }
                IsFirebaseLoadedSuccessfully = true; // Firebaseからデータが正常にロードされた
            }
            else
            {
                Debug.Log("FirestoreConnection: ユーザーデータがありません。新規ユーザーとして処理。");
                // 新規ユーザーの場合、新規作成処理を実行
                if (FindObjectOfType<TitleSky>() != null)
                {
                    FindObjectOfType<TitleSky>().CreateNewCatAfterError(); // 既存の新規作成処理を呼び出し
                }
            }

            // ランキングデータ（rankingData）の処理
            if (allData.rankingData != null)
            {
                Debug.Log("FirestoreConnection: ランキングデータをGameManagerに設定");
                // ダミーデータを追加
                AddDummyData(allData.rankingData);
                // 単一のExRankオブジェクトをList<ExRank>に変換して渡すように修正
                gm.savedata.setRankingFromFirebaseOrLocal(allData.rankingData);
            }
            else
            {
                Debug.Log("FirestoreConnection: ランキングデータがありません。空のリストを設定します。");
                var list = new System.Collections.Generic.List<ExRank>();
                // ダミーデータを追加
                AddDummyData(list);
                gm.savedata.setRankingFromFirebaseOrLocal(list); // 空のリストを渡す
            }

            // アプリバージョン（appVersion）の処理
            // AppVersionManagerに直接バージョンを送信
            if (AppVersionManager.Instance != null)
            {
                Debug.Log($"FirestoreConnection: アプリバージョンをAppVersionManagerに送信: {allData.appVersion}");
                AppVersionManager.Instance.OnFirebaseVersionReceived(allData.appVersion);
            }

            // ロード元に応じたメッセージ表示をTitleSkyに通知
            if (FindObjectOfType<TitleSky>() != null)
            {
                FindObjectOfType<TitleSky>().DisplayLoadCompleteMessage(allData.source);
            }
            else
            {
                Debug.LogError("FirestoreConnection: TitleSkyが見つかりません");
            }
            
            // StartButtonを表示
            if (FindObjectOfType<TitleSky>() != null)
            {
                FindObjectOfType<TitleSky>().ShowStartButton();
            }
            IsFirebaseLoadedSuccessfully = allData.isFirebaseAccessed; // JSLibから渡されたフラグで更新

            // データロード完了後にリセットチェック
            if (gm != null) 
            {
                gm.CheckDailyReset();
            }
            else
            {
                var currentGm = FindObjectOfType<GameManager>();
                if (currentGm != null) currentGm.CheckDailyReset();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FirestoreConnection: 統合データ処理エラー: {e.Message}");
            Debug.LogError($"FirestoreConnection: エラー詳細: {e.StackTrace}");
            // エラー時も新規作成処理を試みる
            if (FindObjectOfType<TitleSky>() != null)
            {
                FindObjectOfType<TitleSky>().CreateNewCatAfterError();
            }
            IsFirebaseLoadedSuccessfully = false; // エラーが発生したため、Firebaseロードは失敗 (isFirebaseAccessedもfalseになるはず)
        }
    }

    /// <summary>
    /// Firebaseに初期データを保存
    /// </summary>
    public void SaveInitialData(string initialDataJson)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("FirestoreConnection: WebGL環境 - Firebaseに初期データを保存");
        SaveToFirestoreJslib(initialDataJson);      // ２３時間縛り無視
#else
        Debug.Log("FirestoreConnection: エディタ環境 - 初期データ保存をスキップ");
        Debug.Log($"保存される初期データ: {initialDataJson}");
#endif
    }
    
    /// <summary>
    /// Firebase保存完了時のコールバック
    /// </summary>
    /// <param name="result">保存結果（'success'、'error'、または 'limited'）</param>
    public void OnSaveComplete(string result)
    {
        if (result == "success")
        {
            Debug.Log("✅ Firebase保存完了");
        }
        else if (result == "limited")
        {
            Debug.Log("⚠️ Firebaseアクセス制限中 - ブラウザに保存完了");
        }
        else
        {
            Debug.LogError("❌ Firebase保存エラー");
        }
    }

    /// <summary>
    /// ランキング保存完了時のコールバック
    /// </summary>
    /// <param name="result">保存結果（'success'、'error'）</param>
    public void OnRankingSaveComplete(string result)
    {
        if (result == "success")
        {
            Debug.Log("✅ Firebaseランキング保存完了");
        }
        else
        {
            Debug.LogError("❌ Firebaseランキング保存エラー");
        }
    }

    /// <summary>
    /// 統合データをFirebaseに保存（ユーザデータとランキングデータ）
    /// </summary>
    public void SaveCombinedDataToFB(string combinedDataJson)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("FirestoreConnection: WebGL環境 - 統合データ（ユーザー＆ランキング）をFirebaseに保存");
        try
        {
            SaveCombinedDataToFBJslib(combinedDataJson, firebaseAccessLimitHoursSave.ToString());
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FirestoreConnection: 統合データ保存JSLib呼び出しエラー: {e.Message}");
        }
#else
        Debug.Log("FirestoreConnection: エディタ環境 - 統合データ保存をスキップ");
#endif
    }

    // ダミーデータ用の名前リスト
    private List<string> randomNames = new List<string> {
        "yuki", "hayato", "haruki", "ryusei", "kaito", "kota", "yuma", "soma", "riku", "sora", "ryota", "daiki", "minato", "ren", "hinata", "kazuki", "takumi", "hiroto", "ryuto", "yuma", "sosuke", "ryu", "keita", "koki", "toma", "seiji", "yu", "hana", "yui", "rin", "mei", "mio", "saki", "aoi", "yuna", "maika", "kokona", "miku", "nana", "rika", "yuka", "haruka", "emi", "risa", "yuri", "sakura", "rei", "noa", "mai", "rio", "meika", "erika", "airi", "marin", "aya", "mina", "yuko", "kaede", "ayumu", "taiga", "shota", "eito", "reo", "kensei", "shin", "manato", "ryoga", "kanata", "tsubasa", "itsuki", "asahi", "mahiro", "haru", "ikki", "sho", "yuki", "kyou", "ayaka", "sena", "himari", "yume", "aina", "kanon", "ryosuke", "saya", "kaho", "fumi", "sara", "momoka", "sumire", "akari", "hinako", "yuina", "riona", "manami", "sayaka", "nao", "yusuke", "tatsuya", "kazuma", "masato", "shun", "kyohei", "takuya", "naoki", "kenta", "jun", "misaki", "riko", "chinatsu", "kumi", "miyu", "ryou", "naoko", "keiko", "chie", "akiko", "asuka", "sato", "natsuki", "ryohei", "satoshi", "takahiro", "yasuharu", "yoshiki", "yota", "daigo", "ema", "himawari", "ichika", "juri", "kairi", "runa", "mao", "nagisa", "otoha", "hina", "rena", "suzu", "saiga", "umi", "nami", "wakana", "haruto", "yuto", "taiko", "mitsu", "nobuyuki", "haruna", "fumiya", "genki", "reisa", "ami", "yua", "miho", "sota", "tomoki", "arisa", "kana", "junya", "miki", "hiroki", "ai", "tetsuya", "yoko", "masaki", "naru", "kenji", "saki", "yuri", "yuki", "syo", "hiro", "mayo", "nori", "hana", "rina", "koji", "yuka", "asami", "ryusei", "shota", "reiko", "tomo", "yuto", "kai", "mao", "nao", "ryo", "kei", "asuka", "miko", "hikari", "taka", "shu", "saya", "yuji", "hiroto", "maki", "rin", "kota", "yumi", "sora", "tatsu", "aiko", "sumi", "seiya", "kotoha", "akira", "yuina", "maomi", "rena", "naoki", "yasu", "Yuto", "Hiroto", "Hina", "Ai", "Yuri", "Ryota", "Seiya", "Mei", "Aoi", "Kotone", "Hinata", "Daiki", "Daichi", "Hayato", "Suzu", "Kousuke", "Yuuji", "Riko", "Emi", "Yusuke", "Ryu", "Kouki", "Mai", "Kanon", "Hideto", "hideto", "Hinako", "Misaki", "Minato", "konan", "Yuna", "naomi", "tomomi", "taito", "yuine", "Akari", "Reona", "Riona", "Rio", "iroha", "hiroshi", "takeshi", "nobuyoshi", "kazuhiro", "takahiro", "hiroshi", "koji", "hidenori", "gou", "toyoshi", "takao", "naoshi", "kenji"
    };

    /// <summary>
    /// ダウンロードしたランキングデータの末尾にダミーデータを追加する
    /// </summary>
    private void AddDummyData(List<ExRank> rankings)
    {
        if (rankings == null) return;

        int dummyCount = 50;
        int startRank = rankings.Count + 1;
        int lastKpm = 10;

        // 既存のデータの最後からKPMを取得
        if (rankings.Count > 0)
        {
            lastKpm = rankings[rankings.Count - 1].Kpm;
        }

        System.Random rand = new System.Random();

        for (int i = 0; i < dummyCount; i++)
        {
            // KPMをランダムに減少 (0~2)
            int adjustment = rand.Next(0, 3);
            lastKpm -= adjustment;
            if (lastKpm < 10) lastKpm = 10;

            ExRank dummy = new ExRank();
            dummy.Ranking = startRank + i;
            dummy.FirstName = randomNames[rand.Next(randomNames.Count)];
            dummy.Kpm = lastKpm;
            dummy.Uid = "dummy_" + System.Guid.NewGuid().ToString(); // ユニークID

            // 装備データ: CatBodyは 201 + rand(0-7), 他は0
            dummy.CatBody = 201 + rand.Next(0, 8);
            dummy.RightHand = 0;
            dummy.Glasses = 0;
            dummy.Head = 0;
            dummy.LeftHand = 0;
            dummy.CatFace = 0;
            dummy.NicknameNo = 0;
            dummy.Stage = 0;

            rankings.Add(dummy);
        }
        Debug.Log($"FirestoreConnection: {dummyCount}件のダミーデータを追加しました。現在の総数: {rankings.Count}");
    }
}

