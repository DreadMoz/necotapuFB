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

    private bool goNextScene = false;    // 次のシーンに遷移するためのフラグ
    

    void Start()
    {
        challengeList.SetActive(false);
        customList.SetActive(false);
        trainingList.SetActive(false);
        panelReset(GameManager.TypingTab);
        pAnimator = housePlayer.GetComponent<Animator>(); // Playerのアニメーターを取得
        lAnimator = littleCat.GetComponent<Animator>(); // littleCatのアニメーターを取得
        lAnimator.SetTrigger("jump");
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
