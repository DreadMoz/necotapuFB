using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // UI関連のクラスを使用するための名前空間

public class DetailMenu : MonoBehaviour
{
    private Button thisButton;
    private int id;
    private int level;

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

    private bool goNextScene = false;    // 次のシーンに遷移するためのフラグ

    void Awake()
    {
        thisButton = this.GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void showStars()
    {
        id = GameManager.TypingDataId / 3;
        level = transform.GetSiblingIndex();    // GameObjectの兄弟の中でのインデックスを取得
        int medal = gm.savedata.Medals[id * 3 + level];

        resetStars();

        switch (medal)
        {
            case 0:
                if (id * 3 + level == 3)     // 閉じない
                {
                    break;
                }
                memo.SetActive(false);
                star0.SetActive(false);
                thisButton.interactable = false;
                break;
            case 1:
                break;
            case 2:
                star1.SetActive(true);
                break;
            case 3:
                star1.SetActive(true);
                star2.SetActive(true);
                break;
            case 5:
                gm.savedata.Medals[id * 3 + level] = 1;       // 新規Detail表示から1へ
                magicProof.SetActive(true);         // magicProofオブジェクトをアクティブにする
                ParticleSystem particleSystem = magicProof.GetComponentInChildren<ParticleSystem>();
                if (particleSystem != null) {       // ParticleSystemが見つかった場合、再生する
                    particleSystem.Play();
                } else {
                    Debug.LogError("ParticleSystemが見つかりません。");
                }
                break;
            default:
                star1.SetActive(true);
                star2.SetActive(true);
                star3.SetActive(true);
                break;
        }
    }

    public void resetStars()
    {
        thisButton.interactable = true;
        star0.SetActive(true);
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        memo.SetActive(true);
    }

    public void chooseTypingLevel()
    {
        GameManager.SetTypingDataLevel(level);

        if (!goNextScene)
        {
            GameManager.SceneNo = (int)scene.Typing;
            SceneManager.LoadScene("typingStage"); // タイピングシーンに遷移
            goNextScene = true;
        }
    }
}
