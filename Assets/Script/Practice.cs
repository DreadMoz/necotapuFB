using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Practice : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;

    private int[] medalSum;             // 指練習、ランダム、応用の合計
    private int[] medalTop;             // トップ画面の星の数
    private int medalTopNum;            // Practiceの項目数

    // Start is called before the first frame update
    void Awake()
    {
        medalTopNum = transform.childCount;
        medalTop = new int[medalTopNum];
        medalSum = new int[medalTopNum];
    }

    public void calcStars()     // ３ステージの合計を算出するところ
    {
        setTopStars();
        showRoomMenu();
    }
    public int[] setTopStars()
    {
        medalTopNum = transform.childCount;
        medalTop = new int[medalTopNum];
        medalSum = new int[medalTopNum];
        
        int[] medals = gm.savedata.Medals;
        for (int i=0; i < medals.Length; i++)
        {
            if (i < medals.Length - 1)    // 次があればステージオープン
            {
                if (((medals[i] == 3) || (medals[i] == 4)) && (medals[i + 1] == 0))    // 星2つ以上で次がクローズだったら
                {
                    gm.savedata.Medals[i + 1] = 5;       // Detailオープン
                    medals[i + 1] = 5;                         // 花火セット
                    Debug.Log("Oepned detail id:" + (i + 1));
                }
            }
        }

        for (int i = 0; i < medalTopNum; i++)
        {
            // ３つのステージの星の合計
            medalSum[i] = remove5(medals[i * 3]) + remove5(medals[i * 3 + 1]) + remove5(medals[i * 3 + 2]);

            if (medalSum[i] == 12)      // 4 + 4 + 4
            {
                medalTop[i] = 4;    // 星3つ
            }
            else if (medalSum[i] >= 9)   // 4 + 4 + 1
            {
                medalTop[i] = 3;    // 星2つ
            }
            else if (medalSum[i] >= 3)   // 2 + 1 + 0
            {
                medalTop[i] = 2;    // 星1つ
            }
            else
            {
                if (medalTop[i] != 5)
                {
                    medalTop[i] = 1;    // 星0こ
                }
            }

            if (medalSum[i] >= 10)      // 4 + 4 + 2
            {
                if (i < medalTopNum - 1)    // 次があればRoomオープンチェック
                {
                    if (medalTop[i + 1] == 0) // 次が錠状態なら
                    {
                        medalTop[i + 1] = 5; // Room花火打ち上げセット
//                        Debug.Log("Opend MedalTop id:" + (i + 1));
                    }
                }
            }
            if (i > 0)
            {
                if (medalSum[i - 1] < 10)    // １つ前が 4 + 4 + 2 未満なら
                {
                    if (i != 1)     // あいうえおは閉じない
                    {
                        medalTop[i] = 0;    // 錠
                    }
                }
            }
        }
        return medalTop;
    }

    private int remove5(int no)
    {
        if (no == 5)
        {
            return 1;
        }
        return no;
    }

    public int getMedalTop(int id)
    {
        return medalTop[id];
    }

    public void setMedalTop(int id, int star)
    {
        medalTop[id] = star;
    }

    // 詳細画面表示
    public void showDetail()
    {
        int id = GameManager.TypingDataId;
        
        if ( id >= 0)
        {
            int roomId = id / 3;
            Transform childTransform = gameObject.transform.GetChild(roomId);
            RoomMenu roommenu = childTransform.GetComponent<RoomMenu>();
            roommenu.showDetail();
        }
    }

    public void showRoomMenu()       // ルームメニュー表示
    {
        Debug.Log("medalTopNum:" + medalTopNum);
        for (int no = 0; no < medalTopNum; no++)
        {
            Transform childTransform = gameObject.transform.GetChild(no);
            RoomMenu roommenu = childTransform.GetComponent<RoomMenu>();
            roommenu.showStars();
        }
    }
}
