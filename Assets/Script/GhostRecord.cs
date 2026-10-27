using System;
using UnityEngine;

/// <summary>
/// ゴーストの進み具合の記録。一定間隔（既定 5 秒）ごとの累計打鍵数を保持する。
/// ゴースト対戦（Kpm から生成）とネット対戦（受信した値）の両方で同じ形を使う。
/// </summary>
[Serializable]
public class GhostRecord
{
    public float interval = 5f;     // 記録の間隔（秒）
    public float totalTime;         // 記録の総時間（秒）
    public int[] counts;            // counts[i] = min(i * interval, totalTime) 秒時点の累計打鍵数。counts[0] は 0
    // こけた記録。ゴースト対戦（Kpm から生成）でだけ使う。3 つの配列は同じ長さ
    public float[] fallTimes = new float[0];        // こけた時刻（秒）
    public float[] fallDurations = new float[0];    // こけている時間（秒）
    public int[] fallKinds = new int[0];            // こけ方（0 つまずく、1 こける、2 おもいっきりこける）

    /// <summary>
    /// 経過秒の時点でこけている最中なら、こけ方（0〜2）を返す。こけていなければ -1。
    /// </summary>
    public int GetFallKind(float elapsed)
    {
        if (fallTimes == null) return -1;
        for (int i = 0; i < fallTimes.Length; i++)
        {
            if (elapsed >= fallTimes[i] && elapsed < fallTimes[i] + fallDurations[i]) return fallKinds[i];
        }
        return -1;
    }

    /// <summary>
    /// 記録の終了時点の累計打鍵数
    /// </summary>
    public int FinalCount
    {
        get { return (counts == null || counts.Length == 0) ? 0 : counts[counts.Length - 1]; }
    }

    /// <summary>
    /// 総時間に対して必要な記録点の数（0 秒の点を含む）
    /// </summary>
    public static int GetPointCount(float totalTime, float interval)
    {
        return Mathf.CeilToInt(totalTime / interval) + 1;
    }

    /// <summary>
    /// i 番目の記録点の時刻（秒）
    /// </summary>
    public float GetPointTime(int i)
    {
        return Mathf.Min(i * interval, totalTime);
    }

    /// <summary>
    /// 経過秒における累計打鍵数を返す。記録点のあいだは直線で補完する。
    /// </summary>
    public float GetCount(float elapsed)
    {
        if (counts == null || counts.Length == 0) return 0f;
        if (elapsed <= 0f) return counts[0];
        if (elapsed >= totalTime) return FinalCount;

        int i = Mathf.Min((int)(elapsed / interval), counts.Length - 2);
        float t0 = GetPointTime(i);
        float t1 = GetPointTime(i + 1);
        float rate = (t1 > t0) ? (elapsed - t0) / (t1 - t0) : 1f;
        return Mathf.Lerp(counts[i], counts[i + 1], rate);
    }
}
