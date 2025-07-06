using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System.Globalization;
using static TypingSoft;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using Shapes2D;
using System.Linq;
using System.Diagnostics.Tracing;

public class TypingSoft : MonoBehaviour
{
    [SerializeField] private GameManager gm;
    [SerializeField] private TypingVoice typingVoice;
    [SerializeField] private GameObject dia1;
    [SerializeField] private GameObject dia2;
    [SerializeField] private GameObject dia3;
    [SerializeField] private GameObject dia4;
    [SerializeField] private GameObject dia5;
    [SerializeField] private GameObject dice;
    // Assist Keyboard JIS
    private static AssistKeyboardJIS AssistKeyboardObj;

    [SerializeField]
    private GameObject lPlayer;       // プレイヤーオブジェクト
    [SerializeField]
    private GameObject player;        // プレイヤーオブジェクト
    [SerializeField]
    private Coins coins;           // コイン操作
    [SerializeField]
    private GameObject door;
    [SerializeField]
    private GameObject suzakuBack;

    int[,] diceProbabilities = new int[5, 6] {
        {20, 50, 75, 95, 99,100}, 
        {10, 35, 60, 85, 95,100}, 
        { 5, 20, 45, 75, 95,100}, 
        { 0,  5, 15, 45, 75,100}, 
        { 0,  0,  5, 15, 50,100}
    };
    private Animator animator;
    private Animator lAnimator;
    private Animator diaAnim1;
    private Animator diaAnim2;
    private Animator diaAnim3;
    private Animator diaAnim4;
    private Animator diaAnim5;
    private Animator diceAnim;
    public GameObject targetCam;
    public GameObject testMode;
    private float necoSpeed = 0.4f;

    private bool goNextScene = false;    // 次のシーンに遷移するためのフラグ
    

    private int seekerStart;
    private int seekerCombo;
    private int seekerKey;
    private int seekerTime;
    private int seekerBonus;
    private int totalSeeker;
    private int maxCombo = 0;

    public float totalTime = 1.0f; // タイマーの総時間（秒）
    private float currentTime; // 現在の経過時間
    private bool isTimerRunning = false; // タイマーが実行中かどうかのフラグ

    private static bool spaceStart;
    private static bool spaceEnd;
    private static bool spaceThrow;
    private int diceNo;
    private int diceSeeker;
    private int diaRank;

    public Animator[] animators; // 各オブジェクトのアニメーターへの参照を格納する配列
    public string[] states; // 各オブジェクトのアニメーションの名前の配列
    public string[] diceStates; // 各オブジェクトのアニメーションの名前の配列
    // 入力受け付け
    private static bool isInputValid;
    // タイピングの正誤判定器
    private static List<List<string>> typingJudge;
    // index 類
    private static int index;
    private static List<List<int>> indexAdd = new List<List<int>>();
    private static List<List<int>> sentenceIndex = new List<List<int>>();
    private static List<List<int>> sentenceValid = new List<List<int>>();
    public static string CurrentTypingSentence { private set; get; } = "";

    // ミスタイプ記録
    private static bool isRecMistype;
    private static bool isSentenceMistyped;

    //　問題の日本語文
    private string[] qJ;
    //　問題のひらがな文
    private string[] qH;

    //　日本語表示テキスト
    private Text UIJ;
    //　ひらがな表示テキスト
    private Text UIH;
    //　ローマ字表示テキスト
    private TMP_Text UIR;
    //　終了表示テキスト
    private Text END;
    //　カウントダウン表示テキスト
    private Text UICountDown;
    //　残り時間表示テキスト
    private Text UITimer;

    //　日本語問題
    private string nQJ;
    //　ひらがな問題
    private string nQH;
    //　ローマ字問題
    private string nQR;


    //　問題番号
    private int numberOfQuestion;
    //　問題の何文字目か
    private int indexOfString;

    //　入力した文字列テキスト
    private TMP_Text UII;
    //　正解キー数
    private int correctN;
    //　コンボ数
    private int comboN;
    //　失敗数
    private int mistakeN;

    //　正解数表示用テキストUI
    private Text UIcorrect;
    //　正解した文字列を入れておく
    private string correctString;

    //　失敗数表示用テキストUI
    private Text UImistake;

    //　1分間あたりの入力キー数表示用テキストUI
    private Text UIkpm;
    private float kpm;
    private Text UIseeker;

    //　コンボ表示用テキストUI
    private TMP_Text UIcombo;
    private Text UIkonbo;

    // 回答数
    private int answers;
    //　正解率
    private float correctAR;
    //　正解率表示用テキストUI
    private Text UIcorrectAR;
    private bool firstEnd = true;
    // お題が一巡した回数
    private int returnCount;

    private ThemeCollection theme;
    private List<Theme> shuffledThemes = new List<Theme>();
    private int currentThemeIndex = 0;

    private List<Message> messages = new List<Message>();
    private int nextMessageNo;
    private Message nextMessage;
    private bool isForceQuit = false;
    private int mailReplaceNo = -1;


    [SerializeField] private GameObject lHand;
    [SerializeField] private GameObject rHand;
    [SerializeField] private GameObject ObjKPM;
    [SerializeField] private GameObject Objkpm;
    [SerializeField] private GameObject ObjTimer;
    [SerializeField] private GameObject Objnokori;
    [SerializeField] private GameObject input;

    [SerializeField] private GameObject Fukidashi;
    [SerializeField] private Text messageText;


    // 結果画面
    [SerializeField] private GameObject resultWindow;
    [SerializeField] private GameObject exitTyping;
    // 結果お題
    [SerializeField] private Text resultTitle;
    // 結果MaxCombo
    [SerializeField] private Text resultCombo;
    // 結果Kpm
    [SerializeField] private Text resultKpm;
    private GameObject panelMessage;

    [Serializable]
    public class Theme
    {
        public int id;
        public string kanji;
        public string hiragana;
    }
    [Serializable]
    public class Message
    {
        public int count;
        public string description;
    }

    [Serializable]
    public class ThemeCollection
    {
        public string title;
        public string description;
        public int timer;
        public int random;
        public int hide;
        public Message[] messages;
        public Theme[] themes;
    }


