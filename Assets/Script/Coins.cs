using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coins : MonoBehaviour
{
    public GameObject coinPrefab; // NPCのプレハブ
    // Start is called before the first frame update
    public Transform[] spawnPoints = new Transform[20]; // NPCを生成する位置を保持する配列

    public Vector3[] spawnAreaCenters; // スポーンエリアの中心
    public Vector3 spawnAreaSize = new Vector3(6, 2, 0); // スポーンエリアのサイズ

    void Awake()
    {
        spawnAreaCenters = new Vector3[]{
            new Vector3(0, 5, 90),
            new Vector3(-35, 3, 90),
            new Vector3(30, 0, 90)
        };
    }
    void Start()
    {
        InitializeSpawnPoints();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void spawnTestRight1()
    {
        SpawnCoins(1, 2);
    }

    public void spawnTestCenter5()
    {
        SpawnCoins(5, 0);
    }
    public void spawnTestCombo()
    {
        SpawnCoins(1, 1);
    }
    
    public void SpawnCoins(int spawnCount, int areaIndex)
    {
        // スポーンポイントの数より生成する数が多い場合に対処
        int effectiveSpawnCount = Mathf.Min(spawnCount, spawnPoints.Length);

        // 指定された数だけコインをスポーン
        for (int i = 0; i < effectiveSpawnCount; i++)
        {
            // Y軸周りでランダムな角度を選択
            Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 359));
            // コインプレハブのインスタンスを生成し、指定された位置に配置
            GameObject coinInstance;
            int forceDirectionX;
            int forceDirectionY;
            if (areaIndex == 0)
            {
                coinInstance = Instantiate(coinPrefab, spawnPoints[i].position, randomRotation, transform);
                // spawnAreaCctionXenterからの距離に基づいて力の方向を計算
                forceDirectionX = (int)(coinInstance.transform.position.x - spawnAreaCenters[areaIndex].x);
                forceDirectionY = 60;
            }
            else
            {
                coinInstance = Instantiate(coinPrefab, spawnAreaCenters[areaIndex], randomRotation, transform);
                forceDirectionX = (int)(-0.8 * spawnAreaCenters[areaIndex].x);
                forceDirectionY = 50;
            }
            Rigidbody2D rb = coinInstance.GetComponent<Rigidbody2D>();

            // spawnAreaCenterからの距離に基づいて力の方向を計算
            Vector2 forceDirection = new Vector2(forceDirectionX, forceDirectionY);
            float forceMagnitude = Random.Range(0.8f, 2f); // 力の大きさ
            // 力を加える
            rb.AddForce(forceDirection * forceMagnitude, ForceMode2D.Impulse);
        }
    }

    private void InitializeSpawnPoints()
    {
        Debug.Log($"Using spawnAreaCenters: {spawnAreaCenters}");
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // 最初のスポーンエリアでポイントを設定（例として、すべてのエリアで共通のポイントを使用）
            Vector3 randomPosition = new Vector3(
                Random.Range(spawnAreaCenters[0].x - spawnAreaSize.x / 2, spawnAreaCenters[0].x + spawnAreaSize.x / 2),
                Random.Range(spawnAreaCenters[0].y - spawnAreaSize.y / 2, spawnAreaCenters[0].y + spawnAreaSize.y / 2),
                spawnAreaCenters[0].z
            );

            // 空のオブジェクトを生成し、スポーンポイントとして使う
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.transform.position = randomPosition;
            spawnPoint.transform.parent = transform;

            // Transformコンポーネントを配列に追加
            spawnPoints[i] = spawnPoint.transform;
        }
    }
}
