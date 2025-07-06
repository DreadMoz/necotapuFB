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

    private int maxUser = 149;

    void Start()
    {
    }

    void shufflePlayers()
    {
        List<int> playerPool = new List<int>();
        for (int i = 1; i < maxUser; i++)
        {
            playerPool.Add(i);
        }

        for (int I = 0; I < numberOfNPCs; I++)
        {
            if (playerPool.Count == 0) {
                Debug.LogError("No more players to pick.");
                break;
            }
            int randomIndex = Random.Range(0, playerPool.Count);
            pickedPlayers.Add(playerPool[randomIndex]);
            playerPool.RemoveAt(randomIndex);
        }
    }

    public void SpawnNPCs()
    {
        Debug.Log("ランキング数"+gm.savedata.ExRankings.Count);
        if (gm.savedata.ExRankings.Count == 0 || gm.savedata.ExRankings[0].Name == "")
        {
            Debug.Log("SpawnNPCs:ExRankingsが空なのでNPCを作りません。");
            return;
        }

        shufflePlayers();
        // 生成するNPCの数をスポーンポイントの数と比較し、小さい方を使用
        int spawnCount = Mathf.Min(numberOfNPCs, spawnPoints.Length);
        if (spawnPoints.Length < numberOfNPCs) {
            Debug.LogError("Not enough spawn points for the number of NPCs.");
        }

        // 既存のNPCをクリア
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // 指定された数だけNPCをスポーン
        for (int i = 0; i < spawnCount; i++)
        {
            if (i >= pickedPlayers.Count || pickedPlayers[i] >= gm.savedata.ExRankings.Count) {
                Debug.LogError($"Invalid player index: {i}, pickedPlayers length: {pickedPlayers.Count}, ExRankings count: {gm.savedata.ExRankings.Count}");
                continue; // 無効なインデックスをスキップ
            }
            // Y軸周りでランダムな角度を選択
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(110, 220), 0);
            // NPCプレハブのインスタンスを生成し、指定された位置に配置
            GameObject npcInstance = Instantiate(npcPrefab, spawnPoints[i].position, randomRotation, transform);

            // インスタンスにアタッチされているChibiCatスクリプトを取得
            ChibiCat chibiCatScript = npcInstance.GetComponentInChildren<ChibiCat>();

            if (chibiCatScript != null)
            {
                if (gm.savedata.ExRankings.Count > 0)
                {
                    chibiCatScript.setName(gm.savedata.ExRankings[pickedPlayers[i]].Name + gm.getNickname(gm.savedata.ExRankings[pickedPlayers[i]].NickName));
                    chibiCatScript.setChara(gm.savedata.ExRankings[pickedPlayers[i]].CatBody);
                    chibiCatScript.releaseAllEquip();
                    chibiCatScript.changeEquipHands(gm.savedata.ExRankings[pickedPlayers[i]].RightHand, gm.savedata.ExRankings[pickedPlayers[i]].LeftHand, 0);
                    chibiCatScript.changeEquipHead(gm.savedata.ExRankings[pickedPlayers[i]].Head);
                    chibiCatScript.changeEquipGlasses(gm.savedata.ExRankings[pickedPlayers[i]].Glasses);
                }
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
