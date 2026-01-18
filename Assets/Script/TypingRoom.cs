using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TypingRoom : MonoBehaviour
{

    [SerializeField]
    private TMP_Text talk;

    [SerializeField]
    private GameObject housePlayer;
    private Animator pAnimator;

    [SerializeField]
    private GameObject littleCat;
    private Animator lAnimator;

    [SerializeField]
    private GameObject trainingList;
    [SerializeField]
    private GameObject challengeList;
    [SerializeField]
    private GameObject customList;

    // ここで、ShopItemParentのRectTransformを参照する
    [SerializeField]
    private RectTransform listParent;
    [SerializeField]
    private Setting setting;

    [SerializeField]
    private GameObject coinPrefab; // ゆびモード用コインプレハブ
    [SerializeField]
    private Transform coinParent;  // コインを表示する親オブジェクト

    private bool goNextScene = false;    // 次のシーンに遷移するためのフラグ
    
    private GameManager gm;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        
        // 起動時にリセットチェックを行う
        if (gm != null) 
        {
            gm.CheckDailyReset();
        }

        challengeList.SetActive(false);
        customList.SetActive(false);
        trainingList.SetActive(false);
        panelReset(GameManager.TypingTab);
        pAnimator = housePlayer.GetComponent<Animator>(); // Playerのアニメーターを取得
        lAnimator = littleCat.GetComponent<Animator>(); // littleCatのアニメーターを取得
        lAnimator.SetTrigger("jump");
        
        UpdateYubiCoins();
    }

    private void UpdateYubiCoins()
    {
        if (coinParent == null || coinPrefab == null || gm == null) return;

        // LayoutGroupなどの自動レイアウトコンポーネントがついているとスクリプトによる座標変更が上書きされるため無効化する
        var layoutGroup = coinParent.GetComponent<UnityEngine.UI.LayoutGroup>();
        if (layoutGroup != null) layoutGroup.enabled = false;
        
        var contentSizeFitter = coinParent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (contentSizeFitter != null) contentSizeFitter.enabled = false;

        // 既存のコインを削除
        foreach (Transform child in coinParent)
        {
            Destroy(child.gameObject);
        }

        // コインを生成
        int yubiCount = gm.savedata.Settings[se.YubiCnt];
        for (int i = 0; i < yubiCount; i++)
        {
            GameObject coin = Instantiate(coinPrefab, coinParent, false);
            
            // 位置調整 (重ねないようにずらす)
            // プレハブにRectTransformがついていない(Transformのみ)場合に対応するため、transform.localPositionを直接操作する
            Vector3 pos = coin.transform.localPosition;
            pos.y += i * 36f;
            pos.z += i * 5f;
            coin.transform.localPosition = pos;

            // ButtonコンポーネントはGraphic(Image等)がないとクリック判定を持たないため、
            // SpriteRendererしかついていない場合は透明なImageを追加して当たり判定とする
            if (coin.GetComponent<UnityEngine.UI.Image>() == null)
            {
                var img = coin.AddComponent<UnityEngine.UI.Image>();
                img.color = new Color(0, 0, 0, 0); // 透明
                // サイズが0だとクリックできないため、適当なサイズを与える
                RectTransform rtImg = coin.GetComponent<RectTransform>();
                if (rtImg != null)
                {
                    rtImg.sizeDelta = new Vector2(1, 1); // サイズ
                }
            }

            Button btn = coin.GetComponent<Button>();
            if (btn == null)
            {
                btn = coin.AddComponent<Button>();
            }
            btn.onClick.AddListener(StartYubiMode);
        }
    }

    public void StartYubiMode()
    {
        // プレイ直前にもリセットチェック（放置対策）
        if (gm != null)
        {
            gm.CheckDailyReset();
            UpdateYubiCoins(); 
        }

        if (gm.savedata.Settings[se.YubiCnt] > 0)
        {
            gm.savedata.Settings[se.YubiCnt]--;
            gm.saveGameData();
            
            GameManager.isYubiMode = true;
            GameManager.TypingDataPath = "YubiModeData"; // ゆびモード用のお題ファイルを指定
            gotoTypingState();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void gotoTypingState()
    {
        if (!goNextScene)
        {
            GameManager.SceneNo = (int)scene.Typing;
            SceneManager.LoadScene("typingStage"); // タイピングシーンに遷移
            goNextScene = true;
        }
    }

    public void panelReset(int panelNo)
    {
        challengeList.SetActive(panelNo==0);
        customList.SetActive(panelNo==1);
        trainingList.SetActive(panelNo==2);
        ShowMenuList();
    }

    public void openChallenge()
    {
        setting.sayColtu(1);
        GameManager.TypingTab = 0;
        challengeList.SetActive(true);
        customList.SetActive(false);
        trainingList.SetActive(false);
        pAnimator.SetTrigger("fuda");
        lAnimator.SetTrigger("eat");
        talk.text = "ここでいろんなタイピングにちょうせんしてみてね。";
    }

    public void openCustom()
    {
        setting.sayColtu(1);
        GameManager.TypingTab = 1;
        challengeList.SetActive(false);
        customList.SetActive(true);
        trainingList.SetActive(false);
        pAnimator.SetTrigger("fuda");
        lAnimator.SetTrigger("eat");
        talk.text = "ちょっとかしこくなるメニューだよ。\nたのしんでいってね。";
    }

    public void openTraining()
    {
        setting.sayColtu(1);
        GameManager.TypingTab = 2;
        challengeList.SetActive(false);
        customList.SetActive(false);
        trainingList.SetActive(true);
        pAnimator.SetTrigger("fuda");
        lAnimator.SetTrigger("eat");
        talk.text = "タイピングがうまくなりたい人はここでれんしゅうをしよう。";
    }

    private void ShowMenuList()
    {
        double childLines = Math.Ceiling((double)listParent.transform.childCount / 4);
        float contentHeight = (int)childLines * 205; // アイテムの高さ
        listParent.sizeDelta = new Vector2(listParent.sizeDelta.x, contentHeight);
    }
}
