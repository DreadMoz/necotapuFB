using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 朱雀門（SuzakuGate）の屋根（SM_Roof_02 / SM_Roof_03）を軽くするエディタ用ツール（ビルドには含まれない）。
/// ・元の FBX は変更しない。形を簡単にしたメッシュを Assets/LightMeshes/ に新しいファイルとして作る。
/// ・WorldScene の屋根のメッシュを、作ったメッシュに差し替える（Undo できる。シーンは保存しない）。
/// ・形を簡単にする処理は、UnityMeshSimplifier（Whinarn 作、MIT ライセンス）を使う。
///   このツールはリフレクションで呼び出すので、パッケージが未導入でもコンパイルできる。
///   パッケージが無いときは、メニューを押すと導入の案内を出す。
/// </summary>
public static class NekotapuRoofTool
{
    private const string MenuRoot = "ねこたぷ/軽量化/屋根を軽くする/";
    private const string OutDir = "Assets/LightMeshes";
    private const string PrefKey = "Nekotapu.RoofLite.LastPercent";
    private const string BorderKey = "Nekotapu.RoofLite.PreserveBorder";
    private const string SeamKey = "Nekotapu.RoofLite.PreserveSeams";
    private const string SmartLinkKey = "Nekotapu.RoofLite.SmartLink";
    private const string LinkDistanceKey = "Nekotapu.RoofLite.LinkDistance";
    private const string BorderMenu = MenuRoot + "設定：縁を守る";
    private const string SeamMenu = MenuRoot + "設定：継ぎ目を守る";
    private const string SmartLinkMenu = MenuRoot + "設定：近くの頂点をつなぐ（SmartLink）";
    private const string DistanceMenuPrefix = MenuRoot + "設定：つなぐ距離 ";
    // 初期値（おすすめ）：縁は守る、模様の継ぎ目は守らない、近くの頂点をつなぐ（距離 0.001）
    private const string TileOnlyKey = "Nekotapu.RoofLite.TileOnly";
    private const string TileOnlyMenu = MenuRoot + "設定：削る部品を瓦だけにする";
    // 瓦の部品の見分け方：三角形がこの数を超える部品を「瓦」とする（屋根 2 つとも、瓦の部品だけがこれに当たる。木材の最大は 25,920）。
    private const long TileMinTriangles = 100000;
    private const int DefaultPercent = 5;
    private const bool DefaultTileOnly = true;
    private const bool DefaultBorder = true;
    private const bool DefaultSeam = false;
    private const bool DefaultSmartLink = true;
    private const float DefaultLinkDistance = 0.001f;
    private const string LiteMark = "__lite";
    private const string SimplifierType = "UnityMeshSimplifier.MeshSimplifier, Whinarn.UnityMeshSimplifier.Runtime";
    private const long WarnBytes = 8L * 1024 * 1024;

    // 軽くする対象の元の FBX（朱雀門の屋根）
    private static readonly string[] RoofFbx =
    {
        "Assets/AssetsOld/JejumokGwana/Meshes/Roofs/SM_Roof_02.fbx",
        "Assets/AssetsOld/JejumokGwana/Meshes/Roofs/SM_Roof_03.fbx",
    };

    [MenuItem(MenuRoot + "1 軽いメッシュを作る（5%・おすすめ）")]
    private static void Make5() { Make(5); }

    [MenuItem(MenuRoot + "1 軽いメッシュを作る（10%）")]
    private static void Make10() { Make(10); }

    [MenuItem(MenuRoot + "1 軽いメッシュを作る（20%）")]
    private static void Make20() { Make(20); }

    [MenuItem(MenuRoot + "1 軽いメッシュを作る（30%）")]
    private static void Make30() { Make(30); }

    // ---- 軽くするときの設定（メニューで切り替え。作るたびに、この設定が使われる）----
    // 縁を守る：メッシュの外側の輪郭（屋根どうしの境目など）を動かさない。すきまができるのを防ぐ。
    // 継ぎ目を守る：模様（UV）の継ぎ目を動かさない。守ると、このメッシュはほとんど軽くならない。
    // 近くの頂点をつなぐ（SmartLink）：ばらばらになっている、ほぼ同じ位置の頂点を 1 つにつないで、一緒に軽くする。
    // つなぐ距離：この距離より近い頂点をつなぐ（メッシュの座標の単位）。大きいほど、よくつながるが、形が少し動く。

    [MenuItem(BorderMenu)]
    private static void ToggleBorder()
    {
        EditorPrefs.SetBool(BorderKey, !GetBorder());
        Debug.Log("屋根の設定を変えました。\n" + SettingsText());
    }

