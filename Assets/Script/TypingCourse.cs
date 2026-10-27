using UnityEngine;

/// <summary>
/// タイピング画面の「打った数で進むコース」。
/// スタート（タイマー開始時の自分のねこの X 座標）からゴール（画面右端の少し手前の X 座標）までを、
/// 正しく打ったキーの数で進む。進み方は 3 つの区間に分かれる。
///   1. 出だし：最初の数秒ぶんは、ゆっくり進みはじめて少しずつふつうの進み方になる（歩き出しと合わせるため）
///   2. まんなか：打った数に比例して進む
///   3. ゴール手前：ゴールに近づくほど進み方が小さくなり、ゴールにはほぼ届かない（端で止まって見えないようにするため）
/// 自分のねこ（TypingSoft）とゴースト（GhostRunner）が同じコースを使う。ネット対戦（B）でも使う想定。
/// </summary>
public class TypingCourse
{
    public float StartX { get; private set; }
    public float EndX { get; private set; }
    public float KeysToGoal { get; private set; }

    private readonly float startZone;       // 出だしの区間の長さ（コースに対する割合）
    private readonly float startSpeed;      // 出だしの最初の進み方（ふつうの何倍か）
    private readonly float startLoss;       // 出だしをゆっくりにした分、遅れる量
    private readonly float slowdownStart;   // ゴール手前で遅くしはじめる位置（コースに対する割合）

    /// <param name="startX">スタートの X 座標</param>
    /// <param name="endX">ゴールの X 座標</param>
    /// <param name="expectedKeys">基準になる打鍵数（この数を打つとコースの expectedRatio まで進む）</param>
    /// <param name="expectedRatio">基準の打鍵数で進む割合（0〜1）</param>
    /// <param name="startZoneRatio">出だしの区間の長さ（ふつうに進んだときのコースに対する割合）</param>
    /// <param name="startSpeedRatio">出だしの最初の進み方（ふつうの何倍か。0〜1）</param>
    /// <param name="slowdownStartRatio">ゴール手前で遅くしはじめる位置（0〜1）</param>
    public TypingCourse(float startX, float endX, float expectedKeys, float expectedRatio,
        float startZoneRatio = 0f, float startSpeedRatio = 1f, float slowdownStartRatio = 1f)
    {
        StartX = startX;
        EndX = endX;
        float ratio = Mathf.Clamp(expectedRatio, 0.1f, 1f);
        KeysToGoal = Mathf.Max(1f, expectedKeys / ratio);
        startZone = Mathf.Clamp(startZoneRatio, 0f, 0.5f);
        startSpeed = Mathf.Clamp01(startSpeedRatio);
        startLoss = startZone * (1f - startSpeed) / 2f;
        slowdownStart = Mathf.Clamp(slowdownStartRatio, 0.1f, 1f);
    }

    /// <summary>
    /// 打鍵数に対応する X 座標を返す（スタートからゴールの範囲におさめる）。
    /// </summary>
    public float XAt(float keys)
    {
        float u = Mathf.Max(0f, keys / KeysToGoal);     // ふつうに進んだときの割合（1 でゴール。1 を超えることもある）

        // 1. 出だし：進み方を startSpeed から 1 まで直線的に上げる
        float v;
        if (u < startZone)
        {
            v = startSpeed * u + (1f - startSpeed) * u * u / (2f * startZone);
        }
        else
        {
            v = u - startLoss;
        }
        v /= (1f - startLoss);      // ふつうに打ったときに、まんなか以降で元の位置に戻るように伸ばす

        // 3. ゴール手前：slowdownStart から先は、ゴールに近づくほど進み方を小さくする（ゴールには届かない）
        if (v > slowdownStart && slowdownStart < 1f)
        {
            float rest = 1f - slowdownStart;
            v = slowdownStart + rest * (1f - Mathf.Exp(-(v - slowdownStart) / rest));
        }
        return Mathf.Lerp(StartX, EndX, Mathf.Clamp01(v));
    }

    /// <summary>
    /// カメラの画面上で、指定した横位置（ビューポート比）にあたる X 座標を、基準の位置と同じ奥行き・高さで求める。
    /// </summary>
    public static float ViewportX(Camera cam, Vector3 reference, float viewportX)
    {
        Vector3 vp = cam.WorldToViewportPoint(reference);
        return cam.ViewportToWorldPoint(new Vector3(viewportX, vp.y, vp.z)).x;
    }
}