    void Start()
    {
        lPlayer.SetActive(false);
        Fukidashi.SetActive(false);

        // スペースでスタート状態にする
        spaceStart = true;
        // スペースでエンド状態を解除する
        spaceEnd = false;

        spaceThrow = false;
        // 入力禁止状態にする
        isInputValid = false;

#if UNITY_WEBGL && !UNITY_EDITOR
        testMode.SetActive(false);
#endif

        if (GameManager.guestMode)
        {
            suzakuBack.SetActive(true);
            exitTyping.SetActive(false);

            resultWindow = transform.Find("EventPanel").gameObject;
            resultTitle = transform.Find("ResultPanel/TitleText").GetComponent<Text>();
            resultCombo = transform.Find("ResultPanel/maxComboText").GetComponent<Text>();
            resultKpm = transform.Find("EventPanel/totalKpmText").GetComponent<Text>();
            panelMessage = transform.Find("EventPanel/Message").gameObject;
        }
        else
        {
            suzakuBack.SetActive(false);

            resultWindow = transform.Find("ResultPanel").gameObject;
            resultTitle = transform.Find("ResultPanel/TitleText").GetComponent<Text>();
            resultCombo = transform.Find("ResultPanel/maxComboText").GetComponent<Text>();
            resultKpm = transform.Find("ResultPanel/totalKpmText").GetComponent<Text>();
        }
        
        animator = player.GetComponent<Animator>(); // Playerのアニメーターを取得
        lAnimator = lPlayer.GetComponent<Animator>(); // Playerのアニメーターを取得
        diaAnim1 = dia1.GetComponent<Animator>();
        diaAnim2 = dia2.GetComponent<Animator>();
        diaAnim3 = dia3.GetComponent<Animator>();
        diaAnim4 = dia4.GetComponent<Animator>();
        diaAnim5 = dia5.GetComponent<Animator>();
        diceAnim = dice.GetComponent<Animator>();
        lAnimator.SetTrigger("jump");
        player.transform.LookAt(targetCam.transform);   // カメラを向く
        
        animator.SetFloat("walkSpeed", 1.0f);
        animator.SetFloat("moveSpeed", 1.0f);
        animator.SetFloat("runSpeed", 1.0f);

        //　テキストUIを取得
        UIJ = transform.Find("InputPanel/QuestionJ").GetComponent<Text>();
        UIH = transform.Find("InputPanel/QuestionH").GetComponent<Text>();
        UIR = transform.Find("InputPanel/QuestionR").GetComponent<TMP_Text>();
        UII = transform.Find("InputPanel/Input").GetComponent<TMP_Text>();
        END = transform.Find("InputPanel/End").GetComponent<Text>();
        UICountDown = transform.Find("InputPanel/CountDown").GetComponent<Text>();
        UIcorrect = transform.Find("DataPanel/Correct").GetComponent<Text>();
        UIcorrectAR = transform.Find("DataPanel/CorrectAR").GetComponent<Text>();
        UIcombo = transform.Find("DataPanel/Combo").GetComponent<TMP_Text>();
        UIkonbo = transform.Find("DataPanel/konbo").GetComponent<Text>();
        UIkpm = transform.Find("DataPanel/Kpm").GetComponent<Text>();
        UIseeker = transform.Find("DataPanel/Seeker").GetComponent<Text>();
        UImistake = transform.Find("DataPanel/Mistake").GetComponent<Text>();
        UITimer = transform.Find("DataPanel/Timer").GetComponent<Text>();
        AssistKeyboardObj = GameObject.Find("AssistKeyboard").GetComponent<AssistKeyboardJIS>();

        setDiaDisp(gm.savedata.Settings[se.GachaCnt]);
        
        //　データ初期化処理
        correctN = 0;
        comboN = 0;
        mistakeN = 0;
        answers = 0;

        // シーカーゲット用
        seekerCombo = 0;
        seekerKey = 0;
        seekerTime = 0;

        seekerStart = gm.savedata.Status[st.Gold];
        updateSeeker();

        AssistKeyboardObj.SetNextHighlight(" ");

        if (LoadThemes(GameManager.TypingDataPath))
        {
            setMessage();
            ShuffleThemes(theme.random);

            if (theme.timer > 0)    // 時間設定ありなら
            {
                // タイマーを初期化
                totalTime = theme.timer;
                currentTime = totalTime;
                UpdateTimerText();
            }
            else   // 時間設定なしモードなら
            {
                player.transform.position = new Vector3(-1 * player.transform.position.x, player.transform.position.y, player.transform.position.z);
                player.transform.LookAt(targetCam.transform);   // カメラを向く
                lPlayer.SetActive(true);
                currentTime = 0;        // タイマーはゼロにしておく。
                ObjKPM.SetActive(false);
                Objkpm.SetActive(false);
                ObjTimer.SetActive(false);
                Objnokori.SetActive(false);
                input.SetActive(false);
                spaceStart = false;     // 時間制限なしならスペースでスタート状態を解除
                UIH.text = "";
                NoTimerStart();     // タイマーなしスタート
            }
        }
    }

    private void setMessage()
    {
        if (theme.messages == null)
        {
            return;
        }
        messages = new List<Message>(theme.messages);
        if (messages != null && messages.Count != 0)
        {
            nextMessageNo = 0;
            nextMessage = messages[nextMessageNo];
        }
    }

    private void setDescriptionByNo(int targetCount)
    {
        if (theme.messages == null || theme.messages.Count() == 0)
        {
            return;
        }
        if (nextMessage.count == targetCount)     // 回答数がメッセージ表示番号になったら
        {
            lAnimator.SetTrigger("jump");
            Fukidashi.SetActive(true);
            messageText.text = nextMessage.description;      // メッセージ表示
            nextMessageNo++;
            if (nextMessageNo < messages.Count)              // 次のメッセージがあればセット
            {
                nextMessage = messages[nextMessageNo];
            }
            string randAtk = "atk" + (new System.Random().Next(1, 3)).ToString();
            animator.SetTrigger(randAtk);
        }
    }

    private bool LoadThemes(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);

