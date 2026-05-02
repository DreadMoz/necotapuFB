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
// RightHand,Head(151),Glasses(121),LeftHand,CatBody(201)あえて0,CatFace(101),NickName(211),BackpackType(25=亀),予備
public class eq
{
    public const int RightHand = 0;
    public const int Head = 1;
    public const int Glasses = 2;
    public const int LeftHand = 3;
    public const int CatBody = 4;
    public const int CatFace = 5;
    public const int NicknameNo = 6;
    public const int BackpackType = 7;  // 25=亀の甲羅, 0=リュック（NPC表示用）
    public const int Spare8 = 8;
    public const int Spare9 = 9;
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
    public const int YubiCnt = 7;
    public const int YubiDate = 8;
    public const int dummy9 = 9;
    public const int dummy10 = 10;
    public const int dummy11 = 11;
    public const int dummy12 = 12;
    public const int dummy13 = 13;
    public const int dummy14 = 14;
    public const int dummy15 = 15;
    public const int dummy16 = 16;
    public const int dummy17 = 17;
    public const int dummy18 = 18;
    public const int dummy19 = 19;
}

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
    public int kpms;
    public int[] medals;
    public int[] items;
}

// 拡張機能ランキング
[Serializable]
public class ExRank
{
    public string Uid { get; set; }
    public int Ranking { get; set; }
    public string FirstName { get; set; }
    public int RightHand { get; set; }
    public int Glasses { get; set; }
    public int Head { get; set; }
    public int LeftHand { get; set; }
    public int CatBody { get; set; }
    public int CatFace { get; set; }
    public int NicknameNo { get; set; }
    public int BackpackType { get; set; }  // 25=亀の甲羅, 0=リュック（NPC表示用）
    public int Kpm { get; set; }
    public int Stage { get; set; } // ここにStageプロパティを追加しますにゃん！
}



[System.Serializable]
public class SerializableRankingData
{
    public string[][] rankingData;
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
    
    // 既存の配列構造をそのまま使用
    
    // ExRankのリストを作成
    public List<ExRank> ExRankings = new List<ExRank>();
    

    
    /// <summary>
    /// 空のデータで初期化
    /// </summary>
    public void InitializeEmptyData()
    {
        Debug.Log("SaveData: 空のデータで初期化開始");
        
        // 文字列フィールドをクリア
        UserName = "";
        Email = "";
        AuthMethod = "";
        LastName = "";
        
        // 配列を0で初期化
        for (int i = 0; i < Status.Length; i++) Status[i] = 0;
        for (int i = 0; i < Equipment.Length; i++) Equipment[i] = 0;
        for (int i = 0; i < Inventory.Length; i++) Inventory[i] = 0;
        for (int i = 0; i < Items.Length; i++) Items[i] = false;
        for (int i = 0; i < Medals.Length; i++) Medals[i] = 0;
        for (int i = 0; i < Kpms.Length; i++) Kpms[i] = 0;
        for (int i = 0; i < Settings.Length; i++) Settings[i] = 0;
        
        // リストをクリア
        ExRankings.Clear();
        
        Debug.Log("SaveData: 空のデータで初期化完了");
    }

    [SerializeField]
    public string UserName;

    [SerializeField]
    public string Email;

    [SerializeField]
    public string Uid;

    [SerializeField]
    public string AuthMethod;

    [SerializeField]
    public string LastName;

    [SerializeField]
    public int[] Status = new int[4];

    [SerializeField]
    public int[] Equipment = new int[10];

    /// <summary>自動の空きスロット検索・補正投入から除外（UI上リュック/大亀など専用スロット向け）。</summary>
    public const int InventoryAutoFillSkipIndex = 7;

    [SerializeField]
    public int[] Inventory = new int[60];

    [SerializeField]
    public bool[] Items = new bool [256];

    [SerializeField]
    public int[] Medals = new int[100];

    [SerializeField]
    public int[] Kpms = new int[8];

    [SerializeField]
            public int[] Settings = new int[20];

    /// <summary>
    /// JSON文字列からデータを設定します。
    /// </summary>
    /// <param name="json">設定するデータのJSON文字列。</param>
    public void SetData(string json)
    {
        Debug.Log("SaveData: JSONデータを受信し、設定します。");
        JsonUtility.FromJsonOverwrite(json, this);
    }

    // 拡張機能からランキング一覧を取得する。
    public void setRankingFromFirebaseOrLocal(List<ExRank> newRankings)
    {
        Debug.Log("Received Ranking List. Updating ExRankings.");

        ExRankings.Clear();
        if (newRankings != null)
        {
            ExRankings.AddRange(newRankings);
            Debug.Log($"ExRankings updated with {newRankings.Count} items.");
        }
        else
        {
            Debug.LogWarning("Received null ranking list. ExRankings cleared.");
        }
    }

    public void updateLastName(string newLastName)
    {
        LastName = newLastName;
    }

