using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;

    public GameObject npcPrefab; // NPCのプレハブ
    public Transform[] spawnPoints; // NPCを生成する位置を保持する配列
    public int numberOfNPCs = 10; // 生成するNPCの数、デフォルトは10
    List<int> pickedPlayers = new List<int>();   // NPCとして登場するユーザーの順位

    // private int maxUser = 149; // 不要になったため削除

    void Start()
    {
    }

    void shufflePlayers()
    {
        pickedPlayers.Clear(); // 既存のpickedPlayersをクリア
        Debug.Log($"shufflePlayers: gm.savedata.ExRankings.Count = {gm.savedata.ExRankings.Count}");

        if (gm.savedata.ExRankings == null || gm.savedata.ExRankings.Count <= 1)
        {
            // ランキングデータがない、または自分自身しかいない場合はNPCを生成しない
            Debug.Log("shufflePlayers: ランキングデータが不足しているため、NPCをを生成しません。");
            return;
        }

        List<int> realPlayerPool = new List<int>();
        List<int> dummyPlayerPool = new List<int>();

        // リアルユーザーとダミーユーザーを分別
        for (int i = 0; i < gm.savedata.ExRankings.Count; i++)
        {
            var rankData = gm.savedata.ExRankings[i];
            
            // 自分自身は除外
            if (rankData.Uid == gm.savedata.Uid) continue;

            // ダミー判定 (UIDが "dummy_" で始まるもの)
            if (!string.IsNullOrEmpty(rankData.Uid) && rankData.Uid.StartsWith("dummy_"))
            {
                dummyPlayerPool.Add(i);
            }
            else
            {
                realPlayerPool.Add(i);
            }
        }
        
        Debug.Log($"shufflePlayers: RealUsers={realPlayerPool.Count}, DummyUsers={dummyPlayerPool.Count}");

        // リストをシャッフルするローカル関数
        void ShuffleList(List<int> list)
        {
            for (int i = 0; i < list.Count; i++) {
                int temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        // 両方のプールをシャッフル
        ShuffleList(realPlayerPool);
        ShuffleList(dummyPlayerPool);

        int countNeeded = numberOfNPCs;
        
        // 1. リアルユーザーから優先的に選出
        for (int i = 0; i < realPlayerPool.Count && pickedPlayers.Count < countNeeded; i++)
        {
            pickedPlayers.Add(realPlayerPool[i]);
        }
        
        // 2. 足りない分をダミーユーザーから補充
        for (int i = 0; i < dummyPlayerPool.Count && pickedPlayers.Count < countNeeded; i++)
        {
            pickedPlayers.Add(dummyPlayerPool[i]);
        }

        Debug.Log($"shufflePlayers: 最終的に {pickedPlayers.Count} 体のNPCを選出しました。");
    }

    public void SpawnNPCs()
    {
        Debug.Log("SpawnNPCs: ランキング数"+gm.savedata.ExRankings.Count);
        Debug.Log($"SpawnNPCs: gm.savedata.ExRankings == null : { (gm.savedata.ExRankings == null).ToString() }");
        if (gm.savedata.ExRankings == null || gm.savedata.ExRankings.Count == 0 || (gm.savedata.ExRankings.Count == 1 && gm.savedata.ExRankings[0].Uid == gm.savedata.Uid))
        {
            Debug.Log("SpawnNPCs:ExRankingsが空、または自分自身のデータのみなのでNPCを作りません。");
            return;
        }

        shufflePlayers();

        // 生成するNPCの数をスポーンポイントの数と、実際にpickedPlayersにあるプレイヤーの数と比較し、小さい方を使用
        int actualSpawnCount = Mathf.Min(pickedPlayers.Count, spawnPoints.Length);
        Debug.Log($"SpawnNPCs: actualSpawnCount = {actualSpawnCount}, spawnPoints.Length = {spawnPoints.Length}, pickedPlayers.Count = {pickedPlayers.Count}");
        
        if (spawnPoints.Length < actualSpawnCount) {
            Debug.LogError("SpawnNPCs: スポーンポイントがNPCの数に対して不足しています。");
        }

        // 既存のNPCをクリア
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 指定された数だけNPCをスポーン
        for (int i = 0; i < actualSpawnCount; i++)
        {
            // pickedPlayersはshufflePlayersで既に適切なインデックスが設定されているはずなので、
            // ここでの追加チェックは不要ですが、念のためログは残しておきます。
            if (pickedPlayers.Count == 0 || i >= pickedPlayers.Count)
            {
                Debug.LogError($"SpawnNPCs: pickedPlayersに十分なプレイヤーがいません。NPCの生成を停止します。現在まで {i} 体生成済み。");
                break;
            }
            
            // Y軸周りでランダムな角度を選択
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(110, 220), 0);
            // NPCプレハブのインスタンスを生成し、指定された位置に配置
            GameObject npcInstance = Instantiate(npcPrefab, spawnPoints[i].position, randomRotation, transform);

            // インスタンスにアタッチされているChibiCatスクリプトを取得
            ChibiCat chibiCatScript = npcInstance.GetComponentInChildren<ChibiCat>();

            if (chibiCatScript != null)
            {
                chibiCatScript.setName(gm.savedata.ExRankings[pickedPlayers[i]].FirstName + gm.getNickname(gm.savedata.ExRankings[pickedPlayers[i]].NicknameNo));
                chibiCatScript.setChara(gm.savedata.ExRankings[pickedPlayers[i]].CatBody);
                chibiCatScript.releaseAllEquip();
                int bagItem = (gm.savedata.ExRankings[pickedPlayers[i]].BackpackType == 25) ? 0x10 : 0;
                chibiCatScript.changeEquipHands(gm.savedata.ExRankings[pickedPlayers[i]].RightHand, gm.savedata.ExRankings[pickedPlayers[i]].LeftHand, bagItem);
                chibiCatScript.changeEquipHead(gm.savedata.ExRankings[pickedPlayers[i]].Head);
                chibiCatScript.changeEquipGlasses(gm.savedata.ExRankings[pickedPlayers[i]].Glasses);
            }
        }
    }

    // デバッグやUIから呼び出すためのメソッド
    public void UpdateNPCCount(int newCount)
    {
        if ((newCount < 0) || (10 < newCount))
        {
            return;
        }
        if (numberOfNPCs == newCount)
        {
            return;
        }
        numberOfNPCs = newCount;
        SpawnNPCs();
    }
}
