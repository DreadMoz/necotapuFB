using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Collections;
using necotapuFB;
[System.Serializable]
public class UserInfo
{
    public string email;
    public string firstName;
    public string lastName;
    public string picture;
    public string department;
    public string message;
    public string access;
}
public class TitleSky : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed = 0.5f;
    private Material skyboxMaterial;

    [SerializeField]
    private GameManager gm;
    [SerializeField]
    private FirestoreConnection firestoreConnection;

    [SerializeField]
    private GameObject player;        // プレイヤーオブジェクト
    [SerializeField]
    private Fade fade;                // フェード用オブジェクト
    [SerializeField]
    private ChibiCat cat;             // ねこオブジェクト

    [SerializeField]
    private Text ouText; // データ表示用
    [SerializeField]
    private Text firstName; // データ表示用
    [SerializeField]
    private Text lastName; // データ表示用
    [SerializeField]
    private Text department; // データ表示用
    [SerializeField]
    private Text mailText; // データ表示用
    [SerializeField]
    private Image picture; // データ表示用

    [SerializeField]
    private GameObject startButton; // startボタン

    [SerializeField]
    private GameObject guestButton; // guestボタン
    [SerializeField]
    private GameObject userData; // ユーザーデータ
    [SerializeField]
    private GameObject message; // メッセージボックス
    [SerializeField]
    private GameObject reLogin; // ログインしなおす

    [SerializeField]
    private GameObject standupButton; // standupボタン
    [SerializeField]
    private GameObject nextButton; // nextボタン
    [SerializeField]
    private GameObject prevButton; // prevボタン
    [SerializeField]
    private GameObject confirmButton; // confirmボタン
    [SerializeField]
    private GameObject ashiato;
    [SerializeField]
    private GameObject deverop;

    private Animator animator;
    private int necoNo = 201;
    private bool firstPush = false;      // スタートボタンが2回以上押されないようにするためのフラグ
    private bool goNextScene = false;    // ワールドシーンに遷移するためのフラグ

    private int loginFlg = 0;


    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        deverop.SetActive(false);
