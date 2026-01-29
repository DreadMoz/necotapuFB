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
        
        // AuthManagerの認証成功イベントを購読
        var authManager = FindObjectOfType<necotapuFB.AuthManager>();
        if (authManager != null)
        {
            authManager.OnAuthenticationSuccess += OnAuthSuccess;
            // 認証失敗時（未ログイン時含む）も購読してボタンを再表示する
            authManager.OnAuthenticationFailed += OnAuthFailed;
            Debug.Log("OnAuthenticationSuccess/Failedイベントを購読しました");
            
            // 起動直後の認証チェック中はボタンを非表示にする
            if (authManager.IsAuthenticating)
            {
                startButton.SetActive(false);
                guestButton.SetActive(false);
                
                // メッセージボックスを表示
                if (message != null)
                {
                    message.SetActive(true);
                    Text messageText = message.GetComponentInChildren<Text>();
                    if (messageText != null)
                    {
                        messageText.text = "自動ログインチェック中...";
                    }
                }
            }
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
        // 認証中はボタン操作を受け付けない
        var authManager = FindObjectOfType<necotapuFB.AuthManager>();
        if (authManager != null && authManager.IsAuthenticating)
        {
            Debug.Log("認証処理中のためボタン操作を無視します");
            return;
        }

        if (loginFlg == 0)
        {
            startButton.SetActive(false);   // ログイン完了まで一旦消す
            guestButton.SetActive(false);   // ログイン完了まで一旦消す
            // AuthManagerのGoogle認証を呼び出し
            // var authManager = FindObjectOfType<necotapuFB.AuthManager>(); // 重複宣言のため削除
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
    /// 認証失敗時（未ログイン含む）の処理
    /// </summary>
    public void OnAuthFailed(necotapuFB.AuthResult result)
    {
        Debug.Log($"TitleSky: 認証チェック完了 - 未ログイン (Result: {result})");
        
        Text messageText = message.GetComponentInChildren<Text>();
        if (messageText != null)
        {
            messageText.text = "ログインしてはじめよう。";
        }

        // ボタンを再表示して手動ログインを可能にする
        startButton.SetActive(true);
        guestButton.SetActive(true);
        loginFlg = 0;
        
        TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null) buttonText.text = "ログイン";
    }

    /// <summary>
    /// Unityエディタ環境での認証成功時の処理 
    /// </summary>
    public void OnAuthSuccess(necotapuFB.AuthInfo authInfo)
    {
        Debug.Log($"OnAuthenticationSuccess: 認証成功データ受信 - UserID: {authInfo.userId}");

        // 名前が取得できていない場合はエラーを表示して中断
        if (string.IsNullOrEmpty(authInfo.displayName) && !authInfo.isGuest)
        {
            Debug.LogError("TitleSky: 認証情報に名前が含まれていません。Firestoreへの登録を制限します。");
            Text messageTextErr = message.GetComponentInChildren<Text>();
            if (messageTextErr != null)
            {
                messageTextErr.text = "エラー：Googleアカウントから名前を取得できませんでした。ページを再読み込みしてやり直してください。";
            }
            message.SetActive(true);
            startButton.SetActive(false); // ボタンを隠して進行不能にする
            return;
        }

        Text messageText = message.GetComponentInChildren<Text>();
        if (messageText != null)
        {
            messageText.text = "スタートしよう。";
        }

        // ログイン成功状態にする
        loginFlg = 1;
        // ボタンを表示（STARTボタンとして）
        startButton.SetActive(true);
        guestButton.SetActive(false); // ゲストボタンは不要なので非表示のまま

        TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null) buttonText.text = "スタート";

        // ... (以下既存の処理があれば)
        
        // ログインフラグを設定
        loginFlg = 1;
        
        // UIに認証情報を表示
        userData.SetActive(true);
        message.SetActive(true);
        
        // 表示名からfirstNameとlastNameを分割
        string firstNameStr = "";
        string lastNameStr = "";

        if (!string.IsNullOrEmpty(authInfo.displayName))
        {
            string[] nameParts = authInfo.displayName.Split(' ');
            firstNameStr = nameParts.Length > 0 ? nameParts[0] : "";
            lastNameStr = nameParts.Length > 1 ? nameParts[1] : "";
            
            // 名字がない（スペースがない）場合は、displayName全体をfirstNameに入れる
            if (string.IsNullOrEmpty(lastNameStr))
            {
                firstNameStr = authInfo.displayName;
                lastNameStr = " "; // 空文字を避ける
            }
        }
        else if (authInfo.isGuest)
        {
            firstNameStr = "ゲスト";
            lastNameStr = "さん";
            Debug.Log("TitleSky: ゲストユーザーとしてログイン");
        }
        else if (!string.IsNullOrEmpty(authInfo.email))
        {
            // 名前が取得できない場合はメールアドレスの@前を使用
            firstNameStr = authInfo.email.Split('@')[0];
            lastNameStr = " ";
            Debug.LogWarning($"TitleSky: displayNameが空のためメールアドレスから名前を生成: {firstNameStr}");
        }
        else
        {
            firstNameStr = "名無し";
            lastNameStr = "さん";
            Debug.LogError("TitleSky: 認証情報に名前もメールアドレスもありません。");
        }
        
        // 認証情報を保存
        currentFirstName = firstNameStr;
        currentLastName = lastNameStr;
        currentEmail = authInfo.email;
        
        // UIに表示
        mailText.text = authInfo.email;
        this.firstName.text = firstNameStr;
        this.lastName.text = lastNameStr;
        department.text = authInfo.authMethod; // 認証方法を表示
        ouText.text = authInfo.authMethod; // 追加：新規ユーザー時の保存用にセット
        
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

        userData.SetActive(true);   // ユーザー情報を表示
        reLogin.SetActive(true);    // ログアウトボタン表示
        
        Debug.Log($"ログイン情報表示完了: {authInfo.email} ({authInfo.displayName})");
        
        // データロードを開始
        if (firestoreConnection == null)
        {
            Debug.LogError("FirestoreConnectionが見つかりません");
        }
        else
        {
            firestoreConnection.LoadUserData();
        }
    }

    /// <summary>
    /// 強制再読み込みが必要な場合にAppVersionManagerから呼ばれるハンドラ
    /// </summary>
    private void OnForceReloadRequiredHandler()
    {
        Debug.Log("TitleSky: 強制再読み込みが必要なイベントを受信しました。");
        Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = "最新のアプリのバージョンが見つかりました。ロードします。";
        message.SetActive(true); // メッセージウィンドウを表示
    }
    
    /// <summary>
    /// バージョン比較結果：Firebaseデータを使用
    /// </summary>
    public void SetUserData(string jsonData)
    {
        message.SetActive(true);
        Text messageText = message.GetComponentInChildren<Text>();

        try
        {
            // Firebaseからロードしたデータを直接ゲームデータに設定
            gm.savedata.DeserializeFromFirebaseOrLocal(jsonData);
            
            // ユーザー名を最新のGoogleアカウント情報で更新
            UpdateUserInfoFromAuth();
            
            ouText.text = gm.savedata.AuthMethod;
            CheckDailyReset();
            
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
            messageText.text = "データを読み込む時にエラーが発生しました: ";
            Debug.LogError("データの読み込み中に例外発生: " + ex);
        }
    }
    

    // 認証情報を保存する変数
    private string currentFirstName = "";
    private string currentLastName = "";
    private string currentEmail = "";

    /// <summary>
    /// ロード完了メッセージを表示
    /// </summary>
    /// <param name="source">データのロード元 ('firebase' または 'local')</param>
    public void DisplayLoadCompleteMessage(string source)
    {
        Text messageText = message.GetComponentInChildren<Text>();
        if (source == "firebase")
        {
            messageText.text = "サーバーからデータをロードしたよ。スタートしよう！";
        }
        else if (source == "local")
        {
            messageText.text = "ブラウザからデータをロードしたよ。スタートしよう！";
        }
        else if (source == "new_user") // 新規ユーザー用のメッセージを追加
        {
            messageText.text = "新しいねこが生まれたよ！ゲームをスタートしよう！";
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
        if (!string.IsNullOrEmpty(currentFirstName))
        {
            // 現在のFirebaseデータとGoogleアカウント情報を比較
            bool needsUpdate = false;
            
            // 名前の更新（不一致の場合）
            if (gm.savedata.UserName != currentFirstName)
            {
                Debug.Log($"ユーザー名を更新: '{gm.savedata.UserName}' → '{currentFirstName}'");
                gm.savedata.UserName = currentFirstName;
                this.firstName.text = currentFirstName; // UIも更新
                needsUpdate = true;
            }
            
            if (gm.savedata.LastName != currentLastName)
            {
                Debug.Log($"姓を更新: '{gm.savedata.LastName}' → '{currentLastName}'");
                gm.savedata.LastName = currentLastName;
                this.lastName.text = currentLastName; // UIも更新
                needsUpdate = true;
            }
            
            if (gm.savedata.Email != currentEmail)
            {
                Debug.Log($"メールアドレスを更新: '{gm.savedata.Email}' → '{currentEmail}'");
                gm.savedata.Email = currentEmail;
                mailText.text = currentEmail; // UIも更新
                needsUpdate = true;
            }

            // AuthMethodが空の場合の救済
            if (string.IsNullOrEmpty(gm.savedata.AuthMethod) || gm.savedata.AuthMethod == "unknown")
            {
                string method = string.IsNullOrEmpty(ouText.text) ? "google.com" : ouText.text;
                Debug.Log($"認証方法を更新: '{gm.savedata.AuthMethod}' → '{method}'");
                gm.savedata.AuthMethod = method;
                ouText.text = method; // UIも更新
                needsUpdate = true;
            }
            
            if (needsUpdate)
            {
                Debug.Log("ユーザー情報が自動修正されました - Firebaseに保存を実行");
                // 更新された情報をFirebaseに保存
                gm.savedata.saveInitialDataToFirebase();
            }
            else
            {
                Debug.Log("ユーザー情報に変更はありません");
            }
        }
        else
        {
            Debug.LogWarning("認証情報が不完全なため、自動修正をスキップしました。");
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

    /// <summary>
    /// 既存データがある場合の処理
    /// </summary>
    private void UseExistingData()
    {
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
        TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = "スタート";
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
        // 最終チェック：名前が空の場合は絶対に保存させない
        if (string.IsNullOrEmpty(currentFirstName) || currentFirstName == "名無し")
        {
            Debug.LogError("confirmNeco: 名前が不完全なため保存を中止しました。");
            return;
        }

        // ゲストユーザーの場合はFirestoreへの初期保存をスキップ
        necotapuFB.AuthManager authManager = FindObjectOfType<necotapuFB.AuthManager>();
        if (authManager != null && authManager.CurrentAuthInfo != null && authManager.CurrentAuthInfo.isGuest)
        {
            Debug.Log("confirmNeco: ゲストユーザーのためFirestoreへの保存をスキップし、ローカルデータで開始します。");
            // SaveDataの初期化だけ行う
            gm.savedata.setNewDataFB("", currentFirstName, currentLastName, "Guest", necoNo);
            
            // UIボタンを非表示
            standupButton.SetActive(false);
            nextButton.SetActive(false);
            prevButton.SetActive(false);
            confirmButton.SetActive(false);
            reLogin.SetActive(false);
            
            // StartButtonの表示とテキスト設定
            TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = "スタート";
            startButton.SetActive(true);
            loginFlg = 1;
            
            DisplayLoadCompleteMessage("new_user");
            return;
        }

        // UIのテキストではなく、保持している確実な認証情報を使用する
        gm.savedata.setNewDataFB(currentEmail, currentFirstName, currentLastName, ouText.text, necoNo);
        Debug.Log($"confirmNeco: ねこを決定してFirebaseに初期値保存 (User: {currentFirstName}, Email: {currentEmail}, Auth: {ouText.text})");
        
        // 装備データの確認
        Debug.Log($"confirmNeco: 設定後の既存配列装備情報 - CatBody: {gm.savedata.Equipment[eq.CatBody]}, RightHand: {gm.savedata.Equipment[eq.RightHand]}, LeftHand: {gm.savedata.Equipment[eq.LeftHand]}, Head: {gm.savedata.Equipment[eq.Head]}, Glasses: {gm.savedata.Equipment[eq.Glasses]}");
        
        // Firebaseに初期データを保存（SaveData.csのメソッドを使用）
        gm.savedata.saveInitialDataToFirebase();

        // 保存したばかりのデータを直接利用してデータロード完了処理を実行
        string savedDataJson = gm.savedata.SerializeForFB();
        DisplayLoadCompleteMessage("new_user"); // 新規作成完了メッセージを表示

        // UIボタンを非表示
        standupButton.SetActive(false);
        nextButton.SetActive(false);
        prevButton.SetActive(false);
        confirmButton.SetActive(false);
        reLogin.SetActive(false);
        
        // StartButtonの表示とテキスト設定、loginFlgの設定を行う
        TMP_Text startBtnText = startButton.GetComponentInChildren<TMP_Text>();
        startBtnText.text = "スタート"; // テキストを「スタート」に設定
        startButton.SetActive(true); // スタートボタンをアクティブにする
        loginFlg = 1; // ログインフラグを1（既存ユーザー状態）に設定
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

    /// <summary>
    /// ユーザデータの最新性を判定（アイテム数 → ゴールド数の順）
    /// FirebaseBridge.jslibから移動したロジックをC#で実装
    /// </summary>
    /// <param name="localDataJson">ローカルデータのJSON文字列</param>
    /// <param name="firebaseDataJson">FirebaseデータのJSON文字列</param>
    /// <returns>"local" または "firebase"</returns>
    // CompareUserDataVersionInternal メソッドを削除します (JSLibに移動したため)
}
