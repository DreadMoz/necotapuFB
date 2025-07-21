using System;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using Unity.VisualScripting;
public class GssIndex
{
    public const int Status = 3;
    public const int Equipment = 7;
    public const int Kpms = 14;
    public const int Medals = 16;
    public const int Inventory = 21;
}
public class GssSize
{
    public const int Status = 3;
    public const int Equipment = 7;
    public const int Kpms = 2;
    public const int Medals = 5;
    public const int Inventory = 4;
}
// Gold,Server,Rank,userName
public class st
{
    public const int Gold = 0;
    public const int Server = 1;
    public const int Rank = 2;
    public const int Kpm = 3;
}
// RightHand,Head(151),Glasses(121),LeftHand,CatBody(201)あえて0,CatFace(101),NickName(211)
public class eq
{
    public const int RightHand = 0;
    public const int Head = 1;
    public const int Glasses = 2;
    public const int LeftHand = 3;
    public const int CatBody = 4;
    public const int CatFace = 5;
    public const int NickName = 6;
}
// Gold,Server,Rank,userName
public class se
{
    public const int GachaCnt = 0;
    public const int Volume = 1;
    public const int CatNum = 2;
    public const int MailChar = 3;
    public const int Mute = 4;
    public const int LastLogin = 5;
    public const int Capital = 6;
    public const int KeyType = 7;
    public const int dummy8 = 8;
    public const int dummy9 = 9;
}

[System.Serializable]
public class SerializableRankingData
{
    public string[][] rankingData;
}

// 拡張機能ランキング
[Serializable]
public class ExRank
{
    public string Email { get; set; }
    public int Ranking { get; set; }
    public string Name { get; set; }
    public int RightHand { get; set; }
    public int Glasses { get; set; }
    public int Head { get; set; }
    public int LeftHand { get; set; }
    public int CatBody { get; set; }
    public int CatFace { get; set; }
    public int NickName { get; set; }
    public int Kpm { get; set; }
}







[Serializable]
public class PlayerData
{
    // プレイヤー情報
    public string Email { get; set; }
    public string UserName { get; set; }
    public string Ou { get; set; }
    public string LastName { get; set; }
    
    // ステータス
    public int Gold { get; set; }
    public int Stage { get; set; }
    public int Ranking { get; set; }
    public int Kpm { get; set; }
    
    // 装備（全7項目）
    public int RightHand { get; set; }
    public int Head { get; set; }
    public int Glasses { get; set; }
    public int LeftHand { get; set; }
    public int CatBody { get; set; }
    public int CatFace { get; set; }
    public int NickName { get; set; }
    
    // インベントリ・アイテム
    public int[] Inventory { get; set; } = new int[60];
    public bool[] Items { get; set; } = new bool[256];
    
    // メダル・KPM履歴・設定
    public int[] Medals { get; set; } = new int[100];
    public int[] Kpms { get; set; } = new int[8];
    public int[] Settings { get; set; } = new int[10];
    
    // タイピング履歴
    public Dictionary<int, TypingResult> TypingResults { get; set; } = new Dictionary<int, TypingResult>();
}

[Serializable]
public class TypingResult
{
    public int Count { get; set; }
    public int TotalKpmSum { get; set; }
    public int TotalAccuracySum { get; set; }
}

[CreateAssetMenu(fileName = "SaveData", menuName = "SaveData")]
public class SaveData : ScriptableObject
{
    System.Random random = new System.Random(); // Random オブジェクトのインスタンスを作成
    
    // 新しいデータ構造
    [SerializeField] public PlayerData PlayerData = new PlayerData();
    
    // ExRankのリストを作成
    public List<ExRank> ExRankings = new List<ExRank>();

    [SerializeField]
    public string UserName;

    [SerializeField]
    public string Email;

    [SerializeField]
    public string Ou;

    [SerializeField]
    public string LastName;

    [SerializeField]
    public int[] Status = new int[4];

    [SerializeField]
    public int[] Equipment = new int[7];

