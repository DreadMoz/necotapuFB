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

// GASステータス
[System.Serializable]
public class SerializableSympleStatusData
{
    public string email;
    public string ou;
    public string lastName;
    public int gold;
    public int stage;
    public int ranking;
    public string name;
    public int rightHand;
    public int glasses;
    public int head;
    public int leftHand;
    public int catBody;
    public int catFace;
    public int nickName;
    public int kpm;
    public string kpms;
    public string[] medals;
    public string[] items;
}

// 拡張機能ステータス
[Serializable]
public class SerializableStatusData
{
    public string Email;
    public string Ou;
    public string LastName;
    public int Gold;
    public int Stage;
    public int Ranking;
    public string Name;
    public int RightHand;
    public int Glasses;
    public int Head;
    public int LeftHand;
    public int CatBody;
    public int CatFace;
    public int NickName;
    public int Kpm;
    public int[] Inventory;
    public string[] Items;
    public string[] Medals;
    public string Kpms;
    public int[] Settings;
    // 必要に応じて他のフィールドも追加
}

// Gemini送信用データ
[Serializable]
public class SerializableGemini
{
    public string FirstName;
    public int Gold;
    public string Stage;
    public int Ranking;
    public string typingTitle;
    public int maxCombo;
    public int resultKpm;
    public int averageKpm;
    public List<string> mistypedSentences;
    public SerializableGemini()
    {
        mistypedSentences = new List<string>(); // リストの初期化
    }
}

