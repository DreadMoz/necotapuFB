using System;
using System.IO;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class FileListData
{
    public List<string> fileNames;  // JSONファイル内のファイル名のリスト
}

public class Challenge : MonoBehaviour
{
    [SerializeField]
    private GameManager gm;
    [SerializeField]
    private GameObject odaiPanelPrefab; // チャレンジボタンのプレハブ
    // ここで、ShopItemParentのRectTransformを参照する
    [SerializeField]
    private RectTransform menuParent;
    [SerializeField]
    private string odaiDataPath = "TextChallange/"; 

    private bool goNextScene = false;    // 次のシーンに遷移するためのフラグ

    // Start is called before the first frame update
    void Start()
    {
        LoadChallenges();
    }

    void LoadChallenges()
    {
        // JSON形式でファイル名をリスト化したメタファイルをロードする
        TextAsset fileList = Resources.Load<TextAsset>(odaiDataPath+"0fileList");
        FileListData fileListData = JsonUtility.FromJson<FileListData>(fileList.text);

        foreach (var fileName in fileListData.fileNames) {
            TextAsset jsonText = Resources.Load<TextAsset>(odaiDataPath+fileName);
            ChallengeData data = JsonUtility.FromJson<ChallengeData>(jsonText.text);
            CreateChallengeButton(fileName, data);
        }
        // parentObjectは、子オブジェクトの数を数えたいゲームオブジェクトの参照。
        double childLines = Math.Ceiling((double)menuParent.transform.childCount / 3);
        // コンテンツエリアの高さをアイテム数に基づいて設定
        float contentHeight = (int)childLines * 352; // アイテムの高さ
        menuParent.sizeDelta = new Vector2(menuParent.sizeDelta.x, contentHeight);
    }
    
    void CreateChallengeButton(string fileName, ChallengeData data)
    {
        GameObject odaiPanel = Instantiate(odaiPanelPrefab, menuParent);
        odaiPanel.name = data.title; // ボタンにタイトルを設定
        odaiPanel.GetComponentInChildren<Text>().text = data.title; // テキストコンポーネントにタイトルを設定
        odaiPanel.GetComponent<Button>().onClick.AddListener(() => bootTyping(fileName)); // ボタンにクリックイベントを追加
    }

    void StartChallenge(ChallengeData data)
    {
        // ここにチャレンジを開始するコードを追加
        Debug.Log("Challenge started: " + data.title);
    }
    public void bootTyping(string title)
    {
        GameManager.TypingDataPath = odaiDataPath + title;

        GameManager.SceneNo = (int)scene.Typing;
        if (!goNextScene)
        {
            SceneManager.LoadScene("typingStage"); // タイピングシーンに遷移
            goNextScene = true;
        }
    }
}

[System.Serializable]
public class ChallengeData
{
    public string title;
    public string[] questions;
}