    [SerializeField]
    public int[] Inventory = new int[60];

    [SerializeField]
    public bool[] Items = new bool [256];

    [SerializeField]
    public int[] Medals = new int[100];

    [SerializeField]
    public int[] Kpms = new int[8];

    [SerializeField]
    public int[] Settings = new int[10];


    // 拡張機能からランキング一覧を取得する。
    public void setRankingFromLocal(string rankingData)
    {
        Debug.Log("Received Ranking JSON 型をチェック: " + rankingData);

        ExRankings.Clear();
        int existRanking = 0;

        try
        {
            var jsonResponse = JsonConvert.DeserializeObject<SerializableRankingData>(rankingData);
            if (jsonResponse != null && jsonResponse.rankingData != null)
            {
                foreach (var item in jsonResponse.rankingData)
                {
                    // Stageの値をチェックし、変換できない場合はこの項目の処理をスキップ
                    if (item[2].ToString() == "")       // 名前がなければ飛ばす
                    {
                        continue;
                    }
                    if (existRanking >= 200)            // 自分を入れて２００を超えたら終了
                    {
                        break;
                    }
                    if (item[0].ToString() == Email)    // 自分自身は登録しない。スキップ
                    {
                        Status[st.Server] = Convert.ToInt32(item[1]);   // 使っていない順位にはステージが入っている。
                        continue;
                    }
                    var rank = new ExRank
                    {
                        Email = item[0].ToString(),
                        Ranking = ++existRanking,
                        Name = item[2].ToString(),
                        RightHand = Convert.ToInt32(item[3]),
                        Glasses = Convert.ToInt32(item[4]),
                        Head = Convert.ToInt32(item[5]),
                        LeftHand = Convert.ToInt32(item[6]),
                        CatBody = Convert.ToInt32(item[7]),
                        CatFace = Convert.ToInt32(item[8]),
                        NickName = Convert.ToInt32(item[9]),
                        Kpm = Convert.ToInt32(item[10])
                    };
                    ExRankings.Add(rank);
                }
                foreach (var rank in ExRankings)
                {
                    Debug.Log($"Ranking: {rank.Ranking}： {rank.Name}： {rank.Kpm}");
                }
            }
            else
            {
                Debug.LogError("ランキングデータのデシリアライズに失敗しました。");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("データの読み込み中に例外発生: " + ex.Message);
        }
    }

    public void updateLastName(string newLastName)
    {
        LastName = newLastName;
    }

    // 初期データ登録。
    public void setNewData(string googleMail, string googleFirstName, string googleLastName, string googleOu)
    {
        Debug.Log("setNewData: " + googleMail + googleFirstName + googleLastName + googleOu);

        // ApiStatus に値を設定
        Email = googleMail;
        Ou = googleOu;
        LastName = googleLastName;
        Status[st.Gold] = 100;

        // ExRank に値を設定
        Status[st.Server] = 0;
        Status[st.Rank] = 0;
        UserName = googleFirstName;
        Equipment[eq.RightHand] = 0;
        Equipment[eq.Glasses] = 0;
        Equipment[eq.Head] = 0;
        Equipment[eq.LeftHand] = 0;
        Equipment[eq.CatFace] = 0;
        Equipment[eq.NickName] = 0;
        Status[st.Kpm] = 10;

        for (int i = 0; i < Inventory.Length; i++)
        {
            Inventory[i] = 0;
        }
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i] = false;
        }
        for (int i = 0; i < Medals.Length; i++)
        {
            Medals[i] = 0;
        }
        Medals[0] = 1;
        Medals[3] = 1;
        for (int i = 0; i < Kpms.Length; i++)
        {
            Kpms[i] = 10;
        }
        Settings[se.GachaCnt] = 4;
        Settings[se.Volume] = 20;
        Settings[se.Mute] = 0;
        Settings[se.MailChar] = 1;
        Settings[se.CatNum] = 10;

