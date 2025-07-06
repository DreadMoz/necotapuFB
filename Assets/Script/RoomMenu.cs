using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // UI関連のクラスを使用するための名前空間
using TMPro;

public class RoomMenu : MonoBehaviour
{
    private TypingDetail typingDetail;
    private Button thisButton;
    private int id;

    [SerializeField]
    private GameObject memo;
    [SerializeField]
    private GameObject star0;
    [SerializeField]
    private GameObject star1;
    [SerializeField]
    private GameObject star2;
    [SerializeField]
    private GameObject star3;
    [SerializeField]
    private GameObject magicProof;

    [SerializeField]
    private GameManager gm;

    [SerializeField]
    private GameObject housePlayer;
    private Animator pAnimator;

    [SerializeField]
    private GameObject littleCat;
    private Animator lAnimator;

    [SerializeField]
    private Practice practice;

    void Awake()
    {
        housePlayer = GameObject.Find("PlayerHouse");
        littleCat = GameObject.Find("LittleCat");
        pAnimator = housePlayer.GetComponent<Animator>(); // Playerのアニメーターを取得
        lAnimator = littleCat.GetComponent<Animator>(); // littleCatのアニメーターを取得
        thisButton = this.GetComponent<Button>();
        resetStars();
        typingDetail = FindObjectOfType<TypingDetail>();
        practice = GetComponentInParent<Practice>();
        id = transform.GetSiblingIndex();    // GameObjectの兄弟の中でのインデックスを取得
    }

    private void Start()
    {
//        showStars();
    }
    void Update()
    {

    }

    public void showStars()
    {
        if (practice == null)
        {
            Awake();
        }
        int stars = practice.getMedalTop(id);
        if (thisButton == null)
        {
            thisButton = GetComponent<Button>();
        }

        switch (stars)
        {
            case 0:         // 閉じた状態
                resetStars();
                if (id < 2)     // 閉じない
                {
                    break;
                }
                memo.SetActive(false);
                star0.SetActive(false);
                thisButton.interactable = false;
                break;
            case 1:         // 星ゼロ
                break;
            case 2:         // 星１
                star1.SetActive(true);
                break;
            case 3:         // 星２
                star1.SetActive(true);
                star2.SetActive(true);
                break;
            case 5:         // 花火
                practice.setMedalTop(id, 1);        // 新規Room表示から星1へ
                magicProof.SetActive(true);         // magicProofオブジェクトをアクティブにする
                ParticleSystem particleSystem = magicProof.GetComponentInChildren<ParticleSystem>();
                if (particleSystem != null) {       // ParticleSystemが見つかった場合、再生する
                    particleSystem.Play();
                } else {
                    Debug.LogError("ParticleSystemが見つかりません。");
                }
                break;
            default:         // 星３
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(true);
                break;
        }
    }

    public void resetStars()
    {
        thisButton.interactable = true;
        memo.SetActive(true);
        star0.SetActive(true);
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
    }

    public void showDetail()
    {
        if (pAnimator == null)
        {
            Awake();
        }
        pAnimator.SetTrigger("fuda");
        lAnimator.SetTrigger("down");

        GameManager.TypingDataId = id * 3;
        TextMeshProUGUI comment = this.GetComponentInChildren<TextMeshProUGUI>();
        typingDetail = FindObjectOfType<TypingDetail>();
        typingDetail.setComment(comment.text);
        typingDetail.show();
    }
}