    [MenuItem(BorderMenu, true)]
    private static bool ToggleBorderValidate() { Menu.SetChecked(BorderMenu, GetBorder()); return true; }

    [MenuItem(SeamMenu)]
    private static void ToggleSeam()
    {
        EditorPrefs.SetBool(SeamKey, !GetSeam());
        Debug.Log("屋根の設定を変えました。\n" + SettingsText());
    }

    [MenuItem(SeamMenu, true)]
    private static bool ToggleSeamValidate() { Menu.SetChecked(SeamMenu, GetSeam()); return true; }

    [MenuItem(SmartLinkMenu)]
    private static void ToggleSmartLink()
    {
        EditorPrefs.SetBool(SmartLinkKey, !GetSmartLink());
        Debug.Log("屋根の設定を変えました。\n" + SettingsText());
    }

    [MenuItem(SmartLinkMenu, true)]
    private static bool ToggleSmartLinkValidate() { Menu.SetChecked(SmartLinkMenu, GetSmartLink()); return true; }

    // 削る部品：オンなら、瓦の部品だけを削り、木材・壁の部品は元のまま。オフなら、すべての部品を同じ割合で削る。
    [MenuItem(TileOnlyMenu)]
    private static void ToggleTileOnly()
    {
        EditorPrefs.SetBool(TileOnlyKey, !GetTileOnly());
        Debug.Log("屋根の設定を変えました。\n" + SettingsText());
    }

    [MenuItem(TileOnlyMenu, true)]
    private static bool ToggleTileOnlyValidate() { Menu.SetChecked(TileOnlyMenu, GetTileOnly()); return true; }

    [MenuItem(DistanceMenuPrefix + "0（同じ位置だけ）")]
    private static void Distance0() { SetDistance(0f); }
    [MenuItem(DistanceMenuPrefix + "0（同じ位置だけ）", true)]
    private static bool Distance0Validate() { Menu.SetChecked(DistanceMenuPrefix + "0（同じ位置だけ）", Mathf.Approximately(GetLinkDistance(), 0f)); return true; }

    [MenuItem(DistanceMenuPrefix + "0.0001")]
    private static void Distance1() { SetDistance(0.0001f); }
    [MenuItem(DistanceMenuPrefix + "0.0001", true)]
    private static bool Distance1Validate() { Menu.SetChecked(DistanceMenuPrefix + "0.0001", Mathf.Approximately(GetLinkDistance(), 0.0001f)); return true; }

    [MenuItem(DistanceMenuPrefix + "0.001")]
    private static void Distance2() { SetDistance(0.001f); }
    [MenuItem(DistanceMenuPrefix + "0.001", true)]
    private static bool Distance2Validate() { Menu.SetChecked(DistanceMenuPrefix + "0.001", Mathf.Approximately(GetLinkDistance(), 0.001f)); return true; }

    [MenuItem(DistanceMenuPrefix + "0.01")]
    private static void Distance3() { SetDistance(0.01f); }
    [MenuItem(DistanceMenuPrefix + "0.01", true)]
    private static bool Distance3Validate() { Menu.SetChecked(DistanceMenuPrefix + "0.01", Mathf.Approximately(GetLinkDistance(), 0.01f)); return true; }

    [MenuItem(DistanceMenuPrefix + "0.05")]
    private static void Distance4() { SetDistance(0.05f); }
    [MenuItem(DistanceMenuPrefix + "0.05", true)]
    private static bool Distance4Validate() { Menu.SetChecked(DistanceMenuPrefix + "0.05", Mathf.Approximately(GetLinkDistance(), 0.05f)); return true; }

    [MenuItem(MenuRoot + "設定：おすすめに戻す")]
    private static void ResetSettings()
    {
        EditorPrefs.SetBool(TileOnlyKey, DefaultTileOnly);
        EditorPrefs.SetInt(PrefKey, DefaultPercent);
        EditorPrefs.SetBool(BorderKey, DefaultBorder);
        EditorPrefs.SetBool(SeamKey, DefaultSeam);
        EditorPrefs.SetBool(SmartLinkKey, DefaultSmartLink);
        EditorPrefs.SetFloat(LinkDistanceKey, DefaultLinkDistance);
        Debug.Log("屋根の設定を、おすすめに戻しました。\n" + SettingsText());
    }

    private static bool GetTileOnly() { return EditorPrefs.GetBool(TileOnlyKey, DefaultTileOnly); }
    private static bool GetBorder() { return EditorPrefs.GetBool(BorderKey, DefaultBorder); }
    private static bool GetSeam() { return EditorPrefs.GetBool(SeamKey, DefaultSeam); }
    private static bool GetSmartLink() { return EditorPrefs.GetBool(SmartLinkKey, DefaultSmartLink); }
    private static float GetLinkDistance() { return EditorPrefs.GetFloat(LinkDistanceKey, DefaultLinkDistance); }