        DateTime today = DateTime.Now;
        Settings[se.LastLogin] = today.Year * 10000 + today.Month * 100 + today.Day;
        Settings[se.Capital] = 0;
        Settings[se.KeyType] = 1;

        // 新しい構造も初期化
        MigrateToNewStructure();
    }

    // 既存データから新しい構造への移行
    public void MigrateToNewStructure()
    {
        PlayerData.Email = Email;
        PlayerData.UserName = UserName;
        PlayerData.Ou = Ou;
        PlayerData.LastName = LastName;
        PlayerData.Gold = Status[st.Gold];
        PlayerData.Stage = Status[st.Server];
        PlayerData.Ranking = Status[st.Rank];
        PlayerData.Kpm = Status[st.Kpm];
        PlayerData.RightHand = Equipment[eq.RightHand];
        PlayerData.Head = Equipment[eq.Head];
        PlayerData.Glasses = Equipment[eq.Glasses];
        PlayerData.LeftHand = Equipment[eq.LeftHand];
        PlayerData.CatBody = Equipment[eq.CatBody];
        PlayerData.CatFace = Equipment[eq.CatFace];
        PlayerData.NickName = Equipment[eq.NickName];
        PlayerData.Inventory = Inventory;
        PlayerData.Items = Items;
        PlayerData.Medals = Medals;
        PlayerData.Kpms = Kpms;
        PlayerData.Settings = Settings;
    }

    // 新しい構造から既存データへの復元
    public void RestoreFromNewStructure()
    {
        Email = PlayerData.Email;
        UserName = PlayerData.UserName;
        Ou = PlayerData.Ou;
        LastName = PlayerData.LastName;
        Status[st.Gold] = PlayerData.Gold;
        Status[st.Server] = PlayerData.Stage;
        Status[st.Rank] = PlayerData.Ranking;
        Status[st.Kpm] = PlayerData.Kpm;
        Equipment[eq.RightHand] = PlayerData.RightHand;
        Equipment[eq.Head] = PlayerData.Head;
        Equipment[eq.Glasses] = PlayerData.Glasses;
        Equipment[eq.LeftHand] = PlayerData.LeftHand;
        Equipment[eq.CatBody] = PlayerData.CatBody;
        Equipment[eq.CatFace] = PlayerData.CatFace;
        Equipment[eq.NickName] = PlayerData.NickName;
        Inventory = PlayerData.Inventory;
        Items = PlayerData.Items;
        Medals = PlayerData.Medals;
        Kpms = PlayerData.Kpms;
        Settings = PlayerData.Settings;
    }

    // 新しい構造でFirebaseに保存するためのシリアライズ
    public string SerializeForFirebase()
    {
        return JsonConvert.SerializeObject(PlayerData);
    }

    // 新しい構造でFirebaseから読み込むためのデシリアライズ
    public void DeserializeFromFirebase(string json)
    {
        PlayerData = JsonConvert.DeserializeObject<PlayerData>(json);
        RestoreFromNewStructure(); // 既存データにも反映
    }

    // タイピング結果を更新
    public void UpdateTypingResult(int promptId, int kpm, int accuracy)
    {
        if (!PlayerData.TypingResults.ContainsKey(promptId))
        {
            PlayerData.TypingResults[promptId] = new TypingResult();
        }
        
        var result = PlayerData.TypingResults[promptId];
        result.Count++;
        result.TotalKpmSum += kpm;
        result.TotalAccuracySum += accuracy;
    }

    // 拡張機能からステータスデータを取得する。
    public void setStatusFromLocal(string statusData)
     {
        Debug.Log("Received Status JSON: " + statusData);
        // 新しい構造でFirebaseから読み込む
        try
        {
            DeserializeFromFirebase(statusData);
            Debug.Log("Firebaseデータを読み込みました。");
        }
        catch (Exception ex)
        {
            Debug.LogError("Firebaseデータの読み込みに失敗: " + ex.Message);
        }
    }

    // 拡張機能なし GSSから最低限のデータ取得
    public void LoadAllDataFromGss(IList<object> list)
    {
        try
        {
            // ApiStatus に値を設定
            Email = list[0].ToString();
            Ou = list[1].ToString();
            LastName = list[2].ToString();
            Status[st.Gold] = Convert.ToInt32(list[3]);

            // ExRank に値を設定
            Status[st.Server] = Convert.ToInt32(list[4]);
            Status[st.Rank] = Convert.ToInt32(list[5]);
            UserName = list[6].ToString();
            Equipment[eq.RightHand] = 0;
            Equipment[eq.Glasses] = 0;
            Equipment[eq.Head] = 0;
            Equipment[eq.LeftHand] = 0;
            Equipment[eq.CatBody] = Convert.ToInt32(list[11]);
            Equipment[eq.CatFace] = 0;
            Equipment[eq.NickName] = 0;
            Status[st.Kpm] = Convert.ToInt32(list[14]);

            // ここは配列8<-文字列
            // DecodeKpmData(list[15].ToString()); // 削除済み

            string[] gssMedals = new string[5];
            gssMedals[0] = list[16].ToString();
            gssMedals[1] = list[17].ToString();
            gssMedals[2] = list[18].ToString();
            gssMedals[3] = list[19].ToString();
            gssMedals[4] = list[20].ToString();

            // ここはlong[5]をint[100]に変換
            // DecodeMedalData(gssMedals); // 削除済み

            string[] gssItems = new string[4];
            gssItems[0] = list[21].ToString();
            gssItems[1] = list[22].ToString();
            gssItems[2] = list[23].ToString();
            gssItems[3] = list[24].ToString();

            // ここはlong[4]をbool[100]に変換
            // DecodeItemData(gssItems); // 削除済み

            setInventoryFromItems();
            
            // 新しい構造にも反映
            MigrateToNewStructure();
        }
        catch (FormatException ex)
        {
            // エラーメッセージとスタックトレースをログに記録
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
        catch (Exception ex)
        {
            // その他の例外タイプ
            Console.WriteLine($"Unexpected error: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
        }
    }

    private void setInventoryFromItems()
    {
        int inventoryId = 0;
        Array.Clear(Inventory, 0, Inventory.Length);
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] == true)
            {
                Inventory[inventoryId] = i;
                inventoryId++;
            }
        }
    }


















    public int getBlankInventoryIndex()
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == 0)
            {
                return i;
            }
        }
        return -1;
    }

    public bool existInventory(int id)
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (Inventory[i] == id)
            {
                return true;
            }
        }
        return false;
    }

    public bool existEquipment(int id)
    {
        for (int i = 0; i < Equipment.Length; i++)
        {
            if (Equipment[i] == id)
            {
                return true;
            }
        }
        return false;
    }

    public void updateKpm(int newKpm)
    {
        // 要素1からlengthまでを0からxに移動
        for (int i = 0; i < Kpms.Length-1; i++)
        {
            if (Kpms[i + 1] < 0)
            {
                Kpms[i + 1] = 10;
            }
            Kpms[i] = Kpms[i + 1];
        }
        // 最後尾の要素に新しい値を代入
        Kpms[Kpms.Length-1] = newKpm;
        // 平均を計算
        double average = 0;
        int kpmCount = 0;
        for (int i = 0; i < Kpms.Length; i++)
        {
            if (Kpms[i] != 0)
            {
                average += Kpms[i];
                kpmCount++;
            }
        }
        if (kpmCount == 0)
        {
            average = 0;
        }
        else
        {
            average /= kpmCount;
        }

        Status[st.Kpm] = (int)Math.Round(average); // 四捨五入してintにキャスト;
    }
    public int getTotalMedal()
    {
        int total = 0; // 合計値を保持する変数
        foreach (int medalCount in Medals) // Medals配列の各要素に対してループ
        {
            if (medalCount == 5)
            {
                total += 1;
            }
            else
            {
                total += medalCount; // 合計に加算
            }
        }
        return total; // 計算された合計値を返す
    }

    // テスト用メソッド
    public void TestMigration()
    {
        Debug.Log("=== 移行テスト開始 ===");
        
        // 1. 既存データを設定
        Email = "test@example.com";
        UserName = "TestUser";
        Status[st.Gold] = 1000;
        Equipment[eq.RightHand] = 5;
        Inventory[0] = 10;
        Items[0] = true;
        Medals[0] = 3;
        Kpms[0] = 150;
        Settings[se.Volume] = 50;
        
        Debug.Log($"既存データ設定完了: Email={Email}, Gold={Status[st.Gold]}");
        
        // 2. 新しい構造に移行
        MigrateToNewStructure();
        
        Debug.Log($"移行完了: Email={PlayerData.Email}, Gold={PlayerData.Gold}");
        
        // 3. 新しい構造から復元
        RestoreFromNewStructure();
        
        Debug.Log($"復元完了: Email={Email}, Gold={Status[st.Gold]}");
        
        // 4. データの整合性チェック
        bool isConsistent = Email == PlayerData.Email && 
                           Status[st.Gold] == PlayerData.Gold &&
                           Equipment[eq.RightHand] == PlayerData.RightHand;
        
        Debug.Log($"データ整合性チェック: {(isConsistent ? "OK" : "NG")}");
        
        Debug.Log("=== 移行テスト終了 ===");
    }

    // タイピング結果テスト
    public void TestTypingResult()
    {
        Debug.Log("=== タイピング結果テスト開始 ===");
        
        // 1. タイピング結果を追加
        UpdateTypingResult(1, 150, 95);
        UpdateTypingResult(1, 160, 90);
        UpdateTypingResult(2, 140, 85);
        
        Debug.Log($"お題1の結果: Count={PlayerData.TypingResults[1].Count}, " +
                  $"平均KPM={PlayerData.TypingResults[1].TotalKpmSum / PlayerData.TypingResults[1].Count}, " +
                  $"平均正解率={PlayerData.TypingResults[1].TotalAccuracySum / PlayerData.TypingResults[1].Count}");
        
        Debug.Log($"お題2の結果: Count={PlayerData.TypingResults[2].Count}, " +
                  $"平均KPM={PlayerData.TypingResults[2].TotalKpmSum / PlayerData.TypingResults[2].Count}, " +
                  $"平均正解率={PlayerData.TypingResults[2].TotalAccuracySum / PlayerData.TypingResults[2].Count}");
        
        Debug.Log("=== タイピング結果テスト終了 ===");
    }

    // Firebaseシリアライズテスト
    public void TestFirebaseSerialization()
    {
        Debug.Log("=== Firebaseシリアライズテスト開始 ===");
        
        // 1. データを設定
        PlayerData.Email = "test@example.com";
        PlayerData.UserName = "TestUser";
        PlayerData.Gold = 1000;
        PlayerData.RightHand = 5;
        PlayerData.Inventory[0] = 10;
        PlayerData.Items[0] = true;
        PlayerData.Medals[0] = 3;
        PlayerData.Kpms[0] = 150;
        PlayerData.Settings[se.Volume] = 50;
        
        // 2. シリアライズ
        string json = SerializeForFirebase();
        Debug.Log($"シリアライズ結果: {json}");
        
        // 3. デシリアライズ
        PlayerData newPlayerData = new PlayerData();
        var tempPlayerData = PlayerData;
        PlayerData = newPlayerData;
        DeserializeFromFirebase(json);
        
        Debug.Log($"デシリアライズ結果: Email={PlayerData.Email}, Gold={PlayerData.Gold}");
        
        // 4. データの整合性チェック
        bool isConsistent = tempPlayerData.Email == PlayerData.Email && 
                           tempPlayerData.Gold == PlayerData.Gold &&
                           tempPlayerData.RightHand == PlayerData.RightHand;
        
        Debug.Log($"データ整合性チェック: {(isConsistent ? "OK" : "NG")}");
        
        Debug.Log("=== Firebaseシリアライズテスト終了 ===");
    }
}