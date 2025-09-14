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
    [SerializeField] private int firebaseAccessLimitHours = 1;
    
    [SerializeField] private GameManager gm;

    // Firebaseからデータが正常にロードされたかどうかを示すフラグ
    private bool isFirebaseDataLoaded = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"FirestoreConnection: Awakeで設定されている制限時間は {firebaseAccessLimitHours} 時間です"); // ここにログを追加
    }

    // JSLibから受け取る統合データ構造
    [System.Serializable]
    public class UnifiedLoadData
    {
        public object statusData;
        public List<ExRank> rankingData; // ここを ExRank から List<ExRank> に戻します
        public string appVersion;
        public string source; // データロード元 ('firebase' or 'local')
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
    private static extern void SetFirebaseAccessLimitHoursJslib(int hours);

    // 新しい統合データロード完了コールバック
    [DllImport("__Internal")]
    private static extern void OnAllDataLoadCompleteJslib(string allDataJson);

    // ランキングデータ保存のためのJSLib関数
    [DllImport("__Internal")]
    private static extern void saveFirestoreRankingDataFromUnityJslib(string rankingDataJson);

#endif

    /// <summary>
    /// Firestoreからデータを読み込み（制限付き）
    /// </summary>
    public void LoadFromFirestore()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log($"FirestoreConnection: {firebaseAccessLimitHours}時間制限付きでFirestoreからデータを読み込み開始");
        // 制限時間を引数で渡して実行
        LoadAllDataFromFirestoreWithLimitJslib(firebaseAccessLimitHours.ToString());
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
        if (!isFirebaseDataLoaded)
        {
            Debug.LogWarning("FirestoreConnection: Firebaseからデータがロードされていないため、Firebaseへの保存をスキップしました。");
            // Firebaseに保存しない場合でも、LocalStorageには保存する可能性を考慮
            // this.SaveToLocalJslib(dataPointer); // 必要であればLocalStorage保存を呼び出す
            return;
        }
        Debug.Log($"FirestoreConnection: {firebaseAccessLimitHours}時間制限付きでFirestoreにデータを保存");
        // 制限時間を引数で渡して実行
        SaveToFirestoreWithLimitJslib(dataPointer, firebaseAccessLimitHours.ToString());
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
                SetFirebaseAccessLimitHoursJslib(firebaseAccessLimitHours);
                Debug.Log($"制限時間を設定: {firebaseAccessLimitHours}時間");
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
            FindObjectOfType<TitleSky>().FinishDataLoadExtStatus(statusJson);
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
            // 全データ一括ロード関数を呼び出し、23時間制限を適用しますにゃん。
            LoadAllDataFromFirestoreWithLimitJslib(firebaseAccessLimitHours.ToString());
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
    /// JavaScript側からのデータロード完了コールバック
    /// </summary>
    /// <param name="dataJson">ロードされたデータのJSON文字列</param>
    public void OnLoadComplete(string dataJson)
    {
        Debug.Log($"FirestoreConnection: データロード完了 - {dataJson}");
        
        if (dataJson == "error" || dataJson == "{}")
        {
            Debug.Log("🆕 FirestoreConnection: 新規ユーザー - 新規作成処理に移行");
            // 新規ユーザーの場合、新規作成処理を実行
            if (FindObjectOfType<TitleSky>() != null)
            {
                Debug.Log("FirestoreConnection: 新規作成処理を実行");
                FindObjectOfType<TitleSky>().CreateNewCatAfterError();
            }
            else
            {
                Debug.LogError("FirestoreConnection: TitleSkyが見つかりません");
            }
        }
        else
        {
            try
            {
                // バージョン比較を実行してからデータを設定
                if (FindObjectOfType<TitleSky>() != null)
                {
                    FindObjectOfType<TitleSky>().CompareAndSetData(dataJson);
                    Debug.Log("FirestoreConnection: バージョン比較完了");
                }
                else
                {
                    Debug.LogError("FirestoreConnection: TitleSkyが見つかりません");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"FirestoreConnection: データ設定エラー: {e.Message}");
            }
        }
        
        // startButtonを表示（データロード完了後）
        if (FindObjectOfType<TitleSky>() != null)
        {
            FindObjectOfType<TitleSky>().ShowStartButton();
        }
    }
    
    /// <summary>
    /// JavaScript側からの統合データロード完了コールバック
    /// </summary>
    /// <param name="allDataJson">ロードされた全データとロード元を含むJSON文字列</param>
    public void OnAllDataLoadComplete(string allDataJson)
    {
        Debug.Log($"FirestoreConnection: 統合データロード完了 - {allDataJson}");
        Debug.Log($"FirestoreConnection: Firebaseアクセス制限時間は {firebaseAccessLimitHours} 時間です。");

        try
        {
            var allData = Newtonsoft.Json.JsonConvert.DeserializeObject<UnifiedLoadData>(allDataJson);
            
            if (allData == null)
            {
                Debug.LogError("FirestoreConnection: 統合データがnullです");
                isFirebaseDataLoaded = false; // エラー時はフラグをfalseに
                return;
            }
            
            // ユーザーデータ（statusData）の処理
            if (allData.statusData != null)
            {
                Debug.Log("FirestoreConnection: ユーザーデータをGameManagerに設定");
                // GameManager.savedataに設定
                gm.savedata.SetData(Newtonsoft.Json.JsonConvert.SerializeObject(allData.statusData));
                // TitleSkyのデータ設定ロジックを呼び出す
                if (FindObjectOfType<TitleSky>() != null)
                {
                    // TitleSky.CompareAndSetDataがFirebaseデータを採用した場合にのみisFirebaseDataLoadedをtrueにする
                    // 現状のTitleSky.CompareAndSetDataの実装では、どちらのデータが採用されたか直接返さないため、
                    // ここではロード元がfirebaseであればtrueと仮定。厳密な比較はTitleSky側で行われている。
                    FindObjectOfType<TitleSky>().CompareAndSetData(Newtonsoft.Json.JsonConvert.SerializeObject(allData.statusData));
                    if (allData.source == "firebase")
                    {
                        isFirebaseDataLoaded = true;
                        Debug.Log("FirestoreConnection: Firebaseデータが正常にロードされました。");
                    }
                    else
                    {
                        isFirebaseDataLoaded = false;
                        Debug.LogWarning("FirestoreConnection: ローカルデータまたはエラーにより、Firebaseデータはロードされませんでした。");
                    }
                }
            }
            else
            {
                Debug.Log("FirestoreConnection: ユーザーデータがありません。新規ユーザーとして処理。");
                // 新規ユーザーの場合、新規作成処理を実行
                if (FindObjectOfType<TitleSky>() != null)
                {
                    FindObjectOfType<TitleSky>().CreateNewCatAfterError(); // 既存の新規作成処理を呼び出し
                }
                isFirebaseDataLoaded = false; // データなしのためフラグをfalseに
            }

            // ランキングデータ（rankingData）の処理
            if (allData.rankingData != null)
            {
                Debug.Log("FirestoreConnection: ランキングデータをGameManagerに設定");
                // 単一のExRankオブジェクトをList<ExRank>に変換して渡すように修正
                gm.savedata.setRankingFromLocal(allData.rankingData);
            }
            else
            {
                Debug.Log("FirestoreConnection: ランキングデータがありません。空のリストを設定します。");
                gm.savedata.setRankingFromLocal(new System.Collections.Generic.List<ExRank>()); // 空のリストを渡す
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
        }
    }

    /// <summary>
    /// Firebaseに初期データを保存
    /// </summary>
    public void SaveInitialData(string initialDataJson)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isFirebaseDataLoaded)
        {
            Debug.LogWarning("FirestoreConnection: Firebaseからデータがロードされていないため、初期データのFirebaseへの保存をスキップしました。");
            Debug.Log($"初期データはLocalStorageに保存されます: {initialDataJson}"); // LocalStorageへの保存は別途実装が必要
            return;
        }
        Debug.Log("FirestoreConnection: WebGL環境 - Firebaseに初期データを保存");
        SaveToFirestoreJslib(initialDataJson);
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
    /// Firebaseにランキングデータを保存
    /// </summary>
    public void SaveRankingToFirestore(ExRank userRankingData)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("FirestoreConnection: WebGL環境 - Firebaseにランキングデータを保存");
        try
        {
            string rankingDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(userRankingData);
            saveFirestoreRankingDataFromUnityJslib(rankingDataJson);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FirestoreConnection: ランキングデータ保存エラー: {e.Message}");
        }
#else
        Debug.Log("FirestoreConnection: エディタ環境 - ランキングデータ保存をスキップ");
#endif
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

}

