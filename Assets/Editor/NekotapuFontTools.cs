using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 旧UI（UnityEngine.UI.Text）のフォントを BIZUDPGothic の TTF に統一するエディタ用ツール。
/// ビルドには含まれない（Editor フォルダ）。シーン・プレハブのファイルは直接書き換えず、
/// Unity の API で変更する。シーンは保存しない。
/// </summary>
public static class NekotapuFontTools
{
    private const string BizFontPath = "Assets/Fonts/BIZUDPGothic-Regular.ttf";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string MenuRoot = "ねこたぷ/フォント/";

    private class Target
    {
        public Text text;
        public string place;   // 「シーン：xxx」または「プレハブ：xxx」
        public string path;    // ヒエラルキー上のパス
    }

    [MenuItem(MenuRoot + "旧Textのフォント一覧（確認のみ）")]
    private static void ListFonts()
    {
        Font biz = LoadBizFont();
        List<Target> targets = CollectTargets();

        var counts = new SortedDictionary<string, int>();
        var sb = new StringBuilder();
        sb.AppendLine($"旧Textのフォント一覧（確認のみ。何も変更していません）：{targets.Count} 個");
        foreach (Target t in targets)
        {
            string fontName = FontName(t.text.font);
            counts[fontName] = counts.ContainsKey(fontName) ? counts[fontName] + 1 : 1;
            string mark = (biz != null && t.text.font == biz) ? "  " : "★";
            sb.AppendLine($"{mark} [{t.place}] {t.path} | {fontName} | 文字サイズ {t.text.fontSize}");
        }
        sb.AppendLine("--- フォント別の個数（★ は BIZUDPGothic 以外）---");
        foreach (KeyValuePair<string, int> kv in counts)
        {
            sb.AppendLine($"{kv.Key}: {kv.Value} 個");
        }
        Debug.Log(sb.ToString());
    }

    [MenuItem(MenuRoot + "旧Textのフォントを BIZUDPGothic に統一")]
    private static void UnifyFonts()
    {
        Font biz = LoadBizFont();
        if (biz == null)
        {
            EditorUtility.DisplayDialog("ねこたぷ フォント", $"BIZUDPGothic の TTF が見つかりません。\n{BizFontPath}", "OK");
            return;
        }

        List<Target> changeList = new List<Target>();
        foreach (Target t in CollectTargets())
        {
            if (t.text.font != biz)
            {
                changeList.Add(t);
            }
        }

        if (changeList.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ フォント", "BIZUDPGothic 以外を使っている旧Textはありません。", "OK");
            return;
        }

        bool ok = EditorUtility.DisplayDialog(
            "ねこたぷ フォント",
            $"{changeList.Count} 個の旧Textのフォントを BIZUDPGothic に変えます。\n" +
            "文字の大きさ・色・配置などは変えません。\n" +
            "保存はしません。見た目を確認してから、自分で保存してください。\n" +
            "（Ctrl+Z / Cmd+Z で元に戻せます）",
            "変える", "やめる");
        if (!ok)
        {
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("旧Textのフォントを BIZUDPGothic に統一");
        int undoGroup = Undo.GetCurrentGroup();

        var sb = new StringBuilder();
        sb.AppendLine($"旧Textのフォントを BIZUDPGothic に変更しました：{changeList.Count} 個（まだ保存していません）");
        foreach (Target t in changeList)
        {
            string before = FontName(t.text.font);
            Undo.RecordObject(t.text, "旧Textのフォントを BIZUDPGothic に統一");
            t.text.font = biz;
            EditorUtility.SetDirty(t.text);
            if (PrefabUtility.IsPartOfPrefabInstance(t.text))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(t.text);
            }
            if (t.text.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(t.text.gameObject.scene);
            }
            sb.AppendLine($"  [{t.place}] {t.path} | {before} → {FontName(biz)}");
        }
        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log(sb.ToString());
    }

    private static Font LoadBizFont()
    {
        return AssetDatabase.LoadAssetAtPath<Font>(BizFontPath);
    }

    private static string FontName(Font font)
    {
        return font == null ? "（未設定）" : font.name;
    }

    // 今開いているシーンと、Assets/Prefabs 以下のプレハブから旧Textを集める。
    private static List<Target> CollectTargets()
    {
        var list = new List<Target>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded)
            {
                continue;
            }
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Text text in root.GetComponentsInChildren<Text>(true))
                {
                    list.Add(new Target { text = text, place = "シーン：" + scene.name, path = HierarchyPath(text.transform) });
                }
            }
        }

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                continue;
            }
            foreach (Text text in prefab.GetComponentsInChildren<Text>(true))
            {
                list.Add(new Target { text = text, place = "プレハブ：" + assetPath, path = HierarchyPath(text.transform) });
            }
        }
        return list;
    }

    private static string HierarchyPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }
        return path;
    }
}