#endif
    }
    // Start is called before the first frame update
    void Start()
    {
        reLogin.SetActive(false);
        standupButton.SetActive(false);
        nextButton.SetActive(false);
        prevButton.SetActive(false);
        confirmButton.SetActive(false);
        userData.SetActive(false);
        message.SetActive(false);
        ashiato.SetActive(false);
        skyboxMaterial = RenderSettings.skybox;
        skyboxMaterial.SetFloat("_Rotation", 330f);
        animator = player.GetComponent<Animator>(); // Playerのアニメーターを取得
        animator.SetFloat("goroSpeed", 1.0f);
        
        // TitleSky開始時にSaveDataを確実に初期化
        if (gm != null && gm.savedata != null)
        {
            gm.savedata.InitializeEmptyData();
            Debug.Log("TitleSky: SaveDataの初期化完了");
        }
        else
        {
            Debug.LogError("TitleSky: gmまたはgm.savedataがnullです");
        }

        
#if !UNITY_EDITOR

        if (GameManager.SceneNo == scene.Night)
        {
            startButton.SetActive(false);
            guestButton.SetActive(false);

            message.SetActive(true);
            Text messageText = message.GetComponentInChildren<Text>();
            messageText.text = "ねこは寝ています。";
            cat.setEmo(27);                 // 寝顔
            animator.SetFloat("goroSpeed", 0.2f);
            return;
        }
#endif

        TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = "ログイン";
        gm.savedata.Settings[se.GachaCnt] = 1;      // ボーナスダイヤの初期値

        // Firebase設定をhJSに送信
        // if (firestoreConnection != null)
        // {
        //     firestoreConnection.SendFirebaseConfig();
        // }
        
        // AuthManagerの認証成功イベントを購読
        var authManager = FindObjectOfType<necotapuFB.AuthManager>();
        if (authManager != null)
        {
            authManager.OnAuthenticationSuccess += OnAuthenticationSuccess;
            Debug.Log("OnAuthenticationSuccessイベントを購読しました");
        }

        // AppVersionManagerの強制再読み込みイベントを購読
        if (AppVersionManager.Instance != null)
        {
            AppVersionManager.Instance.OnForceReloadRequired += OnForceReloadRequiredHandler; 
        }

    }

    // Update is called once per frame
    void Update()
    {
        skyboxMaterial.SetFloat("_Rotation", Mathf.Repeat(skyboxMaterial.GetFloat("_Rotation") + rotateSpeed * Time.deltaTime, 360f));

        // Sキーが押されたらStartButtonメソッドを呼ぶ
        if (Input.GetKeyDown(KeyCode.S))
        {
//            this.StartButton();
        }

        // 画面遷移
        if (!goNextScene && fade.IsFadeOutComplete())
        {
            GameManager.SceneNo = (int)scene.World;      // ワールドシーンスタート
            SceneManager.LoadScene("WorldScene"); // ワールドシーンに遷移
            goNextScene = true;                   // 2回目以降の遷移を防ぐためのフラグを立てる
        }
    }

    public void StartButton()
    {
        if (loginFlg == 0)
        {
            startButton.SetActive(false);   // ログイン完了まで一旦消す
            guestButton.SetActive(false);   // ログイン完了まで一旦消す
            // AuthManagerのGoogle認証を呼び出し
            var authManager = FindObjectOfType<necotapuFB.AuthManager>();
            if (authManager != null)
            {
                authManager.SignInWithGoogle();
            }
        }
        else if (loginFlg == 1)
        {
            if (!firstPush)
            {
                fade.StartFadeOut();
                firstPush = true;
            }
        }
        else if (loginFlg == 2)
        {
            selectNeco();
        }
    }

    /// <summary>
    /// Unityエディタ環境での認証成功時の処理 
    /// </summary>
    public void OnAuthenticationSuccess(necotapuFB.AuthInfo authInfo)
    {
        Debug.Log($"OnAuthenticationSuccess: Unityエディタ環境での認証成功 - 呼び出し元: {System.Environment.StackTrace}");
        
        // ログインフラグを設定
        loginFlg = 1;
        
        // UIに認証情報を表示
        userData.SetActive(true);
        message.SetActive(true);
        
        // 表示名からfirstNameとlastNameを分割
        string[] nameParts = authInfo.displayName.Split(' ');
        string firstName = nameParts.Length > 0 ? nameParts[0] : "";
        string lastName = nameParts.Length > 1 ? nameParts[1] : "";
        
        // 認証情報を保存
        currentFirstName = firstName;
        currentLastName = lastName;
        currentEmail = authInfo.email;
        
        // UIに表示
        mailText.text = authInfo.email;
        this.firstName.text = firstName;
        this.lastName.text = lastName;
        department.text = authInfo.authMethod; // 認証方法を表示
        
        // プロフィール画像（Googleアカウントの画像がある場合のみ表示）
        if (!string.IsNullOrEmpty(authInfo.photoURL))
        {
            StartCoroutine(LoadImage(authInfo.photoURL));
        }
        else
        {
            Debug.Log("プロフィール画像がありません。画像は表示しません。");
        }
        
        // ログイン成功後の処理
        Debug.Log($"ログイン成功: {authInfo.email} - Firestoreチェックを開始");
        
        // 新しいアカウントでログインした場合、既存データをクリア
        if (!string.IsNullOrEmpty(gm.savedata.Email) && gm.savedata.Email != authInfo.email)
        {
            Debug.Log($"アカウントが変更されました: '{gm.savedata.Email}' → '{authInfo.email}' - データをクリア");
            ClearGameData();
        }
        else
        {
            Debug.Log("同じアカウントでのログイン - データクリアをスキップ");
        }

        reLogin.SetActive(true); // ログアウトボタン表示
        
        Debug.Log($"ログイン情報表示完了: {authInfo.email} ({authInfo.displayName})");
        Debug.Log("認証成功処理完了 - ログイン情報表示を実行");
        
        CheckAccount();
        Debug.Log("checkAccount関数を呼び出してUIを更新");
    }

    public void FinishDataLoadExtStatus(string statusDataJson)
    {
        message.SetActive(true);
        Text messageText = message.GetComponentInChildren<Text>();

        try
        {
            if (statusDataJson != null)
            {
                try
                {
                    // Firebaseからロードしたデータを直接ゲームデータに設定
                    gm.savedata.DeserializeFromFirebase(statusDataJson);
                    
                    // ユーザー名を最新のGoogleアカウント情報で更新
                    UpdateUserInfoFromAuth();

                    // Firebaseから全データを一括取得（ユーザー情報、ランキング情報、バージョン情報）
                    // if (AppVersionManager.Instance != null)
                    // {
                    //     AppVersionManager.Instance.LoadAllDataFromFirebase();
                    // }
                    // else
                    // {
                    //     Debug.LogError("TitleSky: AppVersionManager.Instance が見つかりません");
                    // }
                    
                    ouText.text = gm.savedata.AuthMethod;
                CheckDailyReset();
                    
                    // Firebaseデータロード完了メッセージを表示
                    messageText.text = "✅ サーバーからデータをロードしたよ。スタートしよう！";
                    
                    // 装備データを反映
                    Debug.Log($"FinishDataLoadExtStatus: 装備情報を反映 - CatBody: {gm.savedata.Equipment[eq.CatBody]}, RightHand: {gm.savedata.Equipment[eq.RightHand]}, LeftHand: {gm.savedata.Equipment[eq.LeftHand]}, Head: {gm.savedata.Equipment[eq.Head]}, Glasses: {gm.savedata.Equipment[eq.Glasses]}");
                    cat.setChara(gm.savedata.Equipment[eq.CatBody]);
                    cat.changeEquipHands(gm.savedata.Equipment[eq.RightHand], gm.savedata.Equipment[eq.LeftHand], gm.checkBagItem());
                    cat.changeEquipHead(gm.savedata.Equipment[eq.Head]);
                    cat.changeEquipGlasses(gm.savedata.Equipment[eq.Glasses]);

                    // データロード後、既存データ処理を実行してUIを更新
                    UseExistingData();
                    
                if (gm.savedata.getTotalMedal() >= 264)     // 0~65 * 4
                {
                    ashiato.SetActive(true);
                }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Firebaseデータの解析に失敗: {ex.Message}");
                    messageText.text = "データの読み込みに失敗しました。";
                }
            }
            else
            {
                Debug.Log("ステータスデータがnull");
            }
        }
        catch (Exception ex)
        {
            messageText.text = "データを読み込む時にエラーが発生しました: ";
            Debug.LogError("データの読み込み中に例外発生: " + ex);
        }
    }
    
    /// <summary>
    /// 強制再読み込みが必要な場合にAppVersionManagerから呼ばれるハンドラ
    /// </summary>
    private void OnForceReloadRequiredHandler()
    {
        Debug.Log("TitleSky: 強制再読み込みが必要なイベントを受信しました。");
        Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = "最新のアプリのバージョンが見つかりました。ロードするにゃん。";
        message.SetActive(true); // メッセージウィンドウを表示
    }

    /// <summary>
    /// Firebaseデータとローカルデータを比較して、新しい方で開始
    /// </summary>
    public void CompareAndSetData(string firebaseDataJson)
    {
        Debug.Log("CompareAndSetData: バージョン比較開始");
        
        try
        {
            // Firebaseデータを保存
            pendingFirebaseData = firebaseDataJson;
            
            // ローカルデータを取得
            var localDataJson = gm.savedata.SerializeForFirebase();
            
            // バージョン比較は新しいAppVersionManagerで処理されるため、ここではFirebaseデータを使用
            FinishDataLoadExtStatus(firebaseDataJson);
        }
        catch (Exception ex)
        {
            Debug.LogError($"バージョン比較中にエラーが発生: {ex.Message}");
            // エラーの場合はFirebaseデータを使用
            FinishDataLoadExtStatus(firebaseDataJson);
        }
    }
    
    /// <summary>
    /// バージョン比較結果：ローカルデータを使用
    /// </summary>
    public void UseLocalData()
    {
        Debug.Log("UseLocalData: ローカルデータの方が新しい - ローカルデータを使用");
        
        // ローカルデータでゲームを開始
        message.SetActive(true);
        Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = "🌐 ローカルデータの方が新しいため、ローカルデータをロードしたよ。スタートしよう！";
        
        // 装備データを反映
        Debug.Log($"UseLocalData: 装備情報を反映 - CatBody: {gm.savedata.Equipment[eq.CatBody]}, RightHand: {gm.savedata.Equipment[eq.RightHand]}, LeftHand: {gm.savedata.Equipment[eq.LeftHand]}, Head: {gm.savedata.Equipment[eq.Head]}, Glasses: {gm.savedata.Equipment[eq.Glasses]}");
        cat.setChara(gm.savedata.Equipment[eq.CatBody]);
        cat.changeEquipHands(gm.savedata.Equipment[eq.RightHand], gm.savedata.Equipment[eq.LeftHand], gm.checkBagItem());
        cat.changeEquipHead(gm.savedata.Equipment[eq.Head]);
        cat.changeEquipGlasses(gm.savedata.Equipment[eq.Glasses]);
        
        if (gm.savedata.getTotalMedal() >= 264)     // 0~65 * 4
        {
            ashiato.SetActive(true);
        }
    }
    
    /// <summary>
    /// バージョン比較結果：Firebaseデータを使用
    /// </summary>
    public void UseFirebaseData()
    {
        Debug.Log("UseFirebaseData: サーバーデータの方が新しい - サーバーデータを使用");
        
        // Firebaseデータでゲームを開始（既存の処理を使用）
        Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = "✅ サーバーからデータをロードしたよ。スタートしよう！";
        FinishDataLoadExtStatus(pendingFirebaseData);
    }

    // 認証情報を保存する変数
    private string currentFirstName = "";
    private string currentLastName = "";
    private string currentEmail = "";
    
    // バージョン比較用の変数
    private string pendingFirebaseData = "";

    /// <summary>
    /// ロード完了メッセージを表示
    /// </summary>
    /// <param name="source">データのロード元 ('firebase' または 'local')</param>
    public void DisplayLoadCompleteMessage(string source)
    {
        Text messageText = message.GetComponentInChildren<Text>();
        if (source == "firebase")
        {
            messageText.text = "✅ サーバーからデータをロードしたよ。スタートしよう！";
        }
        else if (source == "local")
        {
            messageText.text = "🌐 ブラウザからデータをロードしたよ。スタートしよう！";
        }
        else
        {
            messageText.text = "データロード完了。スタートしよう！";
        }
        message.SetActive(true);
    }

    /// <summary>
    /// ゲームデータ全体をクリア（アカウント切り替え時のデータ分離）
    /// </summary>
    public void ClearGameData()
    {
        Debug.Log("ClearGameData: ゲームデータ全体をクリア開始");
        
        try
        {
            // SaveDataの全フィールドをリセット
            gm.savedata.UserName = "";
            gm.savedata.Email = "";
            gm.savedata.AuthMethod = "";
            gm.savedata.LastName = "";
            
            // 配列データをリセット
            for (int i = 0; i < gm.savedata.Status.Length; i++)
            {
                gm.savedata.Status[i] = 0;
            }
            
            for (int i = 0; i < gm.savedata.Equipment.Length; i++)
            {
                gm.savedata.Equipment[i] = 0;
            }
            
            for (int i = 0; i < gm.savedata.Inventory.Length; i++)
            {
                gm.savedata.Inventory[i] = 0;
            }
            
            for (int i = 0; i < gm.savedata.Items.Length; i++)
            {
                gm.savedata.Items[i] = false;
            }
            
            for (int i = 0; i < gm.savedata.Medals.Length; i++)
            {
                gm.savedata.Medals[i] = 0;
            }
            
            for (int i = 0; i < gm.savedata.Kpms.Length; i++)
            {
                gm.savedata.Kpms[i] = 0;
            }
            
            for (int i = 0; i < gm.savedata.Settings.Length; i++)
            {
                gm.savedata.Settings[i] = 0;
            }
            
            // リストデータをクリア
            gm.savedata.ExRankings.Clear();
            
            // 猫の装備もリセット
            if (cat != null)
            {
                cat.setChara(0);
                cat.changeEquipHands(0, 0, gm.checkBagItem());
                cat.changeEquipHead(0);
                cat.changeEquipGlasses(0);
                Debug.Log("ClearGameData: 猫の装備をリセット完了");
            }
            
            Debug.Log("ClearGameData: ゲームデータ全体のクリア完了");
        }
        catch (Exception ex)
        {
            Debug.LogError($"ClearGameData: エラーが発生: {ex.Message}");
        }
    }

    /// <summary>
    /// 認証情報からユーザー情報を更新
    /// </summary>
    private void UpdateUserInfoFromAuth()
    {
        if (!string.IsNullOrEmpty(currentFirstName) && !string.IsNullOrEmpty(currentLastName))
        {
            // 現在のFirebaseデータとGoogleアカウント情報を比較
            bool needsUpdate = false;
            
            if (gm.savedata.UserName != currentFirstName)
            {
                Debug.Log($"ユーザー名を更新: '{gm.savedata.UserName}' → '{currentFirstName}'");
                gm.savedata.UserName = currentFirstName;
                needsUpdate = true;
            }
            
            if (gm.savedata.LastName != currentLastName)
            {
                Debug.Log($"姓を更新: '{gm.savedata.LastName}' → '{currentLastName}'");
                gm.savedata.LastName = currentLastName;
                needsUpdate = true;
            }
            
            if (gm.savedata.Email != currentEmail)
            {
                Debug.Log($"メールアドレスを更新: '{gm.savedata.Email}' → '{currentEmail}'");
                gm.savedata.Email = currentEmail;
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                Debug.Log("ユーザー情報が更新されました - Firebaseに保存を実行");
                // 更新された情報をFirebaseに保存
                gm.savedata.saveToFirebase();
            }
            else
            {
                Debug.Log("ユーザー情報に変更はありません");
            }
        }
        else
        {
            Debug.LogWarning("認証情報が不完全です - ユーザー情報の更新をスキップ");
        }
    }

    private void CheckDailyReset()
    {
        DateTime today = DateTime.Now;
        int todayDate = today.Year * 10000 + today.Month * 100 + today.Day;
        
        if (gm.savedata.Settings[se.LastLogin] != todayDate)
        {
            gm.savedata.Settings[se.GachaCnt] = 4;      // ボーナスダイヤを４に

            // 日付の更新
            gm.savedata.Settings[se.LastLogin] = todayDate;
        }
    }

    private void checkLocalData()
    {
        Text messageText = message.GetComponentInChildren<Text>();

        // アカウント情報があるかどうかで判定
        if (!string.IsNullOrEmpty(gm.savedata.Email) && !string.IsNullOrEmpty(gm.savedata.UserName))
        {
            animator.SetBool("Standup", false);
            // アカウント情報が存在する場合（既存ユーザー）
            Debug.Log("既存アカウント情報あり - 既存データを使用");
            messageText.text = "🌐 ブラウザからデータをロードしたよ。スタートしよう！";
            
            // 既存データがある場合の処理を実行
            UseExistingData();
        }
        else
        {
            // アカウント情報がない場合（新規ユーザー）
            Debug.Log("新規ユーザー - ねこを作成");
            messageText.text = "🆕 新規ユーザーです。いっしょにたびをするねこをえらんでね。";
            CreateNewCat();
        }
    }

    /// <summary>
    /// 新規ユーザーの新規作成処理
    /// </summary>
    public void CreateNewCatAfterError()
    {
        Debug.Log("🆕 TitleSky: 新規ユーザー - 新規作成処理を実行");
        
        // ログインフラグを2に設定（新規ユーザー）
        loginFlg = 2;
        
        // 新規作成処理を実行
        CreateNewCat();
        
        Debug.Log("CreateNewCatAfterError: 新規ユーザーフラグ設定完了");
    }

    private void CheckAccount()
    {
        Debug.Log("CheckAccount: アカウント情報をチェック");
        
        // 認証情報を設定
        currentEmail = mailText.text;
        currentFirstName = firstName.text;
        currentLastName = lastName.text;
        
        // ユーザー情報を表示
        userData.SetActive(true);
        
        // データロードを開始
        if (firestoreConnection != null)
        {
            firestoreConnection.LoadUserData();
        }
        else
        {
            Debug.LogError("FirestoreConnectionが見つかりません");
        }
    }
    
    /// <summary>
    /// 既存データがある場合の処理
    /// </summary>
    private void UseExistingData()
    {
        Debug.Log("UseExistingData: 既存データを使用");
        
        // 既存の配列からねこデータを設定（装備は既にFinishDataLoadExtStatusで反映済み）
        Debug.Log($"UseExistingData: 既存配列装備情報 - CatBody: {gm.savedata.Equipment[eq.CatBody]}, RightHand: {gm.savedata.Equipment[eq.RightHand]}, LeftHand: {gm.savedata.Equipment[eq.LeftHand]}, Head: {gm.savedata.Equipment[eq.Head]}, Glasses: {gm.savedata.Equipment[eq.Glasses]}");
        
        // ボタンテキストを「スタート」に設定
            TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = "スタート";
        startButton.SetActive(true);
        
        // ログインフラグを1に設定（既存ユーザー）
            loginFlg = 1;
        
        Debug.Log("UseExistingData: 既存データの設定完了");
    }
    
    /// <summary>
    /// データがない場合の処理（ねこを作る）
    /// </summary>
    private void CreateNewCat()
    {
        Debug.Log("CreateNewCat: 新しいねこを作成");
        
        // 新規ユーザーのため、既存データを完全にクリア
        ClearGameData();
        Debug.Log("CreateNewCat: 既存データをクリア完了");
        
        // メッセージにねこ作成の案内を追加
            Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = "あたらしく" + firstName.text + "さんのデータをつくりましょう。";
        
        // ボタンテキストを「つくる」に設定
            TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = "つくる";
        
        // ログインフラグを2に設定（新規ユーザー）
            loginFlg = 2;
        
        Debug.Log("CreateNewCat: ねこ作成モードに設定完了");
    }

    /// <summary>
    /// startButtonを表示する
    /// </summary>
    public void ShowStartButton()
    {
        Debug.Log("ShowStartButton: スタートボタンを表示");
        // このメソッドは「ボタンを表示する」だけの役割
        // 実際の処理は、ユーザーがボタンを押した時にStartButton()で実行される
        startButton.SetActive(true);
    }

    IEnumerator LoadImage(string url)
    {
        // URLが空またはnullの場合は処理をスキップ
        if (string.IsNullOrEmpty(url))
        {
            Debug.Log("プロフィール画像URLが空のため、ロードをスキップします。");
            yield break;
        }

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            // リクエストを送信し、レスポンスを待つ
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // バイトデータをテクスチャに変換
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(webRequest.downloadHandler.data))
                {
                picture.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    Debug.LogError("GIFデータをテクスチャに変換できませんでした。");
                }
            }
        }
    }

    public void googleLogout()
    {
        standupButton.SetActive(false);
        nextButton.SetActive(false);
        prevButton.SetActive(false);
        confirmButton.SetActive(false);

        // ゲームデータ全体をクリア（アカウント切り替え時のデータ分離）
        ClearGameData();

        // 猫の装備をリセット
        cat.setChara(0);
        cat.changeEquipHands(0, 0, gm.checkBagItem());
        cat.changeEquipHead(0);
        cat.changeEquipGlasses(0);

        // AuthManagerのログアウト処理を実行
        var authManager = FindObjectOfType<necotapuFB.AuthManager>();
        if (authManager != null)
        {
            authManager.Logout();
            Debug.Log("authManager.Logout()を呼び出しました");
        }
        else
        {
            Debug.LogWarning("AuthManagerが見つかりません");
        }
        
        // ログアウト完了処理
        FinishLogout();
    }

    public void FinishLogout()
    {
        loginFlg = 0;
        ashiato.SetActive(false);
        userData.SetActive(false);
        reLogin.SetActive(false);
        startButton.SetActive(true);
        guestButton.SetActive(true);
        Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = "ログアウトしました。";

        TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = "ログイン";
    }

    public void handleDataError()
    {
        Debug.Log("handleDataError");
        checkLocalData();
    }

    public void OnRequestTimeout()
    {
        Debug.Log("OnRequestTimeout");
        checkLocalData();
    }

    public void handleInitialData()
    {
        Debug.Log("handleInitialData");
        checkLocalData();
    }

    private void selectNeco()
    {
        Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = "さぁいっしょにタイピングをするねこをえらんでね。";
        animator.SetBool("Standup", true);
        message.SetActive(true);
        standupButton.SetActive(true);
        nextButton.SetActive(true);
        prevButton.SetActive(true);
        confirmButton.SetActive(true);
        startButton.SetActive(false);
    }

    public void confirmNeco()
    {
        gm.savedata.setNewDataFB(mailText.text, firstName.text, lastName.text, ouText.text, necoNo);
        Debug.Log("confirmNeco: ねこを決定してFirebaseに初期値保存");
        
        // 装備データの確認
        Debug.Log($"confirmNeco: 設定後の既存配列装備情報 - CatBody: {gm.savedata.Equipment[eq.CatBody]}, RightHand: {gm.savedata.Equipment[eq.RightHand]}, LeftHand: {gm.savedata.Equipment[eq.LeftHand]}, Head: {gm.savedata.Equipment[eq.Head]}, Glasses: {gm.savedata.Equipment[eq.Glasses]}");
        
        // Firebaseに初期データを保存（SaveData.csのメソッドを使用）
        gm.savedata.saveToFirebase();

        // 保存したばかりのデータを直接利用してデータロード完了処理を実行
        string savedDataJson = gm.savedata.SerializeForFirebase();
        FinishDataLoadExtStatus(savedDataJson);

        // UIボタンを非表示
        standupButton.SetActive(false);
        nextButton.SetActive(false);
        prevButton.SetActive(false);
        confirmButton.SetActive(false);
        reLogin.SetActive(false);
        
        // StartButtonの表示とテキスト設定、loginFlgの設定はFinishDataLoadExtStatus内で適切に行われる

        // メッセージを更新
    }
    
    public void updownNeco()
    {
        TMP_Text standText = standupButton.GetComponentInChildren<TMP_Text>();
        if (animator.GetBool("Standup"))
        {
            standText.text = "↑";
            animator.SetBool("Standup", false);
        }
        else
        {
            standText.text = "↓";
            animator.SetBool("Standup", true);
        }
    }
    public void nextNeco()
    {
        necoNo++;
        if (necoNo > 209)
        {
            necoNo = 201;
        }
        cat.setChara(necoNo);
    }
    public void prevNeco()
    {
        necoNo--;
        if (necoNo < 201)
        {
            necoNo = 209;
        }
        cat.setChara(necoNo);
    }

    public void onGuestMode()
    {
        if (!firstPush)
        {
            // ゲストモードはキーボードを大文字にする
            gm.savedata.Settings[se.Capital] = 1;
            
            GameManager.guestMode = true;
            GameManager.TypingDataPath = "TextCustom/ehontenEvent";
            GameManager.SceneNo = (int)scene.Typing;
            SceneManager.LoadScene("typingStage"); // タイピングシーンに遷移
            firstPush = true;
        }
    }

    public void onHeijoMode()
    {
        GameManager.eventHeijo = true;
        onGuestMode();
    }
}