[CreateAssetMenu(fileName = "SaveData", menuName = "SaveData")]
public class SaveData : ScriptableObject
{
    System.Random random = new System.Random(); // Random オブジェクトのインスタンスを作成
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
    }

    // 拡張機能からステータスデータを取得する。
    public void setStatusFromLocal(string statusData)
     {
        Debug.Log("Received Status JSON: " + statusData);
        // JSONデータのトップレベルを取得
        var wrappedData = JsonConvert.DeserializeObject<Dictionary<string, SerializableStatusData>>(statusData);
        
        if (wrappedData.TryGetValue("statusData", out SerializableStatusData exData))
        {
            // ApiStatus に値を設定
            Email = exData.Email;
            Ou = exData.Ou;
            LastName = exData.LastName;
            Status[st.Gold] = exData.Gold;

            // ExRank に値を設定
            Status[st.Server] = exData.Stage;
            Status[st.Rank] = exData.Ranking;
            UserName = exData.Name;
            Equipment[eq.RightHand] = exData.RightHand;
            Equipment[eq.Glasses] = exData.Glasses;
            Equipment[eq.Head] = exData.Head;
            Equipment[eq.LeftHand] = exData.LeftHand;
            Equipment[eq.CatBody] = exData.CatBody;
            Equipment[eq.CatFace] = exData.CatFace;
            Equipment[eq.NickName] = exData.NickName;
            Status[st.Kpm] = exData.Kpm;

            // ここは配列40のコピー
            for (int i = 0; i < Inventory.Length; i++)
            {
                Inventory[i] = exData.Inventory[i];
            }

            // ここはlong[4]をbool[100]に変換
            DecodeItemData(exData.Items);

            // ここはlong[5]をint[100]に変換
            DecodeMedalData(exData.Medals);

            // ここは配列8<-文字列
            DecodeKpmData(exData.Kpms);

            // ここは配列10のコピー
            for (int i = 0; i < Settings.Length; i++)
            {
                Settings[i] = exData.Settings[i];
            }
        }
        else
        {
            Debug.LogError("Failed to deserialize statusData.");
        }
    // testEncodeMedals();      // デバッグで使用した。Medalエンコード->デコードテスト
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
            DecodeKpmData(list[15].ToString());

            string[] gssMedals = new string[5];
            gssMedals[0] = list[16].ToString();
            gssMedals[1] = list[17].ToString();
            gssMedals[2] = list[18].ToString();
            gssMedals[3] = list[19].ToString();
            gssMedals[4] = list[20].ToString();

            // ここはlong[5]をint[100]に変換
            DecodeMedalData(gssMedals);

            string[] gssItems = new string[4];
            gssItems[0] = list[21].ToString();
            gssItems[1] = list[22].ToString();
            gssItems[2] = list[23].ToString();
            gssItems[3] = list[24].ToString();

            // ここはlong[4]をbool[100]に変換
            DecodeItemData(gssItems);

            setInventoryFromItems();
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

    public void DecodeItemData(string[] itemData)
    {
        // 各 long 値をビット単位で調べる
        for (int i = 0; i < itemData.Length; i++)
        {
            ulong currentItemData = ulong.Parse(itemData[i]);
            for (int bit = 0; bit < 64; bit++)
            {
                // currentItemData から特定のビット位置の値を取得
                bool isItemPresent = (currentItemData & (1UL << bit)) != 0;
                // 計算したビット位置に応じた items 配列の位置に値をセット
                Items[i * 64 + bit] = isItemPresent;
            }
        }
    }

    public void DecodeMedalData(string[] medalCode)
    {
        ulong mask = 0b111; // 3ビットを取り出すためのマスク

        for (int i = 0; i < medalCode.Length; i++)
        {
            ulong medal = ulong.Parse(medalCode[i]);
            for (int j = 0; j < 20; j++)
            {
                // encodedValues[i]から3ビットずつ切り出して、配列に格納
                // 最下位ビットから開始するため、シフトするビット数を調整
                Medals[i * 20 + j] = (int)((medal >> (j * 3)) & mask);
            }
        }
    }

    public void DecodeKpmData(string rkpm)
    {
        int arrayIndex = 7;

        // 文字列の末尾から3文字ずつ取得していく
        for (int i = rkpm.Length; i > 0; i -= 3)
        {
            // 3文字の部分文字列を取得
            string part = rkpm.Substring(Math.Max(i - 3, 0), i - Math.Max(i - 3, 0));
            Kpms[arrayIndex] = int.Parse(part);
            arrayIndex--;
        }
    }

    // 拡張機能に保存するためのデータを現在のゲームデータから作る。
    public string CompileGameDataForLocal(SaveData sd)
    {
        SerializableStatusData data = new SerializableStatusData
        {
            Email = sd.Email,
            Ou = sd.Ou,
            LastName = sd.LastName,
            Gold = sd.Status[st.Gold],

            Stage = sd.Status[st.Server],
            Ranking = sd.Status[st.Rank],
            Name = sd.UserName,
            RightHand = sd.Equipment[eq.RightHand],
            Glasses = sd.Equipment[eq.Glasses],
            Head = sd.Equipment[eq.Head],
            LeftHand = sd.Equipment[eq.LeftHand],
            CatBody = sd.Equipment[eq.CatBody],
            CatFace = sd.Equipment[eq.CatFace],
            NickName = sd.Equipment[eq.NickName],
            Kpm = sd.Status[st.Kpm],

            Inventory = sd.Inventory,
            Items = EncodeItemData(sd.Items),
            Medals = EncodeMedalData(sd.Medals),
            Kpms = EncodeKpmData(sd.Kpms),
            Settings = sd.Settings,
        };

        // statusDataプロパティを持つ新しいオブジェクトを作成し、JSONにシリアライズ
        var wrappedData = new { statusData = data };
        Debug.Log("wrappedData(SaveData): " + JsonConvert.SerializeObject(wrappedData));    // ログ出力を追加
        return JsonConvert.SerializeObject(wrappedData);
    }

    // GSSに保存するためのデータを現在のゲームデータから作る。
    public string CompileGameDataForGss(SaveData sd)
    {
        SerializableStatusData data = new SerializableStatusData
        {
            Email = sd.Email,
            Ou = sd.Ou,
            LastName = sd.LastName,
            Gold = sd.Status[st.Gold],

            Stage = sd.Status[st.Server],
            Ranking = sd.Status[st.Rank],
            Name = sd.UserName,
            RightHand = sd.Equipment[eq.RightHand],
            Glasses = sd.Equipment[eq.Glasses],
            Head = sd.Equipment[eq.Head],
            LeftHand = sd.Equipment[eq.LeftHand],
            CatBody = sd.Equipment[eq.CatBody],
            CatFace = sd.Equipment[eq.CatFace],
            NickName = sd.Equipment[eq.NickName],
            Kpm = sd.Status[st.Kpm],

            Items = EncodeItemData(sd.Items),
            Medals = EncodeMedalData(sd.Medals),
            Kpms = EncodeKpmData(sd.Kpms),
        };

        // statusDataプロパティを持つ新しいオブジェクトを作成し、JSONにシリアライズ
        Debug.Log("wrappedData(GssSaveData): " + JsonConvert.SerializeObject(data));    // ログ出力を追加
        return JsonConvert.SerializeObject(data);
    }

    // Geminiに送るためのデータを現在のゲームデータから作る。
    public string CompileGeminiData(SaveData sd, string server)
    {
        SerializableGemini data = new SerializableGemini
        {
            FirstName = sd.UserName,
            Gold = sd.Status[st.Gold],
            Stage = server,
            Ranking = sd.Status[st.Rank],
            typingTitle = GameManager.TypingTitle,
            maxCombo = GameManager.MaxCombo,
            resultKpm = GameManager.NewKpm,
            averageKpm = sd.Status[st.Kpm],
            mistypedSentences = GameManager.MistypedSentences,
        };

        // statusDataプロパティを持つ新しいオブジェクトを作成し、JSONにシリアライズ
        Debug.Log("wrappedData(GeminiData): " + JsonConvert.SerializeObject(data));    // ログ出力を追加
        return JsonConvert.SerializeObject(data);
    }

    public string[] EncodeItemData(bool[] items)
    {
        ulong[] encodedItems = new ulong[4];
        string[] returnItems = new string[4];
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i])
            {
                int itemIndex = i / 64;
                int bitPosition = i % 64;
                encodedItems[itemIndex] |= (1UL << bitPosition);
            }
        }
        for(int i = 0; i < encodedItems.Length; i++)
        {
            returnItems[i] = encodedItems[i].ToString();
        }
        return returnItems;
    }
    public void testEncodeMedals()
    {
        int[] test1 = new int[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1};
        int[] test2 = new int[] {2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2};
        int[] test3 = new int[] {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3};
        int[] test4 = new int[] {4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4};
        int[] test5 = new int[] {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5};
        int[] test0 = new int[] {4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        string[] returnString = EncodeMedalData(test4);
        DecodeMedalData(returnString);
    }
    public string[] EncodeMedalData(int[] medals)
    {
        ulong[] encodedMedals = new ulong[5];
        string[] returnMedals = new string[5];
        for (int i = 0; i < medals.Length; i++)
        {
            int medalIndex = i / 20;
            int bitPosition = (i % 20) * 3;
            encodedMedals[medalIndex] |= ((ulong)medals[i] << bitPosition);
        }
        for(int i = 0; i < encodedMedals.Length; i++)
        {
            returnMedals[i] = encodedMedals[i].ToString();
        }
        return returnMedals;
    }
    public string EncodeKpmData(int[] kpms)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = kpms.Length - 1; i >= 0; i--)
        {
            // 最初の要素以外は3桁になるように0でパディング
            if (i == 0 && kpms[i] <= 999)
            {
                sb.Insert(0, kpms[i].ToString());
            }
            else
            {
                sb.Insert(0, kpms[i].ToString("D3"));
            }
        }
        return sb.ToString();
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
}