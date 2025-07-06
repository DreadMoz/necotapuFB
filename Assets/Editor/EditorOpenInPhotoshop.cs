using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

public class EditorOpenInPhotoshop
{
    const string ItemName = "Assets/Open in Photoshop";

    [MenuItem(ItemName, false)]
    static void Open()
    {
        foreach (var guid in Selection.assetGUIDs)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid); // 選択中のアセットのパス
            var fullPath = Path.GetFullPath(path); // アセットの絶対パス
            Process.Start("/Applications/Adobe Photoshop 2024/Adobe Photoshop 2024.app", fullPath); // Photoshopでアセットを開く
        }
    }
}
