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
    private const string LiteMark = "__lite";
    private const string SimplifierType = "UnityMeshSimplifier.MeshSimplifier, Whinarn.UnityMeshSimplifier.Runtime";
    private const long WarnBytes = 8L * 1024 * 1024;

    // 軽くする対象の元の FBX（朱雀門の屋根）
    private static readonly string[] RoofFbx =
    {
        "Assets/AssetsOld/JejumokGwana/Meshes/Roofs/SM_Roof_02.fbx",
        "Assets/AssetsOld/JejumokGwana/Meshes/Roofs/SM_Roof_03.fbx",
    };

    [MenuItem(MenuRoot + "1 軽いメッシュを作る（10%）")]
    private static void Make10() { Make(10); }

    [MenuItem(MenuRoot + "1 軽いメッシュを作る（20%）")]
    private static void Make20() { Make(20); }

    [MenuItem(MenuRoot + "1 軽いメッシュを作る（30%）")]
    private static void Make30() { Make(30); }

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
        int original = 0, lite = 0;
        foreach (MeshFilter mf in AllMeshFilters())
        {
            if (mf.sharedMesh == null) { continue; }
            string p = AssetDatabase.GetAssetPath(mf.sharedMesh);
            if (IsRoofFbx(p))
            {
                original++;
                sb.AppendLine($"  元のメッシュを使用中：{HierarchyPath(mf.transform)}（頂点 {mf.sharedMesh.vertexCount}）");
            }
            else if (IsLiteAsset(p))
            {
                lite++;
                sb.AppendLine($"  軽いメッシュを使用中：{HierarchyPath(mf.transform)}（頂点 {mf.sharedMesh.vertexCount}）");
            }
        }
        sb.AppendLine($"開いているシーンの屋根：元のメッシュ {original} 個、軽いメッシュ {lite} 個");
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
            "元の FBX は変更しません。数十秒〜数分かかることがあります。",
            "作る", "やめる");
        if (!ok) { return; }

        EnsureOutDir();
        var sb = new StringBuilder();
        sb.AppendLine($"軽いメッシュを作りました（{percent}%）：");
        try
        {
            for (int i = 0; i < meshes.Count; i++)
            {
                string fbx = meshes[i].Key;
                Mesh source = meshes[i].Value;
                EditorUtility.DisplayProgressBar("ねこたぷ 屋根", $"{source.name}（{i + 1}/{meshes.Count}）", (float)i / meshes.Count);

                int beforeVertices = source.vertexCount;
                long beforeTriangles = CountTriangles(source);

                object instance = Activator.CreateInstance(simplifier);
                SetOptions(simplifier, instance);
                simplifier.GetMethod("Initialize", new[] { typeof(Mesh) }).Invoke(instance, new object[] { source });
                simplifier.GetMethod("SimplifyMesh", new[] { typeof(float) }).Invoke(instance, new object[] { percent / 100f });
                Mesh lite = (Mesh)simplifier.GetMethod("ToMesh", Type.EmptyTypes).Invoke(instance, null);

                lite.name = source.name + LiteMark + percent;
                lite.RecalculateBounds();
                MeshUtility.SetMeshCompression(lite, ModelImporterMeshCompression.High);

                string path = $"{OutDir}/{Path.GetFileNameWithoutExtension(fbx)}{LiteMark}{percent}.asset";
                if (AssetDatabase.LoadAssetAtPath<Mesh>(path) != null) { AssetDatabase.DeleteAsset(path); }
                AssetDatabase.CreateAsset(lite, path);

                sb.AppendLine($"  {source.name}：頂点 {beforeVertices} → {lite.vertexCount}（{Ratio(lite.vertexCount, beforeVertices)}）、" +
                              $"三角形 {beforeTriangles} → {CountTriangles(lite)}、サブメッシュ {source.subMeshCount} → {lite.subMeshCount}");
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
        int percent = EditorPrefs.GetInt(PrefKey, 0);
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

        var filters = new List<MeshFilter>();
        foreach (MeshFilter mf in AllMeshFilters())
        {
            if (mf.sharedMesh != null && IsRoofFbx(AssetDatabase.GetAssetPath(mf.sharedMesh))) { filters.Add(mf); }
        }
        if (filters.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 屋根", "開いているシーンに、朱雀門の屋根が見つかりません。\nWorldScene を開いてから、もう一度押してください。\n（すでに差し替え済みかもしれません。「状態の一覧」で確認できます）", "OK");
            return;
        }

        bool ok = EditorUtility.DisplayDialog(
            "ねこたぷ 屋根",
            $"屋根のメッシュ {filters.Count} 個を、{percent}% の軽いメッシュに差し替えます。\n" +
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
        foreach (MeshFilter mf in filters)
        {
            string fbx = AssetDatabase.GetAssetPath(mf.sharedMesh);
            Mesh original = mf.sharedMesh;
            Mesh lite = liteByFbx[NormalizeRoofFbx(fbx)];
            Undo.RecordObject(mf, "屋根を軽いメッシュに差し替え");
            mf.sharedMesh = lite;
            MarkChanged(mf);
            sb.AppendLine($"  {HierarchyPath(mf.transform)} | 頂点 {original.vertexCount} → {lite.vertexCount}");

            foreach (MeshCollider mc in mf.GetComponents<MeshCollider>())
            {
                if (mc.sharedMesh != original) { continue; }
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
        var filters = new List<MeshFilter>();
        foreach (MeshFilter mf in AllMeshFilters())
        {
            if (mf.sharedMesh != null && IsLiteAsset(AssetDatabase.GetAssetPath(mf.sharedMesh))) { filters.Add(mf); }
        }
        if (filters.Count == 0)
        {
            EditorUtility.DisplayDialog("ねこたぷ 屋根", "開いているシーンに、軽いメッシュを使っている屋根はありません。", "OK");
            return;
        }
        bool ok = EditorUtility.DisplayDialog(
            "ねこたぷ 屋根",
            $"屋根のメッシュ {filters.Count} 個を、元の FBX のメッシュに戻します。シーンは保存しません。",
            "戻す", "やめる");
        if (!ok) { return; }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("屋根を元のメッシュに戻す");
        int undoGroup = Undo.GetCurrentGroup();
        var sb = new StringBuilder();
        sb.AppendLine("屋根を元のメッシュに戻しました（まだ保存していません）：");
        foreach (MeshFilter mf in filters)
        {
            Mesh lite = mf.sharedMesh;
            Mesh original = FindOriginal(lite);
            if (original == null)
            {
                sb.AppendLine($"  {HierarchyPath(mf.transform)} | 元のメッシュが見つかりません（未変更）");
                continue;
            }
            Undo.RecordObject(mf, "屋根を元のメッシュに戻す");
            mf.sharedMesh = original;
            MarkChanged(mf);
            sb.AppendLine($"  {HierarchyPath(mf.transform)} | 頂点 {lite.vertexCount} → {original.vertexCount}");
            foreach (MeshCollider mc in mf.GetComponents<MeshCollider>())
            {
                if (mc.sharedMesh != lite) { continue; }
                Undo.RecordObject(mc, "屋根を元のメッシュに戻す");
                mc.sharedMesh = original;
                MarkChanged(mc);
                sb.AppendLine("    MeshCollider も戻しました");
            }
        }
        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log(sb.ToString());
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

    // 見た目が変わりにくいように、UV の継ぎ目と境界の辺を守る設定にする。
    private static void SetOptions(Type simplifier, object instance)
    {
        Type optionsType = simplifier.Assembly.GetType("UnityMeshSimplifier.SimplificationOptions");
        if (optionsType == null) { return; }
        object options = optionsType.GetField("Default", BindingFlags.Public | BindingFlags.Static).GetValue(null);
        optionsType.GetField("PreserveBorderEdges").SetValue(options, true);
        optionsType.GetField("PreserveUVSeamEdges").SetValue(options, true);
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

    // 「SM_Roof_03__lite20」のメッシュから、元の FBX の同じ名前のメッシュを探す。
    private static Mesh FindOriginal(Mesh lite)
    {
        string assetName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(lite));
        int mark = assetName.IndexOf(LiteMark, StringComparison.Ordinal);
        if (mark < 0) { return null; }
        string fbxName = assetName.Substring(0, mark);
        string meshName = lite.name;
        int meshMark = meshName.IndexOf(LiteMark, StringComparison.Ordinal);
        if (meshMark >= 0) { meshName = meshName.Substring(0, meshMark); }
        string fbx = RoofFbx.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == fbxName);
        if (fbx == null) { return null; }
        List<Mesh> candidates = AssetDatabase.LoadAllAssetsAtPath(fbx).OfType<Mesh>().ToList();
        return candidates.FirstOrDefault(m => m.name == meshName) ?? (candidates.Count == 1 ? candidates[0] : null);
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
