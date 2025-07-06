using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
using System.Linq;
using Newtonsoft.Json;


public class scene
{
    public const int Title = 0;
    public const int World = 1;
    public const int Typing = 2;
    public const int House = 3;
    public const int Night = 4;
    public const int Heijo = 5;
}

public class GameManager : MonoBehaviour
{
    public DataBase db;
    public SaveData savedata;
    public Connection connection;
    public static int openHour = 0;            // 開店時間
    public static int closeHour = 23;          // 閉店時間


    static public int SceneNo { get; set; }
    static public int TypingTab { get; set; }
    static public int NewKpm { get; set; }
    static public float KeyParSecond { get; set; }
    static public float AnswerRate { get; set; }
    static public int TypingDataId { get; set; }
    static public string TypingDataPath { get; set; }
    static public string TypingTitle { get; set; }
    static public int MaxCombo { get; set; }
    public static List<string> MistypedSentences { get; set; } = new List<string>();
    public static string geminiResponce { get; set; }
    public static bool eventHeijo { get; set; }
    public static bool guestMode { get; set; }

    [SerializeField] private float kpmRatio = 0.8f;
    [SerializeField] private Setting setting;

    public GameObject player;        // プレイヤーオブジェクト
    public ChibiCat chibiCat;        // 猫ボディ
    public ChibiCat chibiCat2D;      // 猫ボディ
    public GameObject cam;           // カメラ
    private Animator animator;       // Playerのアニメーター
    public GameObject inventory;
    public GameObject inventoryFilter;
    public GameObject equip;
    public GameObject ranking;
    public GameObject status;
    public GameObject typingRoom;
    public GameObject shopRoom;
    public NpcManager npcManager;
    public Ranking rankingWindow;

    public GameObject inventoryButton;  // インベントリボタン
    public GameObject rankingButton;    // ランキングボタン
    public GameObject settingButton;    // セッティングボタン
    public GameObject collectionMedalButton;
    public GameObject collectionItemButton;

    public Toggle exToggle;
    public Toggle gssToggle;
    public Toggle enetToggle;
    public Toggle gmailToggle;

    public StatusUI statusui;
    public bool dragging = false;

    [SerializeField]
    private int windowOpenCount = 20;    // ウィンドウが開くフレーム数
    private int count = 0;               // カウンタ
    private int inventoryOpen = 0;
    private int rankingOpen = 0;
    private int cameraMove = 0;          // 0:標準 1:右回転 2:左回転 3:ズームイン

    [SerializeField]
    private TMP_Text talk;

    Vector3 chaseOffset = new Vector3(0f, 8f, -14f);
    Quaternion chaseRotation = Quaternion.Euler(18.5f, 0f, 0f);
    Vector3 statusOffset = new Vector3(1.4f, 1.3f, -4f);
    Quaternion statusRotation = Quaternion.Euler(5f, 0f, 0f);

    private float difx, dify, difz, posx, posy, posz;

    RectTransform statusRectTransform;
    RectTransform inventoryRectTransform;
    RectTransform rankingRectTransform;
    RectTransform equipRectTransform;
    RectTransform inventoryFilterRectTransform;

    // 目標位置
    Vector2 statusShowPos;
    Vector2 inventoryShowPos;
    Vector2 rankingShowPos;
    Vector2 equipmentShowPos;
    Vector2 statusHidePos;
    Vector2 inventoryHidePos;
    Vector2 rankingHidePos;
    Vector2 equipmentHidePos;
    Vector2 inventoryFilterPos;

    private int[] oldInventory;
    private int[] newInventory;
    private int[] oldEquip;
    private int[] newEquip;
    private bool rankScroll = true;
    private void Awake()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

#if !UNITY_EDITOR
        // 現在の時刻を取得
        int currentHour = System.DateTime.Now.Hour;

        // 許可された時間範囲内かどうかをチェック
        if (currentHour < openHour || currentHour >= closeHour)
        {
            GameManager.SceneNo = scene.Night;
            if (sceneName != "TitleScene")
            {
                SceneManager.LoadScene("TitleScene"); // タイトルシーンに遷移
            }
            return;
        }
#endif

