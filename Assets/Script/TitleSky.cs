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
        gm.savedata.Equipment[eq.CatBody] = 0;
        gm.savedata.Settings[se.GachaCnt] = 1;      // ボーナスダイヤの初期値

//        startButton.SetActive(false);   // ログイン完了まで一旦消す
//        StartButton();
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
//        gm.savedata.testEncodeMedals();   // Medalデバッグ
        if (loginFlg == 0)  // 初回ログイン時
        {
            startButton.SetActive(false);   // ログイン完了まで一旦消す
            guestButton.SetActive(false);   // ログイン完了まで一旦消す
            gm.connection.enetLogin();    // OAuthログインHTMLへ
        }
        else if (loginFlg == 1) // ２回目以降のログイン時
        {
            if (!firstPush)
            {
                fade.StartFadeOut();    // ワールドシーンへのフェードアウト
                firstPush = true;
            }
        }
        else if (loginFlg == 2) // 初回「つくる」時
        {
            selectNeco();   // ねこ選び
        }
    }

    // gm.connection.enetLogin() OAuthログインの後HTMLから(GAS管理者権限でアカウント情報取得後)
    public void finishOAuth(string jsonUserInfo)
    {
        userData.SetActive(true);
        message.SetActive(true);
        Text messageText = message.GetComponentInChildren<Text>();

        UserInfo userInfo = JsonUtility.FromJson<UserInfo>(jsonUserInfo);
        mailText.text = userInfo.email;
        firstName.text = userInfo.firstName;
        lastName.text = userInfo.lastName;
        department.text = userInfo.department;
        StartCoroutine(LoadImage(userInfo.picture));
        messageText.text = userInfo.message;


        if (userInfo.access == "true")  // いいネットなら照合成功
        {
            gm.connection.loadLocal(); // ローカルデータをロードする 自動でHTMLとのやり取りが続く
        }
        reLogin.SetActive(true); // ログアウトボタン表示
    }

    // localStrageからデータの読み込みに成功した時呼ばれる
    public void finishDataLoadExtStatus(string statusDataJson)
    {
        message.SetActive(true);
        Text messageText = message.GetComponentInChildren<Text>();

        try
        {
            if (statusDataJson != null)
            {
                gm.savedata.setStatusFromLocal(statusDataJson);
                ouText.text = gm.savedata.Ou;
                CheckDailyReset();
                Debug.Log("ステータスデータをロードしました。");
                if (gm.savedata.getTotalMedal() >= 264)     // 0~65 * 4
                {
                    ashiato.SetActive(true);
                }
                checkLocalData();
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

        if (gm.savedata.Equipment[eq.CatBody] == 0)        // ねこボディあるかないかでlocalStrageの正当性を判定
        {
            Debug.Log("ネコボディなしGASアクセスへ");
            messageText.text += "クラウドにデータがあるかさがしてきます・・・";
            gm.savedata.Settings[se.CatNum] = 0;        // NPC表示なし
            gm.connection.loadGas();    // GSSアクセス。
        }
        else
        {
            if (lastName.text != gm.savedata.LastName) {    // ラストネームが更新されていたら
                gm.savedata.updateLastName(lastName.text);  // メモリのラストネーム更新
                string saveLocalJson = gm.savedata.CompileGameDataForLocal(gm.savedata);    // localStrage保存用データ作成
                gm.connection.saveLocal(saveLocalJson);     // localStrage更新
            }
            Debug.Log("拡張機能正常データあり");
            messageText.text += "ほぞんデータがみつかったよ。スタートしましょう。";
            showStart();
        }
    }

    public void finishDataLoadGas(string jsonMsg)
    {
        reLogin.SetActive(true);
        Text messageText = message.GetComponentInChildren<Text>();

        if (string.IsNullOrEmpty(jsonMsg))
        {
            messageText.text = "クラウドにデータがありませんでした。";
            showStart();
        }
        else
        {
            SerializableSympleStatusData userData = JsonUtility.FromJson<SerializableSympleStatusData>(jsonMsg);

            if (userData != null)
            {
                List<object> dataList = new List<object> {
                    userData.email,
                    userData.ou,
                    userData.lastName,
                    userData.gold,
                    userData.stage,
                    userData.ranking,
                    userData.name,
                    userData.rightHand,
                    userData.glasses,
                    userData.head,
                    userData.leftHand,
                    userData.catBody,
                    userData.catFace,
                    userData.nickName,
                    userData.kpm,
                    userData.kpms
                };

                dataList.AddRange(userData.medals);
                dataList.AddRange(userData.items);

                gm.savedata.LoadAllDataFromGss(dataList);
                Debug.Log("dataList: " + dataList);
                messageText.text = "クラウドデータを読み込みました。";
            }
            else
            {
                messageText.text += "\nクラウドデータに問題が生じました。";
            }
            showStart();
        }
    }

    private void showStart()
    {
        if (gm.savedata.Equipment[eq.CatBody] != 0)
        {
            gm.connection.getRanking();
            cat.setChara(gm.savedata.Equipment[eq.CatBody]);
            cat.changeEquipHands(gm.savedata.Equipment[eq.RightHand], gm.savedata.Equipment[eq.LeftHand], gm.checkBagItem());
            cat.changeEquipHead(gm.savedata.Equipment[eq.Head]);
            cat.changeEquipGlasses(gm.savedata.Equipment[eq.Glasses]);
            TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = "スタート";
            loginFlg = 1;
        }
        else
        {
            Text messageText = message.GetComponentInChildren<Text>();
            messageText.text += "\nあたらしく" + firstName.text + "さんのデータをつくりましょう。";
            TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
            buttonText.text = "つくる";
            loginFlg = 2;
        }
        startButton.SetActive(true);
    }

    IEnumerator LoadImage(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            // リクエストを送信し、レスポンスを待つ
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error);
            }
            else
            {
                // 正常に画像を取得できた場合、TextureをImageに設定する
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                picture.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }

    public void googleLogout()
    {
        standupButton.SetActive(false);
        nextButton.SetActive(false);
        prevButton.SetActive(false);
        confirmButton.SetActive(false);

        gm.savedata.Equipment[eq.CatBody] = 0;

        cat.setChara(gm.savedata.Equipment[eq.CatBody]);
        cat.changeEquipHands(0, 0, gm.checkBagItem());
        cat.changeEquipHead(0);
        cat.changeEquipGlasses(0);

        gm.connection.googleLogout();
    }

    public void finishLogout()
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
        gm.savedata.Equipment[eq.CatBody] = necoNo;

        standupButton.SetActive(false);
        nextButton.SetActive(false);
        prevButton.SetActive(false);
        confirmButton.SetActive(false);
        startButton.SetActive(true);

        gm.savedata.setNewData(mailText.text, firstName.text, lastName.text, ouText.text);
        gm.exportLocal();  // ねこ決定後のデータ保存ローカル＆GSS

        Text messageText = message.GetComponentInChildren<Text>();
        messageText.text = "あたらしいデータをつくりました。スタートしましょう。";
        showStart();
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
            GameManager.guestMode = true;
            GameManager.TypingDataPath = "TextCustom/heijoEvent";
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