    private static void SetDistance(float value)
    {
        EditorPrefs.SetFloat(LinkDistanceKey, value);
        Debug.Log("屋根の設定を変えました。\n" + SettingsText());
    }

    private static string SettingsText()
    {
        return "軽くするときの設定：" +
               $"削る部品＝{(GetTileOnly() ? "瓦だけ（三角形が 10 万を超える部品）" : "すべて")}、" +
               $"縁を守る＝{(GetBorder() ? "はい" : "いいえ")}、" +
               $"継ぎ目を守る＝{(GetSeam() ? "はい" : "いいえ")}、" +
               $"近くの頂点をつなぐ（SmartLink）＝{(GetSmartLink() ? "はい" : "いいえ")}、" +
               $"つなぐ距離＝{(GetLinkDistance() <= 0f ? "0（同じ位置だけ）" : GetLinkDistance().ToString("0.#####"))}";
    }

    // 作っておいた 5% と 10% のメッシュを、作り直さずに切り替えて比べるためのメニュー（見た目だけ差し替える）。
    [MenuItem(MenuRoot + "2 差し替える（5% のメッシュ・見た目だけ）")]
    private static void ReplaceWith5() { EditorPrefs.SetInt(PrefKey, 5); Replace(false); }

    [MenuItem(MenuRoot + "2 差し替える（10% のメッシュ・見た目だけ）")]
    private static void ReplaceWith10() { EditorPrefs.SetInt(PrefKey, 10); Replace(false); }

    [MenuItem(MenuRoot + "2 差し替える（見た目だけ）")]
    private static void ReplaceVisual() { Replace(false); }

    [MenuItem(MenuRoot + "2 差し替える（当たり判定も）")]
    private static void ReplaceWithCollider() { Replace(true); }

    [MenuItem(MenuRoot + "3 差し替えを元に戻す")]
    private static void RestoreOriginal() { Restore(); }