        try
        {
            if (textAsset != null)
            {
                theme = JsonUtility.FromJson<ThemeCollection>(textAsset.text);
                if (theme == null)
                {
                    Debug.Log("JSONファイルの取得に失敗しました。");
                    return false;
                }
                if ((fileName.StartsWith("TextC")) && (gm.savedata.Settings[se.MailChar] != 0) && !GameManager.guestMode)
                {
                    combineCentence();
                    mailReplaceNo = new System.Random().Next(0, theme.themes.Length);
                    theme.themes[mailReplaceNo].hiragana = gm.savedata.Email;
                    theme.themes[mailReplaceNo].kanji = "ボーナス！";
                }
                return true;
            }
            else
            {
                Debug.Log("Cannot find file!");
                GameManager.TypingDataId = -1;
                if (!goNextScene)
                {
                    GameManager.SceneNo = (int)scene.House;   // ワールドシーンショップ
                    if (GameManager.eventHeijo)
                    {
                        GameManager.SceneNo = (int)scene.Heijo;   // ワールドシーン平城京
                        GameManager.eventHeijo = false;
                    }
                    SceneManager.LoadScene("WorldScene"); // ワールドシーンに遷移
                    goNextScene = true;
                }
                return false;
            }
        }
        catch(Exception ex)
        {
            Debug.Log("不正なJSONファイルです。");
            Debug.Log("Exception occurred: " + ex.Message);
            return false;
        }
    }

    private void combineCentence()
    {
        if (theme.hide < 10)
        {
            return;
        }
        int finalLength = theme.themes.Length - theme.hide;

        List<int> numbers = new List<int>();
        for (int i = 0; i < finalLength; i++)
        {
            numbers.Add(i);
        }
        // リストの要素をシャッフル
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = new System.Random().Next(0, i + 1);
            int temp = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = temp;
        }
        for (int i = 0; i < theme.hide; i++)
        {
            if (theme.themes[theme.hide + numbers[i]] != null)
            {
                theme.themes[i].hiragana += theme.themes[theme.hide + numbers[i]].hiragana;
                theme.themes[i].kanji += theme.themes[theme.hide + numbers[i]].kanji;
            }
        }
        Array.Resize(ref theme.themes, theme.hide);
        theme.hide = 0;
    }

    private void ShuffleThemes(int shuffle)
    {
        if (theme != null && theme.themes.Length > 0)
        {
            int firstData;
            if (theme.themes.Length < shuffle)
            {
                firstData = theme.themes.Length;      // 項目数を超えていたら項目数を上限に
            }
            else
            {
                firstData = shuffle;      // 前半のランダム部分の開始id1なら全て
            }

            shuffledThemes = new List<Theme>(theme.themes);
            if (shuffle == 0)
            {
                return;                     // theme.random = 0ならシャッフルしない
            }
            for (int i = 0; i < firstData; i++)             // 前半部分をシャッフル
            {
                Theme temp = shuffledThemes[i];
                int randomIndex = UnityEngine.Random.Range(i, firstData-1);
                shuffledThemes[i] = shuffledThemes[randomIndex];
                shuffledThemes[randomIndex] = temp;
            }
            for (int i = firstData; i < shuffledThemes.Count; i++)  // 後半部分をシャッフル
            {
                Theme temp = shuffledThemes[i];
                int randomIndex = UnityEngine.Random.Range(i, shuffledThemes.Count);
                shuffledThemes[i] = shuffledThemes[randomIndex];
                shuffledThemes[randomIndex] = temp;
            }
        }
    }

    private void updateSeeker()
    {
        totalSeeker = seekerStart + seekerCombo + seekerKey + seekerTime + seekerBonus;
        UIseeker.text = totalSeeker.ToString();
    }

    private void checkSeekerCombo()
    {
        if (comboN == 0)
        {
            return;
        }
        if (comboN % 25 == 0)
        {
            if (comboN == 25)
            {
                return;     // 最初は50スタート
            }
            seekerCombo++;
            coins.SpawnCoins(1, 1);    // コインアニメーション
            typingVoice.sayCoin();
            updateSeeker();
            float comboScale = ((float)comboN / 50.0f + 4) / 5;
            UIcombo.rectTransform.localScale = new Vector3(comboScale, comboScale, comboScale);

            // UIcomboの位置に基づいて、UIkonboの新しい位置を計算
            float newXPosition = UIcombo.rectTransform.localPosition.x + UIcombo.rectTransform.rect.width * comboScale * 0.7f ;
            UIkonbo.rectTransform.localPosition = new Vector3(newXPosition, UIkonbo.rectTransform.localPosition.y, 0);
        }
    }

    private void checkSeekerMail()
    {
        if ((currentThemeIndex > 0) && (mailReplaceNo != -1))
        {
            if (shuffledThemes[currentThemeIndex-1].id == mailReplaceNo + 1)
            {
                seekerBonus += 5;
                coins.SpawnCoins(5, 0);    // コインアニメーション
                typingVoice.sayCoin3();
                updateSeeker();
            }
        }
    }

    private void checkSeekerKey()
    {
        switch (seekerKey)
        {
            case 0:
                if (correctN >= 10)
                {
                    seekerKey = 1;
                    coins.SpawnCoins(1, 2);    // コインアニメーション
                    typingVoice.sayCoin();
                    updateSeeker();
                }
                break;
            case 1:
                if (correctN >= 20)
                {
                    seekerKey = 2;
                    coins.SpawnCoins(1, 2);    // コインアニメーション
                    typingVoice.sayCoin();
                    updateSeeker();
                }
                break;
            case 2:
                if (correctN >= 40)
                {
                    seekerKey = 3;
                    coins.SpawnCoins(1, 2);    // コインアニメーション
                    typingVoice.sayCoin();
                    updateSeeker();
                }
                break;
            case 3:
                if (correctN >= 65)
                {
                    seekerKey = 4;
                    coins.SpawnCoins(1, 2);    // コインアニメーション
                    typingVoice.sayCoin();
                    updateSeeker();
                }
                break;
            case 4:
                if (correctN >= 110)
                {
                    seekerKey = 5;
                    coins.SpawnCoins(1, 2);    // コインアニメーション
                    typingVoice.sayCoin();
                    updateSeeker();
                }
                break;
            case 5:
                if (correctN >= 170)
                {
                    seekerKey = 6;
                    coins.SpawnCoins(1, 2);    // コインアニメーション
                    typingVoice.sayCoin();
                    updateSeeker();
                }
                break;
            case 6:
                if (correctN >= 240)
                {
                    seekerKey = 7;
                    coins.SpawnCoins(1, 2);    // コインアニメーション
                    typingVoice.sayCoin();
                    updateSeeker();
                }
                break;
            case 7:
                if (correctN >= 330)
                {
                    seekerKey = 8;
                    coins.SpawnCoins(1, 2);    // コインアニメーション
                    typingVoice.sayCoin();
                    updateSeeker();
                }
                break;
        }
    }

    private void checkSeekerTimer()
    {
        if (theme.timer > 0)    // 時間設定ありなら
        {
            seekerTime = (int)(totalTime / 10);
        }
        else
        {
            seekerTime = 2;
        }
        coins.SpawnCoins(seekerTime, 0);    // コインアニメーション
        typingVoice.sayCoin();
        updateSeeker();
    }

    private IEnumerator ChangeSentence()
    {
        // コンボ数リセット、表示更新
        if (totalTime != currentTime)
        {
            kpm = correctN / (totalTime - currentTime) * 60.0f;
            UIkpm.text = string.Format("{0:0}", kpm);
        }
        checkSeekerMail();
        checkSeekerKey();

        if (shuffledThemes.Count > 0)
        {
            Theme currentTheme = shuffledThemes[currentThemeIndex];

            // 選択した問題をテキストUIにセット
            nQJ = currentTheme.kanji;
            nQH = currentTheme.hiragana;

            setDescriptionByNo(currentThemeIndex);
            // 次のお題に移動
            currentThemeIndex++;
            if ((currentThemeIndex >= shuffledThemes.Count) && (theme.random > 0))
            {
                ShuffleThemes(theme.random);
                currentThemeIndex = 1; // リストの最初に戻る
                returnCount++;
            }
        }
        else
        {
            Debug.LogError("No themes to display!");
        }
        bool isGenerateSuccess;

        // Generate() 関数を呼び出す
        (isGenerateSuccess, nQR, typingJudge) = GenerateSentence.Generate(nQH);

        // 判定器などの初期化
        InitSentenceData();

        if (theme.hide < 1)
        {
            UIH.text = nQH;     // ひらがな表示
            if (gm.savedata.Settings[se.Capital] == 1)
            {
                UIR.text = nQR.ToUpper();     // ローマ字表示（大文字）
            }
            else
            {
                UIR.text = nQR;     // ローマ字表示（小文字）
            }
        }
        CurrentTypingSentence = nQR;
        UIJ.text = nQJ;     // 日本語表示
        // 変数等の初期化
        isRecMistype = false;
        isSentenceMistyped = false;
        index = 0;

        var nextHighlight = typingJudge[0][0][0].ToString();

        AssistKeyboardObj.SetNextHighlight(nextHighlight);

        yield return new WaitForSeconds(0.01f);  // なんかとりあえず
    }

    private void UpdateTimerText()
    {
        // 残り時間を表示
        UITimer.text = string.Format("{0:0}", currentTime);
    }

    /// <summary>
    /// カウントダウン演出
    /// </summary>
    private IEnumerator CountDown()
    {
        animator.SetTrigger("kamae");
        // キーカラークリア
        AssistKeyboardObj.SetAllKeyColorWhite();
        player.transform.rotation *= Quaternion.Euler(0, -60, 0);
        var count = 3;
        while (count > 0)
        {
            typingVoice.sayCountDown();
            UICountDown.text = count.ToString();
            yield return new WaitForSeconds(1f);
            count--;
        }
        UICountDown.text = "";

        // 次の文章
        StartCoroutine(ChangeSentence());
        isTimerRunning = true;
        isInputValid = true;

        animator.SetBool("move", true);
    }

    /// <summary>
    /// 時間制限なしスタート演出
    /// </summary>
    private void NoTimerStart()
    {
        string randAtk = "atk" + (new System.Random().Next(1, 3)).ToString();

        UICountDown.text = "";

        // 次の文章
        StartCoroutine(ChangeSentence());
        isInputValid = true;
    }

    void Update()
    {
        if (isForceQuit)
        {
            if (!goNextScene)
            {
                GameManager.TypingDataPath = null;
                GameManager.SceneNo = (int)scene.House;   // ワールドシーンショップ
                if (GameManager.eventHeijo)
                {
                    GameManager.SceneNo = (int)scene.Heijo;   // ワールドシーン平城京
                    GameManager.eventHeijo = false;
                }
                SceneManager.LoadScene("WorldScene"); // ワールドシーンに遷移
                goNextScene = true;
            }
        }
        // タイマーが実行中の場合、時間を減少させる
        if (isTimerRunning)
        {
            // 進んでいく フレームに依存しないTime.deltaTime
            player.transform.position += new Vector3(necoSpeed * Time.deltaTime, 0, 0);
            player.transform.LookAt(targetCam.transform);   // カメラを向く
            player.transform.rotation *= Quaternion.Euler(0, -60, 0);

            currentTime -= Time.deltaTime;
            // タイマーが0以下になったら停止
            if (currentTime < 0)
            {
                currentTime = 0;
                isTimerRunning = false;
                isInputValid = false;
                Fukidashi.SetActive(false);

                checkSeekerTimer();
                dispResultTimerVer();

                door.SetActive(false);
            }
            UpdateTimerText();
        }

        if (spaceStart)
        {
            // アニメーションを切り替える
            if (Time.time % 20 > 19.9)
            {
                animator.SetTrigger("reset");
                System.Threading.Thread.Sleep(100);     // 連続実行防止
            }
        }
        if (spaceThrow)
        {
            END.text = "";
            UIH.text = "スペースキーでボーナスゲット！";
        }
        if (spaceEnd)
        {
            // アニメーションを切り替える
            if (Time.time % 18 > 17.9)
            {
                if (firstEnd)
                {
                    firstEnd = false;
                    System.Threading.Thread.Sleep(100);     // 連続実行防止
                }
                else
                {
                    END.text = "";
                    UIH.text = "スペースキーでしゅうりょう";
                    animator.SetTrigger("make");
                    System.Threading.Thread.Sleep(100);     // 連続実行防止
                }
            }
        }
    }

    private void getKeyComboBonus()
    {
        seekerBonus += seekerCombo/2 + seekerKey/2;
        coins.SpawnCoins(seekerCombo/2 + seekerKey/2, 0);    // コインアニメーション
        typingVoice.sayCoin3();
        updateSeeker();
    }

    private void dispResultNonTimerVer()
    {
        END.text = "よくできました！";
        UIJ.text = "";
        UIH.text = "";
        UIR.text = "";
        UII.text = "";

        getKeyComboBonus();

        GameManager.NewKpm = 0;                 // 今回のKPM
        GameManager.KeyParSecond = 2;           // 今回の１秒あたりのキー入力。常にOKとするため１より大を設定
        GameManager.AnswerRate = correctAR;     // 今回の正答率
        GameManager.TypingTitle = theme.title;  // 実施したテーマ
        GameManager.MaxCombo = maxCombo;        // 今回の最大コンボ数

        // キーカラークリア
        AssistKeyboardObj.SetAllKeyColorWhite();
        AssistKeyboardObj.SetNextHighlight(" ");

        // アニメーション
        animator.SetTrigger("end3");

        // 結果処理
        currentTime = 0;
        isTimerRunning = false;
        isInputValid = false;
        Fukidashi.SetActive(false);
        door.SetActive(false);
        spaceEnd = true;

        gm.setGemini();
    }

    private void dispResultTimerVer()
    {
        END.text = "おしまい！";
        UIJ.text = "";
        UIH.text = "";
        UIR.text = "";
        UII.text = "";

        getKeyComboBonus();

        kpm = correctN / totalTime * 60.0f;
        UIkpm.text = string.Format("{0:0}", kpm);

        GameManager.NewKpm = (theme.timer > 0) ? (int)kpm : 0;  // 今回のKPM
        GameManager.KeyParSecond = correctN / totalTime;        // 今回の１秒あたりのキー入力
        GameManager.AnswerRate = correctAR;     // 今回の正答率
        GameManager.TypingTitle = theme.title;  // 実施したテーマ
        GameManager.MaxCombo = maxCombo;        // 今回の最大コンボ数

        // キーカラークリア
        AssistKeyboardObj.SetAllKeyColorWhite();
        AssistKeyboardObj.SetNextHighlight(" ");

        // アニメーション
        string randEnd = "end" + (new System.Random().Next(1, 6)).ToString();
        animator.SetTrigger(randEnd);
        player.transform.LookAt(targetCam.transform);   // カメラを向く

        // 結果ウィンドウ表示
        resultTitle.text = GameManager.TypingTitle;
        resultKpm.text = UIkpm.text;
        resultCombo.text = maxCombo.ToString();
        resultWindow.SetActive(true);
        lHand.SetActive(false);
        rHand.SetActive(false);

        gm.setGemini();

        if (GameManager.guestMode)
        {
            int itemId = 241;
            int blankIndex = gm.savedata.getBlankInventoryIndex();
            gm.savedata.Inventory[blankIndex] = itemId;
            gm.savedata.Items[itemId] = true;

            if (GameManager.eventHeijo)
            {
                panelMessage.SetActive(true);
                gm.exportLocal();
            }
            else
            {
                panelMessage.SetActive(false);
            }
        }

        gm.savedata.Settings[se.GachaCnt] ++;
        hopDiamond(gm.savedata.Settings[se.GachaCnt]);
    }

    private void setDiaDisp(int no)
    {
        dia1.SetActive(false);
        dia2.SetActive(false);
        dia3.SetActive(false);
        dia4.SetActive(false);
        dia5.SetActive(false);
        if (no >= 1) {
            dia1.SetActive(true);
        }
        if (no >= 2) {
            dia2.SetActive(true);
        }
        if (no >= 3) {
            dia3.SetActive(true);
        }
        if (no >= 4) {
            dia4.SetActive(true);
        }
    }
    private void hopDiamond(int no)
    {
        switch(no)
        {
            case 1:
                dia1.SetActive(true);
                diaAnim1.SetTrigger("Jump");
                spaceEnd = true;
                break;
            case 2:
                dia2.SetActive(true);
                diaAnim2.SetTrigger("Jump");
                spaceEnd = true;
                break;
            case 3:
                dia3.SetActive(true);
                diaAnim3.SetTrigger("Jump");
                spaceEnd = true;
                break;
            case 4:
                dia4.SetActive(true);
                diaAnim4.SetTrigger("Jump");
                spaceEnd = true;
                break;
            case 5:
                dia5.SetActive(true);
                diaAnim5.SetTrigger("Jump");
                spaceThrow = true;
                break;
        }
    }

    IEnumerator throwDise(int rank)
    {
        dice.SetActive(true);

        int rand = new System.Random().Next(1, 101);
        int diceNo = getDiceNo(rank, rand);
        
        switch(rank)
        {
            case 0:
                diceAnim.SetTrigger("rank1");
                break;
            case 1:
                diceAnim.SetTrigger("rank2");
                break;
            case 2:
                diceAnim.SetTrigger("rank3");
                break;
            case 3:
                diceAnim.SetTrigger("rank4");
                break;
            case 4:
                diceAnim.SetTrigger("rank5");
                break;
        }
        switch(diceNo)
        {
            case 1:
                diceAnim.SetTrigger("dice1");
                break;
            case 2:
                diceAnim.SetTrigger("dice2");
                break;
            case 3:
                diceAnim.SetTrigger("dice3");
                break;
            case 4:
                diceAnim.SetTrigger("dice4");
                break;
            case 5:
                diceAnim.SetTrigger("dice5");
                break;
            case 6:
                diceAnim.SetTrigger("dice6");
                break;
        }

        typingVoice.sayDice();
        yield return new WaitUntil(() => diceAnim.GetCurrentAnimatorStateInfo(0).IsName(diceStates[diceNo-1]) &&
                                            diceAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

        
        if (diceNo == 6) {
            seekerBonus += 36;
            typingVoice.sayCoin3();
            coins.SpawnCoins(20, 0);
            coins.SpawnCoins(8, 1);
            coins.SpawnCoins(8, 2);
        } else if (diceNo == 5) {
            seekerBonus += 20;
            typingVoice.sayCoin3();
            coins.SpawnCoins(14, 0);
            coins.SpawnCoins(3, 1);
            coins.SpawnCoins(3, 2);
        } else {
            seekerBonus += (int)(diceNo * 2)+6;
            coins.SpawnCoins((int)(diceNo * 2)+6, 0);
        }
        typingVoice.sayCoin3();
        updateSeeker();
        gm.savedata.Settings[se.GachaCnt] = 1;
        spaceEnd = true;
    }

    private int getDiceNo(int rank, int rand)
    {
        if (rand <= diceProbabilities[rank, 0]) {
            return 1;
        }
        else if (rand <= diceProbabilities[rank, 1]) {
            return 2;
        }
        else if (rand <= diceProbabilities[rank, 2]) {
            return 3;
        }
        else if (rand <= diceProbabilities[rank, 3]) {
            return 4;
        }
        else if (rand <= diceProbabilities[rank, 4]) {
            return 5;
        }
        else if (rand <= diceProbabilities[rank, 5]) {
            return 6;
        }
        return 3;
    }
    
    private void OnGUI()
    {
        Event e = Event.current;
        var isPushedShiftKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        var inputStr = ConvertKeyCodeToStr(e.keyCode, isPushedShiftKey);
        if (e.type == EventType.KeyDown && inputStr.Equals(" "))
        {
            if (spaceStart)     // スペースでスタート状態のとき
            {
                AssistKeyboardObj.pushKeyAction(inputStr);
                // スペースでスタート状態を解除する
                spaceStart = false;
                UIH.text = "";
                StartCoroutine(CountDown());    // カウントダウンからのスタート
                return;
            }
            else if (spaceThrow)
            {
                spaceThrow = false;
                if (GameManager.guestMode)
                {
                    return;     // イベント用に固まらせる
                }
                diaRank = new System.Random().Next(0, 5);

                StartCoroutine(PlayAnimationsInSequence());
            }
            else if (spaceEnd)    // スペースで終了状態のとき
            {
                AssistKeyboardObj.pushKeyAction(inputStr);
                if (!goNextScene)
                {
                    gm.savedata.Status[st.Gold] = totalSeeker;      // 所持シーカー
                    GameManager.SceneNo = (int)scene.House;         // ワールドシーンショップ前
                    if (GameManager.guestMode)
                    {
                        return;     // イベント用に固まらせる

                        GameManager.SceneNo = (int)scene.Heijo;   // ワールドシーン平城京
                        GameManager.eventHeijo = false;
                    }
                    SceneManager.LoadScene("WorldScene");           // ワールドシーンに遷移
                    goNextScene = true;
                }
            }
        }

        if (isInputValid && e.type == EventType.KeyDown && e.keyCode != KeyCode.None
        && !Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !Input.GetMouseButton(2))
        {
            var inputMouse = ConvertKeyCodeToStr(e.keyCode, isPushedShiftKey);
            AssistKeyboardObj.pushKeyAction(inputMouse);

            double currentTime = Time.realtimeSinceStartup;
            // タイピングで使用する文字以外は受け付けない
            // Esc など画面遷移などで使うキーと競合を避ける
            if (!inputMouse.Equals(""))
            {
                // 正誤チェック
                StartCoroutine(TypingCheck(inputMouse));
            }
        }
    }
    // アニメーション完了を待つコルーチン
    IEnumerator PlayAnimationsInSequence()
    {
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];

            // アニメーションステートを設定
            animator.SetTrigger("Next");

            typingVoice.sayDia(i);

            // アニメーションの完了を待つ
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Jump") &&
                                             animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        }

        for (int i = 0; i <= diaRank; i++)
        {
            Animator animator = animators[i];
            string state = states[i];

            typingVoice.sayDia(i+5);

            // アニメーションステートを設定
            animator.SetTrigger("Jump");

            // アニメーションの完了を待つ
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(state) &&
                                             animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        }
        
        StartCoroutine(throwDise(diaRank));
    }

    public void testThrow()
    {
        dia1.SetActive(true);
        dia2.SetActive(true);
        dia3.SetActive(true);
        dia4.SetActive(true);
        dia5.SetActive(true);
        diaRank = new System.Random().Next(0, 5);
        StartCoroutine(PlayAnimationsInSequence());
    }
    public void testThrowOnly()
    {
        StartCoroutine(throwDise(3));
    }
    /// <summary>
    /// キーコードから string
    /// <param name="key">keycode</param>
    /// <param name="isShiftkeyPushed">シフトキーが押されたかどうか</param>
    /// </summary>
    private string ConvertKeyCodeToStr(KeyCode key, bool isShiftkeyPushed)
    {
        if (key != 0) {     // デバッグ用
            Debug.Log("key: " + key);
            END.text = "key: " + key;
        }
        int keyType = gm.savedata.Settings[se.Capital];
        switch (key)
        {
            // かな入力用に便宜的にタブ文字を Shift+0 に割り当てている
            case KeyCode.Alpha0:
                return isShiftkeyPushed ? "\t" : "0";
            case KeyCode.Alpha1:
                return isShiftkeyPushed ? "!" : "1";
            case KeyCode.Alpha2:
                return isShiftkeyPushed ? "\"" : "2";
            case KeyCode.Alpha3:
                return isShiftkeyPushed ? "#" : "3";
            case KeyCode.Alpha4:
                return isShiftkeyPushed ? "$" : "4";
            case KeyCode.Alpha5:
                return isShiftkeyPushed ? "%" : "5";
            case KeyCode.Alpha6:
                return isShiftkeyPushed ? "&" : "6";
            case KeyCode.Alpha7:
                return isShiftkeyPushed ? "\'" : "7";
            case KeyCode.Alpha8:
                return isShiftkeyPushed ? "(" : "8";
            case KeyCode.Alpha9:
                return isShiftkeyPushed ? ")" : "9";
            case KeyCode.A:
                return isShiftkeyPushed ? "A" : "a";
            case KeyCode.B:
                return isShiftkeyPushed ? "B" : "b";
            case KeyCode.C:
                return isShiftkeyPushed ? "C" : "c";
            case KeyCode.D:
                return isShiftkeyPushed ? "D" : "d";
            case KeyCode.E:
                return isShiftkeyPushed ? "E" : "e";
            case KeyCode.F:
                return isShiftkeyPushed ? "F" : "f";
            case KeyCode.G:
                return isShiftkeyPushed ? "G" : "g";
            case KeyCode.H:
                return isShiftkeyPushed ? "H" : "h";
            case KeyCode.I:
                return isShiftkeyPushed ? "I" : "i";
            case KeyCode.J:
                return isShiftkeyPushed ? "J" : "j";
            case KeyCode.K:
                return isShiftkeyPushed ? "K" : "k";
            case KeyCode.L:
                return isShiftkeyPushed ? "L" : "l";
            case KeyCode.M:
                return isShiftkeyPushed ? "M" : "m";
            case KeyCode.N:
                return isShiftkeyPushed ? "N" : "n";
            case KeyCode.O:
                return isShiftkeyPushed ? "O" : "o";
            case KeyCode.P:
                return isShiftkeyPushed ? "P" : "p";
            case KeyCode.Q:
                return isShiftkeyPushed ? "Q" : "q";
            case KeyCode.R:
                return isShiftkeyPushed ? "R" : "r";
            case KeyCode.S:
                return isShiftkeyPushed ? "S" : "s";
            case KeyCode.T:
                return isShiftkeyPushed ? "T" : "t";
            case KeyCode.U:
                return isShiftkeyPushed ? "U" : "u";
            case KeyCode.V:
                return isShiftkeyPushed ? "V" : "v";
            case KeyCode.W:
                return isShiftkeyPushed ? "W" : "w";
            case KeyCode.X:
                return isShiftkeyPushed ? "X" : "x";
            case KeyCode.Y:
                return isShiftkeyPushed ? "Y" : "y";
            case KeyCode.Z:
                return isShiftkeyPushed ? "Z" : "z";
            case KeyCode.Minus:
                return isShiftkeyPushed ? "=" : "-";
            case KeyCode.Caret:
                return isShiftkeyPushed ? "~" : "^";
            case KeyCode.BackQuote:
                return isShiftkeyPushed ? "`" : "@";
            case KeyCode.LeftBracket:
                if (keyType == 1) {
                    return isShiftkeyPushed ? "`" : "@";
                } else if (keyType == 2) {
                    return isShiftkeyPushed ? "{" : "[";
                } else {
                    return isShiftkeyPushed ? "{" : "[";
                }
            case KeyCode.RightBracket:
                return isShiftkeyPushed ? "}" : "]";
            case KeyCode.Equals:
                return isShiftkeyPushed ? "+" : ";";
            case KeyCode.Semicolon:
                return isShiftkeyPushed ? "+" : ";";
            case KeyCode.Colon:
                return isShiftkeyPushed ? "*" : ":";
            case KeyCode.Comma:
                return isShiftkeyPushed ? "<" : ",";
            case KeyCode.Period:
                return isShiftkeyPushed ? ">" : ".";
            case KeyCode.Slash:
                return isShiftkeyPushed ? "?" : "/";
            case KeyCode.Underscore:
                return "_";
            case KeyCode.Space:
                return " ";
            case KeyCode.Backslash:
                return isShiftkeyPushed ? "|" : "Yen";
            default:
                return "";
        }
    }

    /// <summary>
    /// タイピングの正誤判定部分
    /// </summary>
    private IEnumerator TypingCheck(string nextString)
    {
        // まだ可能性のあるセンテンス全てに対してミスタイプかチェックする
        bool isMistype = JudgeTyping(nextString);
        if (!isMistype)
        {
            Correct(nextString);
        }
        else
        {
            necoSpeed = 0.0f;
            Mistype();
        }
        correctAR = (float)correctN / ((float)correctN + (float)mistakeN);
        if (((float)correctN + (float)mistakeN) != 0)
        {
            UIcorrectAR.text = string.Format("{0:0.0} %", correctAR*100);
        }

        if (comboN == 0)   // コンボ依存のアニメーション
        {
            UIcombo.rectTransform.localScale = new Vector3(0.8f, 0.8f, 0.8f);

            // UIcomboの位置に基づいて、UIkonboの新しい位置を計算
            float newXPosition = UIcombo.rectTransform.localPosition.x + UIcombo.rectTransform.rect.width * 0.8f * 0.7f;
            UIkonbo.rectTransform.localPosition = new Vector3(newXPosition, UIkonbo.rectTransform.localPosition.y, 0);
            
            animator.SetBool("run", false);
            animator.SetBool("move", false);
            animator.SetBool("walk", true);

            float run = animator.GetFloat("runSpeed");
            if (run > 2)
            {
                animator.SetTrigger("die2");
            }
            else if (run > 1)
            {
                animator.SetTrigger("die1");
            }
            else
            {
                animator.SetTrigger("damage");
            }
        }
        else if (comboN > 20)    // コンボ依存のアニメーション(run)
        {
            necoSpeed = 1.2f;
            animator.SetBool("run", true);
            animator.SetBool("move", false);
            animator.SetBool("walk", false);
            if (comboN > 30)
            {
                necoSpeed = 1.4f;
                animator.SetFloat("runSpeed", 1 + (float)((comboN - 30) / 35));
            }
        }
        else if (comboN > 8)    // コンボ依存のアニメーション（move)
        {
            necoSpeed = 0.6f;
            animator.SetBool("run", false);
            animator.SetBool("move", true);
            animator.SetBool("walk", false);
            animator.SetFloat("walkSpeed", 1.0f);

            if (comboN > 10)
            {
                necoSpeed = 0.9f;
                animator.SetFloat("moveSpeed", 1 + (float)((comboN - 10) / 6));
            }
            animator.ResetTrigger("die1");
            animator.ResetTrigger("die2");
        }
        else if (comboN > 5)    // トリガー解除
        {
            animator.ResetTrigger("damage");
            if (comboN > 3)
            {
                necoSpeed = 0.4f;
                animator.SetFloat("walkSpeed", 1 + (float)((comboN - 3) / 3));
            }
            else
            {
                necoSpeed = 0.2f;
                animator.SetFloat("walkSpeed", 1.0f);
            }
            animator.SetFloat("runSpeed", 1.0f);
            animator.SetFloat("moveSpeed", 1.0f);
        }
        yield return null;
    }

    /// <summary>
    /// ミスタイプ判定と次打つべき文字のインデックス更新
    /// </summary>
    /// <param name="currentStr">打った文字</param>
    /// <returns>ミスタイプなら true</returns>
    private bool JudgeTyping(string currentStr)
    {
        bool isMistype = true;
        for (int i = 0; i < typingJudge[index].Count; ++i)
        {
            // すでに打った文字から判定候補でないとわかるときはパス
            if (sentenceValid[index][i] == 0) { continue; }
            int j = sentenceIndex[index][i];
            string judgeString = typingJudge[index][i][j].ToString();
            if (currentStr.Equals(judgeString))
            {
                isMistype = false;
                indexAdd[index][i] = 1;
            }
            else { indexAdd[index][i] = 0; }
        }
        return isMistype;
    }

    /// <summary>
    /// タイピング正解時の処理
    /// <param name="typeChar">打った文字</param>
    /// </summary>
    private void Correct(string typeChar)
    {
        correctN++;
        comboN++;

        isRecMistype = false;

        typingVoice.sayNya();
        if (maxCombo < comboN)
        {
            maxCombo = comboN;
        }
        UIcorrect.text = string.Format("{0:0}", correctN);
        UIcombo.text = string.Format("{0:0}", comboN);

        checkSeekerCombo();

        // 可能な入力パターンのチェック
        bool isIndexCountUp = IsJudgeIndexCountUp(typeChar);
        // ローマ字入力表示を更新
        UpdateSentence(typeChar);
        if (isIndexCountUp) { index++; }

        // 文章入力完了処理
        if (index >= typingJudge.Count) { CompleteTask(); }
    }

    /// <summary>
    /// 有効パターンをチェックし、インデックスを増やすかどうか判定する
    /// index はオートマトン上での index
    /// <param names="typeChar">打った文字</param>
    /// <returns>インデックス増やすなら true、さもなくば false</returns>
    /// </summary>
    private bool IsJudgeIndexCountUp(string typeChar)
    {
        bool ret = false;
        // 可能な入力パターンを残す
        for (int i = 0; i < typingJudge[index].Count; ++i)
        {
            // typeChar と一致しないものを無効化処理
            if (!typeChar.Equals(typingJudge[index][i][sentenceIndex[index][i]].ToString()))
            {
                sentenceValid[index][i] = 0;
            }
            // 次のキーへ
            sentenceIndex[index][i] += indexAdd[index][i];
            // 次の文字へ
            if (sentenceIndex[index][i] >= typingJudge[index][i].Length) { ret = true; }
        }
        return ret;
    }
    /// <summary>
    /// 1文打ち終わった後の処理
    /// </summary>
    private void CompleteTask()
    {
        // タイプした文字を緑色に
//        UIR.text = $"<color=#20A01D>{UIR.text}</color>";
        answers++;

        if ((currentThemeIndex >= shuffledThemes.Count) && (theme.random == 0))     // 時間制じゃない時の終わり
        {
            dispResultNonTimerVer();            // 終了処理
        }
        else
        {
            StartCoroutine(ChangeSentence());   // 次の文章
        }
    }

    /// <summary>
    /// 画面上に表示する打つ文字の表示を更新する
    /// <param name="typeChar">打った文字</param>
    /// </summary>
    private void UpdateSentence(string typeChar)
    {
        // 打った文字を消去するオプションの場合
        // 複数入力パターンが考えられるときは最初にマッチしたものを表示しなおす
        var nextTypingSentence = "";
        for (int i = 0; i < typingJudge.Count; ++i)
        {
            if (i < index) { continue; }
            for (int j = 0; j < typingJudge[i].Count; ++j)
            {
                if (index == i && sentenceValid[index][j] == 0) { continue; }
                else if (index == i && sentenceValid[index][j] == 1)
                {
                    for (int k = 0; k < typingJudge[index][j].Length; ++k)
                    {
                        if (k >= sentenceIndex[index][j])
                        {
                            nextTypingSentence += typingJudge[index][j][k].ToString();
                        }
                    }
                    break;
                }
                else if (index != i && sentenceValid[i][j] == 1)
                {
                    nextTypingSentence += typingJudge[i][j];
                    break;
                }
            }
        }
        correctString += typeChar;
        // Space は打ったか打ってないかわかりにくいので表示上はアンダーバーに変更
        var UIStr = "";
        if (ConfigScript.IsBeginnerMode || ConfigScript.IsShowTypeSentence)
        {
            UIStr = nextTypingSentence;
        }
        else
        {
            UIStr = correctString + (isSentenceMistyped ? ("<color=#ff8888ff>" + nextTypingSentence + "</color>") : "");
        }
        SetUITypeText(UIStr);
        CurrentTypingSentence = nextTypingSentence;


        if (CurrentTypingSentence == "" || !isInputValid)
        {
            AssistKeyboardObj.SetAllKeyColorWhite();
        }
        else if (isInputValid)
        {
            var nextHighlight = CurrentTypingSentence[0].ToString();
//            handAnimation(nextHighlight);
            AssistKeyboardObj.SetNextHighlight(nextHighlight);
        }
    }

    /// <summary>
    /// タイピング文の半角スペースをアンダーバーに置換して表示
    /// 打ったか打ってないかわかりにくいため、アンダーバーを表示することで改善
    /// </summary>
    private void SetUITypeText(string sentence)
    {
        if (theme.hide < 1)
        {
            if (gm.savedata.Settings[se.Capital] == 1)
            {
                UIR.text = sentence.ToUpper().Replace(' ', '_');
            }
            else
            {
                UIR.text = sentence.Replace(' ', '_');
            }
        }
    }

    /// <summary>
    /// タイピング正誤判定まわりの初期化
    /// </summary>
    private void InitSentenceData()
    {
        var sLength = typingJudge.Count;
        sentenceIndex.Clear();
        sentenceValid.Clear();
        indexAdd.Clear();
        sentenceIndex = new List<List<int>>();
        sentenceValid = new List<List<int>>();
        indexAdd = new List<List<int>>();
        for (int i = 0; i < sLength; ++i)
        {
            var typeNum = typingJudge[i].Count;
            sentenceIndex.Add(new List<int>());
            sentenceValid.Add(new List<int>());
            indexAdd.Add(new List<int>());
            for (int j = 0; j < typeNum; ++j)
            {
                sentenceIndex[i].Add(0);
                sentenceValid[i].Add(1);
                indexAdd[i].Add(0);
            }
        }
    }

    /// <summary>
    /// ミスタイプ時の処理
    /// </summary>
    private void Mistype()
    {
        if (0 < comboN)
        {
            mistakeN++;
        }
        // コンボ数リセット、表示更新
        comboN = 0;
        UIcombo.text = string.Format("{0:0}", comboN);
        UImistake.text = string.Format("{0:0}", mistakeN);

        // 打つべき文字を赤く表示
        if (!isRecMistype)
        {
            string UIStr = "";
            if (ConfigScript.IsBeginnerMode || ConfigScript.IsShowTypeSentence)
            {
                UIStr = "<color=#ff8888ff>" + CurrentTypingSentence.ToString() + "</color>";
            }
            else
            {
                UIStr = correctString + "<color=#ff0000ff>" + CurrentTypingSentence.ToString() + "</color>";
            }
            SetUITypeText(UIStr);
        }
        // color タグを多重で入れないようにする
        isRecMistype = true;
        // 最初のミスタイプ時にのみ保存
        if (!isSentenceMistyped)
        {
            GameManager.MistypedSentences.Add(nQH.ToString());
            isSentenceMistyped = true;
        }
    }
    public void onForceQuit()
    {
        isForceQuit = true;
    }

}