    /// <summary>
    /// Firebase用の初期データ登録（古いデータ構造は使用しない）
    /// </summary>
    public void setNewDataFB(string googleMail, string googleFirstName, string googleLastName, string authType, int catBody)
    {
        Debug.Log("setNewDataFB: " + googleMail + googleFirstName + googleLastName + authType);

        // 既存の配列構造に直接初期値を設定
        
        necotapuFB.AuthManager authManager = FindObjectOfType<necotapuFB.AuthManager>();
        if (authManager != null && authManager.CurrentAuthInfo != null)
        {
            Uid = authManager.CurrentAuthInfo.userId;
            Debug.Log($"setNewDataFB: UIDをSaveDataに設定: {authManager.CurrentAuthInfo.userId}");
        }
        else
        {
            Debug.LogError("setNewDataFB: AuthManagerまたはAuthInfoが見つからないか、UIDが取得できません。");
        }
        Email = googleMail;
        UserName = googleFirstName;
        AuthMethod = authType;
        LastName = googleLastName;
        
        // Status配列に設定
        Status[st.Gold] = 100;      // ゴールド
        Status[st.Server] = 0;      // ステージ
        Status[st.Rank] = 0;        // ランキング
        Status[st.Kpm] = 10;        // KPM
        
        // Equipment配列に設定
        Equipment[eq.RightHand] = 1;    // 右手にフォーク
        Equipment[eq.Head] = 0;         // ヘッド
        Equipment[eq.Glasses] = 0;      // メガネ
        Equipment[eq.LeftHand] = 0;     // 左手
        Equipment[eq.CatBody] = catBody; // ねこボディ
        Equipment[eq.CatFace] = 0;      // ねこ顔
        Equipment[eq.NicknameNo] = 0;     // ニックネーム
        Equipment[eq.BackpackType] = 0;   // リュック
        Equipment[eq.Spare8] = 0;
        Equipment[eq.Spare9] = 0;
        
        // インベントリ・アイテム
        for (int i = 0; i < Inventory.Length; i++)
        {
            Inventory[i] = 0;
        }
        for (int i = 0; i < Items.Length; i++)
        {
            Items[i] = false;
        }
        Items[1] = true;     // フォークを所持
        
        // メダル・KPM履歴
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
        
        // 設定
        Settings[se.GachaCnt] = 4;    // ガチャ回数
        Settings[se.Volume] = 20;     // 音量
        Settings[se.CatNum] = 10;     // ねこ数
        Settings[se.MailChar] = 1;    // メール文字
        Settings[se.Mute] = 0;        // ミュート
        
        // 最終ログイン日を設定
        DateTime today = DateTime.Now;
        Settings[se.LastLogin] = today.Year * 10000 + today.Month * 100 + today.Day;  // 最終ログイン
        Settings[se.Capital] = 0;     // 大文字
        Settings[se.YubiCnt] = 0;      // 未使用
        Settings[se.YubiDate] = 0;      // 未使用
        Settings[se.dummy9] = 0;      // 未使用
        Settings[se.dummy10] = 0;     // 未使用
        Settings[se.dummy11] = 0;     // 未使用
        Settings[se.dummy12] = 0;     // 未使用
        Settings[se.dummy13] = 0;     // 未使用
        Settings[se.dummy14] = 0;     // 未使用
        Settings[se.dummy15] = 0;     // 未使用
        Settings[se.dummy16] = 0;     // 未使用
        Settings[se.dummy17] = 0;     // 未使用
        Settings[se.dummy18] = 0;     // 未使用
        Settings[se.dummy19] = 0;     // 未使用
        
        Debug.Log($"setNewDataFB: 既存配列にデータを設定完了 - CatBody: {catBody}, RightHand: 1");
    }

    // SaveDataの構造をそのままFirebaseに保存するためのシリアライズ
    public string SerializeForFB()
    {
        return JsonUtility.ToJson(this);
    }

    /// <summary>
    /// 初期データをFirebaseに保存
    /// </summary>
    public void saveInitialDataToFirebase()
    {
        Debug.Log("SaveData: Firebaseに初期データを保存開始");
        
        try
        {
            // 既存のSerializeForFBを使用してJSONに変換
            string firebaseJson = SerializeForFB();
            Debug.Log($"Firebase用JSON: {firebaseJson}");
            
            // FirestoreConnectionを使用してFirebaseに保存
            var firestoreConnection = FindObjectOfType<FirestoreConnection>();
            if (firestoreConnection != null)
            {
                firestoreConnection.SaveInitialData(firebaseJson);
                Debug.Log("✅ Firebaseにデータを保存完了");
            }
            else
            {
                Debug.LogWarning("FirestoreConnectionが見つかりません - Firebase保存をスキップ");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase保存中にエラーが発生: {e.Message}");
        }
    }

    // SaveDataの構造をそのままFirebaseまたはローカルから読み込むためのデシリアライズ
    public void DeserializeFromFirebaseOrLocal(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
        if (Equipment != null && Equipment.Length < 10)
        {
            int[] expanded = new int[10];
            for (int i = 0; i < Equipment.Length; i++) expanded[i] = Equipment[i];
            for (int i = Equipment.Length; i < 10; i++) expanded[i] = 0;
            Equipment = expanded;
        }
    }

    public int getBlankInventoryIndex()
    {
        for (int i = 0; i < Inventory.Length; i++)
        {
            if (i == InventoryAutoFillSkipIndex)
            {
                continue;
            }

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