        if (sceneName == "TitleScene")
        {
            GameManager.SceneNo = (int)scene.Title;
        }
        else if (sceneName == "WorldScene")
        {
            if ((GameManager.SceneNo != (int)scene.House) && (GameManager.SceneNo != (int)scene.Heijo))
            {
                GameManager.SceneNo = (int)scene.World;
            }
        }
        else if (sceneName == "TypingStage")
        {
            GameManager.SceneNo = (int)scene.Typing;
        }
        
        animator = player.GetComponent<Animator>(); // Playerのアニメーターを取得

        oldInventory = new int[64];
        newInventory = new int[64];
        oldEquip = new int[10];
        newEquip = new int[10];


        // アニメーションステートがタイトルの場合
        if (SceneNo == (int)scene.Title)
        {
        }
        else if (SceneNo == (int)scene.Typing)
        {
        }
        else {
            statusRectTransform = status.GetComponent<RectTransform>();
            inventoryRectTransform = inventory.GetComponent<RectTransform>();
            rankingRectTransform = ranking.GetComponent<RectTransform>();
            equipRectTransform = equip.GetComponent<RectTransform>();
            inventoryFilterRectTransform = inventoryFilter.GetComponent<RectTransform>();
            // 目標位置
            statusShowPos = new Vector2(-statusRectTransform.sizeDelta.x / 2 - 10, -statusRectTransform.sizeDelta.y / 2 - 20);
            statusHidePos = new Vector2(-statusRectTransform.sizeDelta.x / 2 - 20, statusRectTransform.sizeDelta.y / 2);
            inventoryShowPos = new Vector2(-inventoryRectTransform.sizeDelta.x / 2 - 10, -statusRectTransform.sizeDelta.y - inventoryRectTransform.sizeDelta.y / 2 - 20);
            inventoryHidePos = new Vector2(inventoryRectTransform.sizeDelta.x / 2, -statusRectTransform.sizeDelta.y - inventoryRectTransform.sizeDelta.y / 2 - 20);
            rankingShowPos = new Vector2(-rankingRectTransform.sizeDelta.x / 2 - 10, -statusRectTransform.sizeDelta.y - rankingRectTransform.sizeDelta.y / 2 - 20);
            rankingHidePos = new Vector2(rankingRectTransform.sizeDelta.x / 2, -statusRectTransform.sizeDelta.y - rankingRectTransform.sizeDelta.y / 2 - 20);
            equipmentShowPos = new Vector2(0, equipRectTransform.sizeDelta.y / 2 + 30);
            equipmentHidePos = new Vector2(0, -equipRectTransform.sizeDelta.y / 2 - 50);
            inventoryFilterPos = new Vector2(-inventoryRectTransform.sizeDelta.x / 2 - 40, -statusRectTransform.sizeDelta.y - inventoryRectTransform.sizeDelta.y / 2 - 20);
            inventoryFilterRectTransform.anchoredPosition = inventoryFilterPos;

            // アニメーションステートが1最初のワールドの場合
            if (SceneNo == (int)scene.World)
            {
                statusui.dispStatus();
                status.SetActive(false);
                inventory.SetActive(false);
                equip.SetActive(false);
                ranking.SetActive(false);
                typingRoom.SetActive(false);
                shopRoom.SetActive(false);
//                savedata.Settings[se.GachaCnt] = 1;
            }
            // アニメーションステートが3タイピング後の場合
            else if (SceneNo == (int)scene.House)
            {
                inventory.SetActive(false);
                equip.SetActive(false);
                settingButton.SetActive(false);
                rankingButton.SetActive(false);
                inventoryButton.SetActive(false);
                collectionMedalButton.SetActive(false);
                collectionItemButton.SetActive(false);
                status.SetActive(true);
                ranking.SetActive(true);

                statusRectTransform.anchoredPosition = statusShowPos;
                rankingRectTransform.anchoredPosition = rankingShowPos;
                typingRoom.SetActive(true);
                shopRoom.SetActive(false);

                if (geminiResponce != null)
                {
                    talk.text = geminiResponce;
                }
                else{
                    talk.text = "よくがんばりましたね";
                }
                geminiResponce = null;
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (SceneNo != (int)scene.Title)
        {
        }

        difx = (statusRotation.eulerAngles.x - chaseRotation.eulerAngles.x) / windowOpenCount;
        dify = (statusRotation.eulerAngles.y - chaseRotation.eulerAngles.y) / windowOpenCount;
        difz = (statusRotation.eulerAngles.z - chaseRotation.eulerAngles.z) / windowOpenCount;
        posx = (statusOffset.x - chaseOffset.x) / windowOpenCount;
        posy = (statusOffset.y - chaseOffset.y) / windowOpenCount;
        posz = (statusOffset.z - chaseOffset.z) / windowOpenCount;

        // シーンがタイトルの場合
        if (SceneNo == (int)scene.Title)
        {
        }
        // シーンが1最初のワールドの場合
        else if (SceneNo == (int)scene.World)
        {
            npcManager.SpawnNPCs();
            if (savedata.Equipment[eq.CatBody] != 0)
            {
                chibiCat.setChara(savedata.Equipment[eq.CatBody]);
                chibiCat2D.setChara(savedata.Equipment[eq.CatBody]);
            }
            chibiCat.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
            chibiCat.changeEquipHead(savedata.Equipment[eq.Head]);
            chibiCat.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
            chibiCat2D.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
            chibiCat2D.changeEquipHead(savedata.Equipment[eq.Head]);
            chibiCat2D.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
        }
        // シーンが3タイピング後の場合
        else if (SceneNo == (int)scene.House)
        {
            setting.initVolume();
            setting.sayOutDoor();
            rankingWindow.DisplayRankings();    // ランキング更新してから・・・プレイヤーのタイピング更新してから・・・保存したい
            npcManager.SpawnNPCs();
            if (savedata.Equipment[(int)eq.CatBody] != 0)
            {
                chibiCat.setChara(savedata.Equipment[eq.CatBody]);
                chibiCat2D.setChara(savedata.Equipment[eq.CatBody]);
            }
            chibiCat.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
            chibiCat.changeEquipHead(savedata.Equipment[eq.Head]);
            chibiCat.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
            chibiCat2D.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
            chibiCat2D.changeEquipHead(savedata.Equipment[eq.Head]);
            chibiCat2D.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
            if (NewKpm != 0)
            {
                rankingWindow.SetTo(savedata.Status[st.Rank]);
                rankingWindow.ScrollTo(savedata.Status[st.Rank]);
            }
            MistypedSentences.Clear();  // リストから全ての要素を削除
        }
        // シーンが2タイピングの場合
        else if (SceneNo == (int)scene.Typing)
        {
            geminiResponce = null;
            if (savedata.Equipment[(int)eq.CatBody] != 0)
            {
                chibiCat.setChara(savedata.Equipment[eq.CatBody]);
            }
            chibiCat.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
            chibiCat.changeEquipHead(savedata.Equipment[eq.Head]);
            chibiCat.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
        }
        // シーンが5平城後の場合
        else if (SceneNo == (int)scene.Heijo)
        {
            setting.initVolume();
            setting.sayOutDoor();
            rankingWindow.DisplayRankings();    // ランキング更新してから・・・プレイヤーのタイピング更新してから・・・保存したい
            npcManager.SpawnNPCs();
            if (savedata.Equipment[(int)eq.CatBody] != 0)
            {
                chibiCat.setChara(savedata.Equipment[eq.CatBody]);
                chibiCat2D.setChara(savedata.Equipment[eq.CatBody]);
            }
            chibiCat.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
            chibiCat.changeEquipHead(savedata.Equipment[eq.Head]);
            chibiCat.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
            chibiCat2D.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
            chibiCat2D.changeEquipHead(savedata.Equipment[eq.Head]);
            chibiCat2D.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
            if (NewKpm != 0)
            {
                rankingWindow.SetTo(savedata.Status[st.Rank]);
                rankingWindow.ScrollTo(savedata.Status[st.Rank]);
            }
            MistypedSentences.Clear();  // リストから全ての要素を削除
        }
    }

    // Update is called once per frame
    void Update()
    {
        // アニメーションステートが1最初のワールドの場合
        if (SceneNo == (int)scene.World)
        {
            // インベントリボタンまたはIキーが押され、カウンタが0の場合
         //   if ((Input.GetKeyDown(KeyCode.I) || inventoryButton.isOpen()) && (count == 0))
            if (inventoryButton.GetComponent<OpenButton>().isOpen() && (count == 0))
            {
                count = windowOpenCount;
                inventoryButton.GetComponent<OpenButton>().resetOpen();

                if (inventory.activeSelf)   // インベントリ表示中なら
                {
                    setting.sayWindowClose();
                    inventoryOpen = -1;         // インベントリ引っ込める
                    checkInventory();
                    rankingOpen = 0;            // ランキングなんもなし
                    cameraMove = 2;             // カメラは引き
                }
                else if (ranking.activeSelf)  // ランキング表示中なら
                {
                    setting.sayWindowOpen();
                    rankingOpen = -1;           // ランキング引っ込める
                    inventoryOpen = 1;          // インベントリでてくる
                    keepInventory();
                    cameraMove = 1;             // カメラ動作なし
                }
                else                        // ワールド通常表示中なら
                {
                    setting.sayWindowOpen();
                    inventoryOpen = 1;          // インベントリでてくる
                    keepInventory();
                    rankingOpen = 0;            // ランキングなんもなし
                    cameraMove = 3;             // カメラは寄り
                }
            }
            // ランキングボタンまたはRキーが押され、カウンタが0の場合
            // else if ((Input.GetKeyDown(KeyCode.R) || rankingButton.isOpen()) && (count == 0))
            else if (rankingButton.GetComponent<OpenButton>().isOpen() && (count == 0))
            {
                count = windowOpenCount;
                rankingButton.GetComponent<OpenButton>().resetOpen();

                if (ranking.activeSelf)         // ランキング表示中なら
                {
                    setting.sayWindowClose();
                    inventoryOpen = 0;              // インベントリなんもなし
                    rankingOpen = -1;               // ランキング引っ込める
                    cameraMove = 2;                 // カメラは引き
                }
                else if (inventory.activeSelf)   // インベントリ表示中なら
                {
                    setting.sayWindowOpen();
                    inventoryOpen = -1;             // インベントリ引っ込める
                    checkInventory();
                    rankingOpen = 1;                // ランキングでてくる
                    cameraMove = 1;                 // カメラ動作なし
                }
                else                            // ワールド通常表示中なら
                {
                    setting.sayWindowOpen();
                    inventoryOpen = 0;              // インベントリなんもなし
                    rankingOpen = 1;                // ランキングでてくる
                    cameraMove = 3;                 // カメラは寄り
                }
            }

            if (count > 0)
            {
                if (count > windowOpenCount / 2)    // ウィンドウひっこむ
                {
                    if (inventoryOpen == -1)
                    {
                        // オブジェクトの位置を更新する
                        statusRectTransform.anchoredPosition = Vector2.MoveTowards(statusRectTransform.anchoredPosition, statusHidePos, Time.deltaTime * 20000 / windowOpenCount);
                        inventoryRectTransform.anchoredPosition = Vector2.MoveTowards(inventoryRectTransform.anchoredPosition, inventoryHidePos, Time.deltaTime * 70000 / windowOpenCount);
                        equipRectTransform.anchoredPosition = Vector2.MoveTowards(equipRectTransform.anchoredPosition, equipmentHidePos, Time.deltaTime * 30000 / windowOpenCount);
                    }
                    if (rankingOpen == -1)
                    {
                        statusRectTransform.anchoredPosition = Vector2.MoveTowards(statusRectTransform.anchoredPosition, statusHidePos, Time.deltaTime * 20000 / windowOpenCount);
                        rankingRectTransform.anchoredPosition = Vector2.MoveTowards(rankingRectTransform.anchoredPosition, rankingHidePos, Time.deltaTime * 70000 / windowOpenCount);
                    }
                    count--;
                }
                else if (count == windowOpenCount / 2)
                {
                    status.SetActive(false);
                    inventory.SetActive(false);
                    equip.SetActive(false);
                    ranking.SetActive(false);
                    if (inventoryOpen == -1)
                    {
                        // オブジェクトの位置を確定させる
                        statusRectTransform.anchoredPosition = statusHidePos;
                        inventoryRectTransform.anchoredPosition = inventoryHidePos;
                        equipRectTransform.anchoredPosition = equipmentHidePos;
                    }
                    if (rankingOpen == -1)
                    {
                        statusRectTransform.anchoredPosition = statusHidePos;
                        rankingRectTransform.anchoredPosition = rankingHidePos;
                    }
                    count--;
                }
                else if (count == 1)
                {
                    if (inventoryOpen == 1)
                    {
                        // オブジェクトの位置を確定させる
                        statusRectTransform.anchoredPosition = statusShowPos;
                        inventoryRectTransform.anchoredPosition = inventoryShowPos;
                        if (!shopRoom.activeSelf) {
                            equipRectTransform.anchoredPosition = equipmentShowPos;
                        } else {
                            equip.SetActive(true);
                            equipRectTransform.anchoredPosition = equipmentHidePos;
                        }
                    }
                    if (rankingOpen == 1)
                    {
                        statusRectTransform.anchoredPosition = statusShowPos;
                        rankingRectTransform.anchoredPosition = rankingShowPos;
                        if (rankScroll)
                        {
                            rankingWindow.ScrollTo(savedata.Status[st.Rank]);
                            rankScroll = false;
                        }
                    }
                    count--;
                }
                else   // ウィンドウでてくる
                {
                    if (inventoryOpen == 1)
                    {
                        status.SetActive(true);
                        inventory.SetActive(true);
                        // オブジェクトの位置を更新する
                        statusRectTransform.anchoredPosition = Vector2.MoveTowards(statusRectTransform.anchoredPosition, statusShowPos, Time.deltaTime * 20000 / windowOpenCount);
                        inventoryRectTransform.anchoredPosition = Vector2.MoveTowards(inventoryRectTransform.anchoredPosition, inventoryShowPos, Time.deltaTime * 70000 / windowOpenCount);
                            if (!shopRoom.activeSelf) {
                            equip.SetActive(true);
                            equipRectTransform.anchoredPosition = Vector2.MoveTowards(equipRectTransform.anchoredPosition, equipmentShowPos, Time.deltaTime * 30000 / windowOpenCount);
                        }
                    }
                    if (rankingOpen == 1)
                    {
                        status.SetActive(true);
                        ranking.SetActive(true);
                        statusRectTransform.anchoredPosition = Vector2.MoveTowards(statusRectTransform.anchoredPosition, statusShowPos, Time.deltaTime * 20000 / windowOpenCount);
                        rankingRectTransform.anchoredPosition = Vector2.MoveTowards(rankingRectTransform.anchoredPosition, rankingShowPos, Time.deltaTime * 70000 / windowOpenCount);
                    }
                    count--;
                }
                if (cameraMove == 3)            // カメラより
                {
                    cam.transform.rotation = Quaternion.Euler(statusRotation.eulerAngles.x - difx * count, statusRotation.eulerAngles.y - dify * count, statusRotation.eulerAngles.z - difz * count);
                    cam.transform.position = player.transform.position + new Vector3(statusOffset.x - posx * count, statusOffset.y - posy * count, statusOffset.z - posz * count);
                }
                else if (cameraMove == 2)       // カメラひき
                {
                    cam.transform.rotation = Quaternion.Euler(chaseRotation.eulerAngles.x + difx * count, chaseRotation.eulerAngles.y + dify * count, chaseRotation.eulerAngles.z + difz * count);
                    cam.transform.position = player.transform.position + new Vector3(chaseOffset.x + posx * count, chaseOffset.y + posy * count, chaseOffset.z + posz * count);
                }
                if ((count == 0) && (cameraMove == 2))
                {
                    cameraMove = 0;
                }
            }
        }
        // アニメーションステートが0タイトルシーンの場合
        if (SceneNo == (int)scene.Title)
        {
            // 1秒ごとにアニメーションを切り替える
            if (Time.time % 60 > 50)
            {
                animator.SetBool("Swim", true);
            }
            else
            {
                animator.SetBool("Swim", false);
            }
        }
    }

    private void keepInventory()
    {
        oldInventory = (int[])savedata.Inventory.Clone();    // インベントリを開いた時の並びを保存しておく
        oldEquip = (int[])savedata.Equipment.Clone();        // インベントリを開いた時の装備を保存しておく
    }

    private void checkInventory()
    {
        InventryUI inventoryUi = inventory.GetComponentInChildren<InventryUI>();
        SoubiUI equipUi = equip.GetComponentInChildren<SoubiUI>();
        inventoryUi.getAllItems();
        equipUi.getAllSoubi();
        newInventory = savedata.Inventory;     // 現在のインベントリの並びを保存
        newEquip = savedata.Equipment;         // 現在の装備を保存

        if (oldInventory == null || oldEquip == null)    // 何かの手違いで変更前がnullの場合抜ける
        {
            return;
        }
        int[] oldItems = oldEquip.Concat(oldInventory).ToArray();
        int[] newItems = newEquip.Concat(newInventory).ToArray();

        bool noChangeFlg = true;
        for (int i = 0; i < oldItems.Length; i++)
        {
            if (oldItems[i] != newItems[i])     // インベントリを開いたときと変化があるかチェック
            {
                noChangeFlg = false;
                break;
            }
        }
        if (!noChangeFlg)
        {
            exportLocal();  // インベントリ変更後のデータ保存ローカル＆GSS
        }
        oldInventory = null;        // データクリア
        oldEquip = null;
    }

    public void recalculateKpm()
    {
        if ((savedata.Kpms[0] == 0) || (savedata.Status[st.Kpm] * kpmRatio < NewKpm))    // 今回の成績が一定の成績以上であれば
        {
            savedata.updateKpm(NewKpm);   // kpm更新
        }
    }
    public void registerRecentTypingResult()
    {
        int stars = judgeStar();
        if (savedata.Medals[TypingDataId] < stars)
        {
            savedata.Medals[TypingDataId] = stars;

/*            int changeMedalId = savedata.EncodeToLongArray();
            if (changeMedalId != -1)
            {
                savedata.saveGssMedals(changeMedalId);
            }
*/
        }
    }

    private int judgeStar()
    {
        if ((AnswerRate > 0.95) && (KeyParSecond > 1.0))
        {
            return 4;       // 星3つ
        }
        else if ((AnswerRate > 0.75) && (KeyParSecond > 0.3))
        {
            return 3;       // 星2つ
        }
        else if (AnswerRate > 0.4)
        {
            return 2;       // 星1つ
        }
        else
        {
            return 1;       // 星0こ
        }
    }

    public int getCameraMove()
    {
        return cameraMove;
    }
    public bool getWindowOpen()
    {
        if (inventoryOpen == 1 || rankingOpen == 1)
        {
            return true;
        }
        return false;
    }

    public static void SetTypingDataLevel(int no)
    {
        TypingDataId += no;
        TypingDataPath = "TextPrompts/" + TypingDataId.ToString();
    }

    public void changeEquip(int parts)
    {
        switch (parts)
        {
            case 0:     // 両手
                chibiCat.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
                chibiCat2D.changeEquipHands(savedata.Equipment[eq.RightHand], savedata.Equipment[eq.LeftHand], checkBagItem());
                break;

            case 1:     // 頭
                chibiCat.changeEquipHead(savedata.Equipment[eq.Head]);
                chibiCat2D.changeEquipHead(savedata.Equipment[eq.Head]);
                break;

            case 2:     // メガネ
                chibiCat.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
                chibiCat2D.changeEquipGlasses(savedata.Equipment[eq.Glasses]);
                break;
        }
    }

    public int checkBagItem()
    {
        int ret = 0;
        if (savedata.existInventory(6))
        {
            ret += 0x01;
        }
        if (savedata.existInventory(1))
        {
            ret += 0x02;
        }
        if (savedata.existInventory(2))
        {
            ret += 0x04;
        }
        if (savedata.existInventory(3))
        {
            ret += 0x08;
        }
        return ret;
    }

    public void exportLocal()
    {
        string saveLocalJson = savedata.CompileGameDataForLocal(savedata);
        connection.saveLocal(saveLocalJson);
        exportGas();        // 毎回GASアクセス要求。index.htmlで２４ｈに１回に制限される。
    }

    public void exportGas()
    {
        string saveGasObject = savedata.CompileGameDataForGss(savedata);
        connection.saveGas(saveGasObject);
    }

    public void importGas()
    {
        connection.loadGas();
    }

    public void firstExtention()
    {
        connection.enetLogin();
    }

    public void setGemini()
    {
        recalculateKpm();
        getRanking();
        string promptData = savedata.CompileGeminiData(savedata, db.GetServerList()[savedata.Status[st.Server]]);
        #if UNITY_WEBGL && !UNITY_EDITOR
            connection.throughGemini(promptData);
        #endif
    }

    private void getRanking()
    {
        int myKpm = savedata.Status[st.Kpm];
        int ranking = 1;
        foreach (ExRank rank in savedata.ExRankings)
        {
            if(myKpm >= rank.Kpm)
            {
                savedata.Status[st.Rank] = ranking;
                return;
            }
            ranking++;
        }
    }

    public string getNickname(int nicknameNo)
    {
        string nickname;
        Item item = db.GetItemList()[nicknameNo];
        if (item != null)
        {
            nickname = item.MyItemName;
        }
        else
        {
            nickname = "さん";
        }
        return nickname;
    }

    public void finishDataLoadExtRanking(string rankingDataJson)
    {
        try
        {
            var rankingData = JsonConvert.DeserializeObject<SerializableRankingData>(rankingDataJson);
            if (rankingData != null && rankingData.rankingData != null)
            {
                savedata.setRankingFromLocal(JsonConvert.SerializeObject(rankingData));
                Debug.Log("ランキングデータをロードしました。");
            }
            else
            {
                Debug.Log("ランキングデータがnullまたは不完全");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("データの読み込み中に例外発生: " + ex);
        }
    }

    public void returnGemini(string response)
    {
        // フィルタリング前の文字列をログに出力
        Debug.Log("Before Filtering: " + response);

        // ここでフィルタリングを試みる
        response = new string(response.Where(c => (c >= 0x0020 && c <= 0x007E) || // 基本ラテン文字と記号 (スペース, 英数字, 記号)
                                                (c >= 0x3040 && c <= 0x309F) || // ひらがな
                                                (c >= 0x30A0 && c <= 0x30FF) || // カタカナ
                                                (c >= 0x4E00 && c <= 0x9FFF) || // CJK統合漢字
                                                (c >= 0xFF00 && c <= 0xFFEF)    // 全角の記号、数字、アルファベットなど
                                                ).ToArray());

        // フィルタリング後の文字列をログに出力
        Debug.Log("After Filtering: " + response);

        geminiResponce = response; // ジェミニレスポンスをWorldに戻ったとき用に保存
        Debug.Log("GameManager returnGemini: " + response);

        if (talk != null)
        {
            talk.text = response;
        }
    }
}
