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

        // 自分の最新KPMを反映して再ソート
        var myRankData = gm.savedata.ExRankings.Find(r => r.Uid == gm.savedata.Uid);
        if (myRankData != null)
        {
            myRankData.Kpm = gm.savedata.Status[st.Kpm];
        }

        // KPM降順でソート
        gm.savedata.ExRankings.Sort((a, b) => b.Kpm - a.Kpm);

        // 修正: 既存のRanking値の最小値を探して、それを基準にする
        int minRank = int.MaxValue;
        foreach(var r in gm.savedata.ExRankings) {
            if (r.Ranking < minRank && r.Ranking > 0) minRank = r.Ranking;
        }
        if (minRank == int.MaxValue) minRank = 1;

        // もし自分が1位になってminRankより上に行く場合を考慮し、
        // 単純に minRank から連番を振ります。
        // (ステージの切れ目での順位変動はクライアントだけで完結しない場合がありますが、表示用として)
        for (int i = 0; i < gm.savedata.ExRankings.Count; i++)
        {
            gm.savedata.ExRankings[i].Ranking = minRank + i;
        }


        // 新しいランキングデータをUIに表示する
        foreach (ExRank rank in gm.savedata.ExRankings)
        {
            GameObject rankBoard;
            // リスト内のUidが自分と一致するか確認
            if (rank.Uid == gm.savedata.Uid)
            {
                rankBoard = Instantiate(rankBoardMePrefab, rankBoardParent);
                // 自分の順位を更新
                gm.savedata.Status[st.Rank] = rank.Ranking;
                statusBord.dispStatus();
            }
            else
            {
                rankBoard = Instantiate(rankBoardPrefab, rankBoardParent);
            }

            // UI設定
            if (rankBoard != null)
            {
                rankBoard.transform.Find("Rank").GetComponent<TextMeshProUGUI>().text = rank.Ranking.ToString();
                rankBoard.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = rank.FirstName + gm.getNickname(rank.NicknameNo);
                rankBoard.transform.Find("Kpm").GetComponent<TextMeshProUGUI>().text = rank.Kpm.ToString();
            }
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
