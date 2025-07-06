using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshProの名前空間を使用

public class Ranking : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;

    [SerializeField]
    private StatusUI statusBord; // ステータスウィンドウ
    [SerializeField]
    private GameObject rankBoardPrefab; // RankBoardのプレファブ
    [SerializeField]
    private GameObject rankBoardMePrefab; // RankBoardMeのプレファブ

    [SerializeField]
    private Transform rankBoardParent;  // RankBoardをインスタンス化する親オブジェクト

    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private RectTransform contentPanel;
    [SerializeField]
    private float itemHeight; // アイテム間のスペース
    // Start is called before the first frame update
    void Start()
    {
        DisplayRankings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // JavaScriptからデータを受け取るメソッド
    public void ReceiveDataFromJS(string data)
    {
        Debug.Log("Received data from JS: " + data);
        // 受け取ったデータを処理
    }

    // ランキングデータを受け取って表示するメソッド
    public void DisplayRankings()
    {
        // 既存のランキングをクリアする
        foreach (Transform child in rankBoardParent)
        {
            Destroy(child.gameObject);
        }
        if (gm.savedata.ExRankings == null)
        {
            return;
        }

        int kpm;
        int myBordSet = 0;
        int rankingNo = 0;
        int count = 0;
        // 新しいランキングデータをUIに表示する
        foreach (ExRank rank in gm.savedata.ExRankings)
        {
            kpm = rank.Kpm;
            if (kpm <= gm.savedata.Status[st.Kpm] && myBordSet == 0)   // 自分の場合（1回だけ）
            {
                GameObject myRankBoard = Instantiate(rankBoardMePrefab, rankBoardParent);

                // RankBoardのUIコンポーネントにデータを設定
                gm.savedata.Status[st.Rank] = rankingNo + 1;
                myRankBoard.transform.Find("Rank").GetComponent<TextMeshProUGUI>().text = gm.savedata.Status[st.Rank].ToString();
                myRankBoard.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = gm.savedata.UserName + gm.getNickname(gm.savedata.Equipment[eq.NickName]);
                myRankBoard.transform.Find("Kpm").GetComponent<TextMeshProUGUI>().text = gm.savedata.Status[st.Kpm].ToString();
                myBordSet = 1;
                statusBord.dispStatus();
                count++;
            }
            // RankBoardのプレファブをインスタンス化
            GameObject newRankBoard = Instantiate(rankBoardPrefab, rankBoardParent);

            // RankBoardのUIコンポーネントにデータを設定
            rankingNo = rank.Ranking + myBordSet;
            newRankBoard.transform.Find("Rank").GetComponent<TextMeshProUGUI>().text = rankingNo.ToString();
            newRankBoard.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = rank.Name + gm.getNickname(rank.NickName);
            newRankBoard.transform.Find("Kpm").GetComponent<TextMeshProUGUI>().text = rank.Kpm.ToString();
            count++;
        }
        if ((count == 199) && myBordSet == 0)   // 自分が最下位の場合
        {
                GameObject myRankBoard = Instantiate(rankBoardMePrefab, rankBoardParent);

                // RankBoardのUIコンポーネントにデータを設定
                gm.savedata.Status[st.Rank] = rankingNo + 1;
                myRankBoard.transform.Find("Rank").GetComponent<TextMeshProUGUI>().text = gm.savedata.Status[st.Rank].ToString();
                myRankBoard.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = gm.savedata.UserName + gm.getNickname(gm.savedata.Equipment[eq.NickName]);
                myRankBoard.transform.Find("Kpm").GetComponent<TextMeshProUGUI>().text = gm.savedata.Status[st.Kpm].ToString();
                statusBord.dispStatus();
        }
//        ScrollTo(gm.savedata.Status[st.Rank]);
    }

    public void SetTo(int itemIndex)
    {
        float contentHeight = contentPanel.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float targetPositionY = itemHeight * itemIndex;
        float scrollPosition = 0;

        if (contentHeight > viewportHeight)
        {
            scrollPosition = (contentHeight - targetPositionY - viewportHeight / 2) / (contentHeight - viewportHeight);
            scrollPosition = Mathf.Clamp01(scrollPosition);
        }
        scrollRect.verticalNormalizedPosition = scrollPosition - 0.05f;
    }
    public void ScrollTo(int itemIndex)
    {
        float contentHeight = contentPanel.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float targetPositionY = itemHeight * itemIndex;
        float scrollPosition = 0;

        if (contentHeight > viewportHeight)
        {
            scrollPosition = (contentHeight - targetPositionY - viewportHeight / 2) / (contentHeight - viewportHeight);
            scrollPosition = Mathf.Clamp01(scrollPosition);
        }
        StartCoroutine(SmoothScroll(scrollPosition));
    }

    private IEnumerator SmoothScroll(float targetPosition)
    {
        float timeElapsed = 0;
        float duration = 1f; // スクロールにかける時間（秒）
        float startPosition = scrollRect.verticalNormalizedPosition;

        while (timeElapsed < duration)
        {
            float newPos = Mathf.Lerp(startPosition, targetPosition, timeElapsed / duration);
            scrollRect.verticalNormalizedPosition = newPos;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = targetPosition;
    }
}