    [MenuItem(MenuRoot + "状態の一覧（確認のみ）")]
    private static void ShowStatus()
    {
        var sb = new StringBuilder();
        sb.AppendLine("屋根の状態（確認のみ。何も変更していません）");
        sb.AppendLine(FindSimplifier() != null
            ? "UnityMeshSimplifier：導入済み"
            : "UnityMeshSimplifier：未導入（Window ＞ Package Manager から追加が必要）");
        foreach (string fbx in RoofFbx)
        {
            sb.AppendLine($"元の FBX：{fbx}（{(File.Exists(fbx) ? "あり" : "見つかりません")}）");
        }
        if (AssetDatabase.IsValidFolder(OutDir))
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Mesh", new[] { OutDir }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Mesh m = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                sb.AppendLine($"作ったメッシュ：{path} | 頂点 {m.vertexCount} | ファイル {FileSize(path)}");
            }
        }
        else
        {
            sb.AppendLine($"作ったメッシュ：まだなし（{OutDir} がありません）");
        }
        int original = 0, lite = 0, missing = 0;
        foreach (RoofRef r in FindRoofFilters())
        {
            string where = HierarchyPath(r.filter.transform);
            if (r.missing)
            {
                missing++;
                sb.AppendLine($"  メッシュが見つかりません（Missing）：{where}（「2 差し替える」で直せます）");
            }
            else if (r.isLite)
            {
                lite++;
                sb.AppendLine($"  軽いメッシュを使用中：{where}（頂点 {r.filter.sharedMesh.vertexCount}）");
            }
            else
            {
                original++;
                sb.AppendLine($"  元のメッシュを使用中：{where}（頂点 {r.filter.sharedMesh.vertexCount}）");
            }
        }
        sb.AppendLine($"開いているシーンの屋根：元のメッシュ {original} 個、軽いメッシュ {lite} 個、Missing {missing} 個");
        sb.AppendLine(SettingsText());
        int last = EditorPrefs.GetInt(PrefKey, 0);
        sb.AppendLine(last > 0 ? $"差し替えに使うメッシュ：直前に作った {last}%" : "差し替えに使うメッシュ：まだ作っていません");
        Debug.Log(sb.ToString());
    }

    // ---- 1 軽いメッシュを作る ----

    private static void Make(int percent)
    {
        Type simplifier = FindSimplifier();
        if (simplifier == null)
        {
            ShowNotInstalled();
            return;
        }

        var meshes = new List<KeyValuePair<string, Mesh>>();
        foreach (string fbx in RoofFbx)
        {
            if (!File.Exists(fbx)) { Debug.LogError($"屋根：FBX が見つかりません：{fbx}"); continue; }
            foreach (Mesh m in AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Mesh>())
            {
                meshes.Add(new KeyValuePair<string, Mesh>(fbx, m));
            }
        }
        if (meshes.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 屋根", "元の屋根のメッシュが見つかりません。", "OK");
            return;
        }

        bool ok = EditorUtility.DisplayDialog(
            "ねこたぷ 屋根",
            $"屋根のメッシュ {meshes.Count} 個を、{percent}% の頂点数の軽いメッシュにして、{OutDir}/ に新しく作ります。\n" +
            SettingsText() + "\n" +
            "元の FBX は変更しません。数十秒〜数分かかることがあります。",
            "作る", "やめる");
        if (!ok) { return; }

        EnsureOutDir();
        var sb = new StringBuilder();
        sb.AppendLine($"軽いメッシュを作りました（{percent}%）。" + SettingsText());
        try
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                string fbx = meshes[i].Key;
                Mesh source = meshes[i].Value;
                EditorUtility.DisplayProgressBar("ねこたぷ 屋根", $"{source.name}（{i + 1}/{meshes.Count}）", (float)i / meshes.Count);

                int beforeVertices = source.vertexCount;
                long beforeTriangles = CountTriangles(source);
                long[] beforeParts = SubMeshTriangles(source);

                Mesh lite = BuildLite(simplifier, source, percent, GetTileOnly(), sb);
                if (lite == null)
                {
                    Debug.LogError(sb.ToString());
                    return;
                }

                lite.name = source.name + LiteMark + percent;
                lite.RecalculateBounds();
                MeshUtility.SetMeshCompression(lite, ModelImporterMeshCompression.High);

                string path = $"{OutDir}/{Path.GetFileNameWithoutExtension(fbx)}{LiteMark}{percent}.asset";
                if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null) { AssetDatabase.DeleteAsset(path); }
                AssetDatabase.CreateAsset(lite, path);

                sb.AppendLine($"  {source.name}：頂点 {beforeVertices} → {lite.vertexCount}（{Ratio(lite.vertexCount, beforeVertices)}）、" +
                              $"三角形 {beforeTriangles} → {CountTriangles(lite)}、サブメッシュ {source.subMeshCount} → {lite.subMeshCount}");
                long[] afterParts = SubMeshTriangles(lite);
                for (int part = 0; part < afterParts.Length && part < beforeParts.Length; part++)
                {
                    sb.AppendLine($"    部品 {part}：三角形 {beforeParts[part]} → {afterParts[part]}（{Ratio((int)afterParts[part], (int)beforeParts[part])}）" +
                                  (beforeParts[part] >= TileMinTriangles ? "  ← 瓦" : ""));
                }
                sb.AppendLine($"    → {path}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("屋根：軽いメッシュの作成に失敗しました。\n" + e);
            return;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 保存後のファイルの大きさ（git に入るため確認する）
        foreach (string fbx in RoofFbx)
        {
            string path = $"{OutDir}/{Path.GetFileNameWithoutExtension(fbx)}{LiteMark}{percent}.asset";
            if (!File.Exists(path)) { continue; }
            long size = new FileInfo(path).Length;
            sb.AppendLine($"  ファイルの大きさ：{path} = {FormatBytes(size)}" + (size > WarnBytes ? "（大きい。git に入れる前に確認してください）" : ""));
        }
        EditorPrefs.SetInt(PrefKey, percent);
        sb.AppendLine("次は「2 差し替える」を押してください（WorldScene を開いてから）。");
        Debug.Log(sb.ToString());
    }

    // ---- 2 差し替える ----

    private static void Replace(bool alsoCollider)
    {
        int percent = EditorPrefs.GetInt(PrefKey, DefaultPercent);
        if (percent <= 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 屋根", "軽いメッシュをまだ作っていません。\n先に「1 軽いメッシュを作る」を押してください。", "OK");
            return;
        }

        // 元の FBX のパス → 軽いメッシュ
        var liteByFbx = new Dictionary<string, Mesh>();
        foreach (string fbx in RoofFbx)
        {
            string path = $"{OutDir}/{Path.GetFileNameWithoutExtension(fbx)}{LiteMark}{percent}.asset";
            Mesh lite = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (lite == null)
            {
                EditorUtility.DisplayDialog("ねこたぷ 屋根", $"軽いメッシュが見つかりません：\n{path}\n先に「1 軽いメッシュを作る（{percent}%）」を押してください。", "OK");
                return;
            }
            liteByFbx[fbx] = lite;
        }

        // 元のメッシュを使っているもの、別の割合の軽いメッシュを使っているもの、メッシュが無くなっている（Missing）ものを、すべて対象にする。
        var targets = new List<RoofRef>();
        foreach (RoofRef r in FindRoofFilters())
        {
            if (!r.missing && r.filter.sharedMesh == liteByFbx[r.fbx]) { continue; }   // すでに同じ割合
            targets.Add(r);
        }
        if (targets.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 屋根", "差し替える屋根が見つかりません。\nWorldScene を開いているか、確認してください。\n（すでに差し替え済みかもしれません。「状態の一覧」で確認できます）", "OK");
            return;
        }

        bool ok = EditorUtility.DisplayDialog(
            "ねこたぷ 屋根",
            $"屋根のメッシュ {targets.Count} 個を、{percent}% の軽いメッシュに差し替えます。\n" +
            (alsoCollider ? "当たり判定（MeshCollider）が同じメッシュなら、それも差し替えます。\n" : "当たり判定は変えません。\n") +
            "Cmd+Z で元に戻せます。シーンは保存しません。見た目を確認してから、自分で保存してください。",
            "差し替える", "やめる");
        if (!ok) { return; }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("屋根を軽いメッシュに差し替え");
        int undoGroup = Undo.GetCurrentGroup();

        var sb = new StringBuilder();
        sb.AppendLine($"屋根を軽いメッシュ（{percent}%）に差し替えました（まだ保存していません）：");
        int colliderSkipped = 0;
        foreach (RoofRef r in targets)
        {
            MeshFilter mf = r.filter;
            Mesh before = mf.sharedMesh;   // Missing のときは null
            Mesh lite = liteByFbx[r.fbx];
            Undo.RecordObject(mf, "屋根を軽いメッシュに差し替え");
            mf.sharedMesh = lite;
            MarkChanged(mf);
            string beforeText = before == null ? "Missing" : $"頂点 {before.vertexCount}";
            sb.AppendLine($"  {HierarchyPath(mf.transform)} | {beforeText} → 頂点 {lite.vertexCount}");

            foreach (MeshCollider mc in mf.GetComponents<MeshCollider>())
            {
                if (mc.sharedMesh != before) { continue; }
                if (!alsoCollider)
                {
                    colliderSkipped++;
                    sb.AppendLine("    ※MeshCollider が元のメッシュを使っています（未変更。元の重いメッシュがビルドに残ります）");
                    continue;
                }
                Undo.RecordObject(mc, "屋根を軽いメッシュに差し替え");
                mc.sharedMesh = lite;
                MarkChanged(mc);
                sb.AppendLine("    MeshCollider も差し替えました");
            }
        }
        Undo.CollapseUndoOperations(undoGroup);
        if (colliderSkipped > 0) { sb.AppendLine($"当たり判定が元のメッシュのままのもの：{colliderSkipped} 個"); }
        Debug.Log(sb.ToString());
    }

    // ---- 3 元に戻す ----

    private static void Restore()
    {
        var targets = new List<RoofRef>();
        foreach (RoofRef r in FindRoofFilters())
        {
            if (r.isLite || r.missing) { targets.Add(r); }
        }
        if (targets.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 屋根", "開いているシーンに、軽いメッシュ（または Missing）の屋根はありません。", "OK");
            return;
        }
        bool ok = EditorUtility.DisplayDialog(
            "ねこたぷ 屋根",
            $"屋根のメッシュ {targets.Count} 個を、元の FBX のメッシュに戻します。シーンは保存しません。",
            "戻す", "やめる");
        if (!ok) { return; }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("屋根を元のメッシュに戻す");
        int undoGroup = Undo.GetCurrentGroup();
        var sb = new StringBuilder();
        sb.AppendLine("屋根を元のメッシュに戻しました（まだ保存していません）：");
        foreach (RoofRef r in targets)
        {
            MeshFilter mf = r.filter;
            Mesh before = mf.sharedMesh;   // Missing のときは null
            Mesh original = FindOriginal(r.fbx, before);
            if (original == null)
            {
                sb.AppendLine($"  {HierarchyPath(mf.transform)} | 元のメッシュが見つかりません（未変更）");
                continue;
            }
            Undo.RecordObject(mf, "屋根を元のメッシュに戻す");
            mf.sharedMesh = original;
            MarkChanged(mf);
            string beforeText = before == null ? "Missing" : $"頂点 {before.vertexCount}";
            sb.AppendLine($"  {HierarchyPath(mf.transform)} | {beforeText} → 頂点 {original.vertexCount}");
            foreach (MeshCollider mc in mf.GetComponents<MeshCollider>())
            {
                if (mc.sharedMesh != before) { continue; }
                Undo.RecordObject(mc, "屋根を元のメッシュに戻す");
                mc.sharedMesh = original;
                MarkChanged(mc);
                sb.AppendLine("    MeshCollider も戻しました");
            }
        }
        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log(sb.ToString());
    }

    // ---- 部品（サブメッシュ）ごとに削る ----

    private static long[] SubMeshTriangles(Mesh mesh)
    {
        var result = new long[mesh.subMeshCount];
        for (int i = 0; i < mesh.subMeshCount; i++) { result[i] = mesh.GetIndexCount(i) / 3; }
        return result;
    }

    // 1 つのメッシュ全体を UnityMeshSimplifier で削る。
    private static Mesh SimplifyMesh(Type simplifier, Mesh mesh, int percent)
    {
        object instance = Activator.CreateInstance(simplifier);
        SetOptions(simplifier, instance);
        simplifier.GetMethod("Initialize", new[] { typeof(Mesh) }).Invoke(instance, new object[] { mesh });
        simplifier.GetMethod("SimplifyMesh", new[] { typeof(float) }).Invoke(instance, new object[] { percent / 100f });
        return (Mesh)simplifier.GetMethod("ToMesh", Type.EmptyTypes).Invoke(instance, null);
    }

    // 軽いメッシュを作る。tileOnly が true なら、瓦の部品だけを削り、ほかの部品は元のまま。部品の順（材質の並び）は変えない。
    private static Mesh BuildLite(Type simplifier, Mesh source, int percent, bool tileOnly, StringBuilder log)
    {
        if (!tileOnly) { return SimplifyMesh(simplifier, source, percent); }

        int partCount = source.subMeshCount;
        long[] triangles = SubMeshTriangles(source);
        var parts = new List<Mesh>();
        bool anyTile = false;
        try
        {
            for (int i = 0; i < partCount; i++)
            {
                Mesh part = ExtractSubMesh(source, i);
                if (triangles[i] >= TileMinTriangles)
                {
                    anyTile = true;
                    Mesh simplified = SimplifyMesh(simplifier, part, percent);
                    UnityEngine.Object.DestroyImmediate(part);
                    part = simplified;
                }
                parts.Add(part);
            }
            if (!anyTile)
            {
                log.AppendLine($"  {source.name}：三角形が {TileMinTriangles} を超える部品（瓦）が見つかりません。「設定：削る部品を瓦だけにする」をオフにすると、全体を削れます。");
                return null;
            }
            return JoinParts(source, parts);
        }
        finally
        {
            foreach (Mesh part in parts) { if (part != null) { UnityEngine.Object.DestroyImmediate(part); } }
        }
    }

    // メッシュから、1 つの部品の三角形と、それが使う頂点だけを取り出して、小さなメッシュにする。
    private static Mesh ExtractSubMesh(Mesh source, int subMesh)
    {
        int[] indices = source.GetIndices(subMesh);
        int vertexCount = source.vertexCount;
        var remap = new int[vertexCount];
        for (int i = 0; i < vertexCount; i++) { remap[i] = -1; }
        var used = new List<int>();
        var newIndices = new int[indices.Length];
        for (int i = 0; i < indices.Length; i++)
        {
            int old = indices[i];
            if (remap[old] < 0) { remap[old] = used.Count; used.Add(old); }
            newIndices[i] = remap[old];
        }

        var mesh = new Mesh { name = source.name + "_part" + subMesh, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(Pick(source.vertices, used));
        if (source.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal)) { mesh.SetNormals(Pick(source.normals, used)); }
        if (source.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent)) { mesh.SetTangents(Pick(source.tangents, used)); }
        if (source.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color)) { mesh.SetColors(Pick(source.colors, used)); }
        for (int ch = 0; ch < 8; ch++)
        {
            var attribute = (UnityEngine.Rendering.VertexAttribute)((int)UnityEngine.Rendering.VertexAttribute.TexCoord0 + ch);
            if (!source.HasVertexAttribute(attribute)) { continue; }
            var uvs = new List<Vector4>();
            source.GetUVs(ch, uvs);
            SetUVsByDimension(mesh, ch, Pick(uvs.ToArray(), used), source.GetVertexAttributeDimension(attribute));
        }
        mesh.SetTriangles(newIndices, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    // 部品ごとのメッシュを、元の部品の順に 1 つのメッシュへ組み直す（頂点の種類は元のメッシュに合わせる）。
    private static Mesh JoinParts(Mesh source, List<Mesh> parts)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var tangents = new List<Vector4>();
        var colors = new List<Color>();
        var uvs = new List<Vector4>[8];
        var indices = new List<int[]>();
        bool hasNormal = source.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal);
        bool hasTangent = source.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent);
        bool hasColor = source.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color);
        var uvDimension = new int[8];
        for (int ch = 0; ch < 8; ch++)
        {
            var attribute = (UnityEngine.Rendering.VertexAttribute)((int)UnityEngine.Rendering.VertexAttribute.TexCoord0 + ch);
            if (source.HasVertexAttribute(attribute))
            {
                uvDimension[ch] = source.GetVertexAttributeDimension(attribute);
                uvs[ch] = new List<Vector4>();
            }
        }

        foreach (Mesh part in parts)
        {
            int offset = vertices.Count;
            int count = part.vertexCount;
            vertices.AddRange(part.vertices);
            if (hasNormal) { AddOrFill(normals, part.normals, count, Vector3.up); }
            if (hasTangent) { AddOrFill(tangents, part.tangents, count, new Vector4(1f, 0f, 0f, 1f)); }
            if (hasColor) { AddOrFill(colors, part.colors, count, Color.white); }
            for (int ch = 0; ch < 8; ch++)
            {
                if (uvs[ch] == null) { continue; }
                var list = new List<Vector4>();
                part.GetUVs(ch, list);
                if (list.Count == count) { uvs[ch].AddRange(list); }
                else { for (int i = 0; i < count; i++) { uvs[ch].Add(Vector4.zero); } }
            }
            int[] partIndices = part.GetIndices(0);
            var shifted = new int[partIndices.Length];
            for (int i = 0; i < partIndices.Length; i++) { shifted[i] = partIndices[i] + offset; }
            indices.Add(shifted);
        }

        var mesh = new Mesh { name = source.name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        if (hasNormal) { mesh.SetNormals(normals); }
        if (hasTangent) { mesh.SetTangents(tangents); }
        if (hasColor) { mesh.SetColors(colors); }
        for (int ch = 0; ch < 8; ch++)
        {
            if (uvs[ch] != null) { SetUVsByDimension(mesh, ch, uvs[ch].ToArray(), uvDimension[ch]); }
        }
        mesh.subMeshCount = indices.Count;
        for (int i = 0; i < indices.Count; i++) { mesh.SetTriangles(indices[i], i, false); }
        mesh.RecalculateBounds();
        return mesh;
    }

    private static T[] Pick<T>(T[] values, List<int> used)
    {
        var result = new T[used.Count];
        for (int i = 0; i < used.Count; i++) { result[i] = values[used[i]]; }
        return result;
    }

    private static void AddOrFill<T>(List<T> list, T[] values, int count, T fill)
    {
        if (values != null && values.Length == count) { list.AddRange(values); }
        else { for (int i = 0; i < count; i++) { list.Add(fill); } }
    }

    private static void SetUVsByDimension(Mesh mesh, int channel, Vector4[] values, int dimension)
    {
        if (dimension >= 4) { mesh.SetUVs(channel, new List<Vector4>(values)); return; }
        if (dimension == 3)
        {
            var v3 = new List<Vector3>(values.Length);
            foreach (Vector4 v in values) { v3.Add(new Vector3(v.x, v.y, v.z)); }
            mesh.SetUVs(channel, v3);
            return;
        }
        var v2 = new List<Vector2>(values.Length);
        foreach (Vector4 v in values) { v2.Add(new Vector2(v.x, v.y)); }
        mesh.SetUVs(channel, v2);
    }

    // ---- 補助 ----

    private static Type FindSimplifier()
    {
        return Type.GetType(SimplifierType);
    }

    private static void ShowNotInstalled()
    {
        EditorUtility.DisplayDialog(
            "ねこたぷ 屋根",
            "UnityMeshSimplifier が入っていません。\n\n" +
            "Window ＞ Package Manager ＞ 左上の「＋」＞ Add package from git URL… で、次を入れてください。\n" +
            "https://github.com/Whinarn/UnityMeshSimplifier.git\n\n" +
            "入れたら、もう一度このメニューを押してください。",
            "OK");
    }

    // メニューの設定（縁・継ぎ目・SmartLink・つなぐ距離）を、UnityMeshSimplifier のオプションに反映する。
    private static void SetOptions(Type simplifier, object instance)
    {
        Type optionsType = simplifier.Assembly.GetType("UnityMeshSimplifier.SimplificationOptions");
        if (optionsType == null) { return; }
        object options = optionsType.GetField("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        optionsType.GetField("PreserveBorderEdges").SetValue(options, GetBorder());
        optionsType.GetField("PreserveUVSeamEdges").SetValue(options, GetSeam());
        optionsType.GetField("EnableSmartLink").SetValue(options, GetSmartLink());
        if (GetLinkDistance() > 0f)
        {
            optionsType.GetField("VertexLinkDistance").SetValue(options, (double)GetLinkDistance());
        }
        simplifier.GetProperty("SimplificationOptions").SetValue(instance, options);
    }

    private static IEnumerable<MeshFilter> AllMeshFilters()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) { continue; }
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (MeshFilter mf in root.GetComponentsInChildren<MeshFilter>(true)) { yield return mf; }
            }
        }
    }

    private static bool IsRoofFbx(string assetPath)
    {
        return RoofFbx.Any(f => string.Equals(f, assetPath, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeRoofFbx(string assetPath)
    {
        return RoofFbx.First(f => string.Equals(f, assetPath, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsLiteAsset(string assetPath)
    {
        return assetPath.StartsWith(OutDir + "/", StringComparison.Ordinal) && assetPath.Contains(LiteMark);
    }

    private class RoofRef
    {
        public MeshFilter filter;
        public string fbx;       // 元の FBX のパス
        public bool isLite;      // 軽いメッシュを使っている
        public bool missing;     // メッシュが無くなっている（Missing）
    }

    // 開いているシーンの屋根の MeshFilter を探す。
    // 元の FBX のメッシュ、軽いメッシュ（どの割合でも）、メッシュが Missing になっているもの（朱雀門の中の Roof_02 / Roof_03）が対象。
    private static List<RoofRef> FindRoofFilters()
    {
        var list = new List<RoofRef>();
        foreach (MeshFilter mf in AllMeshFilters())
        {
            Mesh mesh = mf.sharedMesh;
            if (mesh != null)
            {
                string path = AssetDatabase.GetAssetPath(mesh);
                if (IsRoofFbx(path))
                {
                    list.Add(new RoofRef { filter = mf, fbx = NormalizeRoofFbx(path) });
                }
                else if (IsLiteAsset(path))
                {
                    string fbx = FbxFromLiteAsset(path);
                    if (fbx != null) { list.Add(new RoofRef { filter = mf, fbx = fbx, isLite = true }); }
                }
            }
            else
            {
                string fbx = FbxFromName(mf);
                if (fbx != null) { list.Add(new RoofRef { filter = mf, fbx = fbx, missing = true }); }
            }
        }
        return list;
    }

    // 「Assets/LightMeshes/SM_Roof_03__lite20.asset」から、元の FBX のパスを求める。
    private static string FbxFromLiteAsset(string assetPath)
    {
        string assetName = Path.GetFileNameWithoutExtension(assetPath);
        int mark = assetName.IndexOf(LiteMark, StringComparison.Ordinal);
        if (mark < 0) { return null; }
        string fbxName = assetName.Substring(0, mark);
        return RoofFbx.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == fbxName);
    }

    // メッシュが Missing の MeshFilter は、名前（Roof_02 / Roof_03）と、親に SuzakuGate があることで屋根と判断する。
    private static string FbxFromName(MeshFilter mf)
    {
        string fbx = RoofFbx.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == "SM_" + mf.gameObject.name);
        if (fbx == null) { return null; }
        for (Transform t = mf.transform.parent; t != null; t = t.parent)
        {
            if (t.name == "SuzakuGate") { return fbx; }
        }
        return null;
    }

    // 元の FBX の中のメッシュを探す（現在のメッシュの名前があれば、同じ名前を優先する）。
    private static Mesh FindOriginal(string fbx, Mesh current)
    {
        List<Mesh> candidates = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Mesh>().ToList();
        if (current != null)
        {
            string meshName = current.name;
            int meshMark = meshName.IndexOf(LiteMark, StringComparison.Ordinal);
            if (meshMark >= 0) { meshName = meshName.Substring(0, meshMark); }
            Mesh byName = candidates.FirstOrDefault(m => m.name == meshName);
            if (byName != null) { return byName; }
        }
        return candidates.Count == 1 ? candidates[0] : null;
    }

    private static void MarkChanged(Component component)
    {
        EditorUtility.SetDirty(component);
        if (PrefabUtility.IsPartOfPrefabInstance(component))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        }
        if (component.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
        }
    }

    private static void EnsureOutDir()
    {
        if (!AssetDatabase.IsValidFolder(OutDir))
        {
            AssetDatabase.CreateFolder("Assets", "LightMeshes");
        }
    }

    private static long CountTriangles(Mesh mesh)
    {
        long total = 0;
        for (int i = 0; i < mesh.subMeshCount; i++) { total += mesh.GetIndexCount(i) / 3; }
        return total;
    }

    private static string Ratio(int after, int before)
    {
        return before == 0 ? "-" : $"{100.0 * after / before:0.#}%";
    }

    private static string FileSize(string assetPath)
    {
        return File.Exists(assetPath) ? FormatBytes(new FileInfo(assetPath).Length) : "-";
    }

    private static string FormatBytes(long bytes)
    {
        return bytes >= 1024 * 1024 ? $"{bytes / 1048576.0:0.0}MB" : $"{bytes / 1024.0:0}KB";
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
