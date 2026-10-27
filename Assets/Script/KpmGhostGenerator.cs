using UnityEngine;

/// <summary>
/// Kpm から GhostRecord を生成する。
/// 出だしは遅め、途中で速さが大きく増減し、終了時点の累計打鍵数は Kpm どおり（Kpm × 秒 ÷ 60）にそろえる。
/// 速さのゆらぎ方は Uid から決まるため、同じねこは毎回同じ走り方になる。
/// </summary>
public static class KpmGhostGenerator
{
    private const int IntegrateSteps = 400;     // 速さを積分する分割数

    /// <param name="wobbleStrength">速さのゆらぎの大きさ（0 で一定、1 で標準）</param>
    /// <param name="falls">こける設定（null ならこけない）</param>
    public static GhostRecord Create(int kpm, float totalTime, string uid, float interval = 5f, float wobbleStrength = 1f, FallSettings falls = null)
    {
        var record = new GhostRecord();
        record.interval = interval;
        record.totalTime = totalTime;

        int pointCount = GhostRecord.GetPointCount(totalTime, interval);
        record.counts = new int[pointCount];
        int finalCount = Mathf.Max(0, Mathf.RoundToInt(kpm * totalTime / 60f));
        if (finalCount == 0 || totalTime <= 0f)
        {
            return record;      // すべて 0
        }

        // Uid から固定の乱数でゆらぎのパラメータを決める
        uint state = HashUid(uid);
        // ゆらぎを大きめにして、速くなったり遅くなったりが見て分かるようにする（標準でおよそ 0.6〜1.4 倍）
        // 記録は 5 秒ごとなので、細かすぎる波（周期 10 秒未満）は使わない
        float strength = Mathf.Max(0f, wobbleStrength);
        float amp1 = Range(ref state, 0.18f, 0.28f) * strength;
        float freq1 = Range(ref state, 1.0f, 2.0f);
        float phase1 = Range(ref state, 0f, 1f);
        float amp2 = Range(ref state, 0.08f, 0.14f) * strength;
        float freq2 = Range(ref state, 2.5f, 4.0f);
        float phase2 = Range(ref state, 0f, 1f);
        float startSpeed = Range(ref state, 0.45f, 0.65f);     // 出だしの速さ（通常の何倍か）
        float rampEnd = Range(ref state, 0.10f, 0.18f);        // 通常の速さに達する位置（全体に対する割合）

        // こけないときの速さ（通常を 1 とした倍率）。u は全体に対する割合（0〜1）
        float BaseSpeed(float u)
        {
            float ramp = Mathf.Lerp(startSpeed, 1f, Mathf.SmoothStep(0f, 1f, u / rampEnd));
            float wobble = 1f
                + amp1 * Mathf.Sin(2f * Mathf.PI * (freq1 * u + phase1))
                + amp2 * Mathf.Sin(2f * Mathf.PI * (freq2 * u + phase2));
            return Mathf.Max(0.15f, ramp * wobble);
        }

        // こける時刻を決め、その直前の速さでこけ方と時間を決める（速いほど大きくこけて長く止まる）
        if (falls != null)
        {
            record.fallTimes = ChooseFallTimes(falls, totalTime);
            int n = record.fallTimes.Length;
            record.fallKinds = new int[n];
            record.fallDurations = new float[n];
            for (int f = 0; f < n; f++)
            {
                float before = BaseSpeed(record.fallTimes[f] / totalTime);
                int kind = (before >= falls.hardFallSpeed) ? 2 : (before >= falls.fallSpeed) ? 1 : 0;
                record.fallKinds[f] = kind;
                record.fallDurations[f] = (falls.durations != null && falls.durations.Length > 0)
                    ? falls.durations[Mathf.Min(kind, falls.durations.Length - 1)] : 1f;
            }
        }

        // 速さを積分して累計の進み具合（0〜1 に正規化する前の値）を求める
        float[] cumulative = new float[IntegrateSteps + 1];
        cumulative[0] = 0f;
        for (int s = 1; s <= IntegrateSteps; s++)
        {
            float u = (s - 0.5f) / IntegrateSteps;
            float speed = BaseSpeed(u);
            if (falls != null)
            {
                speed = ApplyFalls(speed, u * totalTime, record, falls);
            }
            cumulative[s] = cumulative[s - 1] + speed;
        }
        float total = cumulative[IntegrateSteps];

        // 各記録点の累計打鍵数を求める（単調増加・最後は finalCount ちょうど）
        int prev = 0;
        for (int i = 0; i < pointCount; i++)
        {
            float u = record.GetPointTime(i) / totalTime;
            float pos = u * IntegrateSteps;
            int idx = Mathf.Min((int)pos, IntegrateSteps - 1);
            float value = Mathf.Lerp(cumulative[idx], cumulative[idx + 1], pos - idx) / total;
            int count = Mathf.RoundToInt(finalCount * value);
            count = Mathf.Clamp(count, prev, finalCount);
            record.counts[i] = count;
            prev = count;
        }
        record.counts[0] = 0;
        record.counts[pointCount - 1] = finalCount;

        return record;
    }

    /// <summary>
    /// こける設定
    /// </summary>
    public class FallSettings
    {
        public int maxCount = 2;            // 1 回のゲームでこける最大の回数（0〜maxCount 回）
        public float[] durations = { 1f, 2f, 2.5f };    // こけている時間（秒）。つまずく・こける・おもいっきりこける。このあいだは進まない
        public float fallSpeed = 1.0f;      // こける直前の速さがこれ以上なら「こける」
        public float hardFallSpeed = 1.25f; // こける直前の速さがこれ以上なら「おもいっきりこける」。未満で fallSpeed 未満は「つまずく」
        public float slowSpeed = 0.4f;      // こけたあとの速さ（通常の何倍か。いちばん遅い速さ）
        public float slowTime = 3f;         // いちばん遅い速さが続く時間（秒）
        public float recoverTime = 2f;      // いちばん遅い速さから元の速さに戻るまでの時間（秒）
        public int seed;                    // こける回数・時刻を決める乱数の種（毎回変える）
    }

    // こける回数と時刻を決める。ゲームの 20%〜85% のあいだに、こけてから元に戻るまでが重ならないように置く
    private static float[] ChooseFallTimes(FallSettings falls, float totalTime)
    {
        var rand = new System.Random(falls.seed);
        int count = rand.Next(0, Mathf.Max(0, falls.maxCount) + 1);
        float longest = (falls.durations != null && falls.durations.Length > 0) ? Mathf.Max(falls.durations) : 1f;
        float span = longest + falls.slowTime + falls.recoverTime;
        float from = totalTime * 0.2f;
        float to = totalTime * 0.85f;
        var times = new System.Collections.Generic.List<float>();
        for (int tries = 0; tries < 50 && times.Count < count; tries++)
        {
            float t = from + (float)rand.NextDouble() * Mathf.Max(0f, to - from);
            bool overlaps = false;
            foreach (float other in times)
            {
                if (Mathf.Abs(t - other) < span) { overlaps = true; break; }
            }
            if (!overlaps) times.Add(t);
        }
        times.Sort();
        return times.ToArray();
    }

    // こけている最中は 0、こけたあとはいちばん遅い速さ、そこから元の速さへ戻す
    private static float ApplyFalls(float speed, float time, GhostRecord record, FallSettings falls)
    {
        for (int i = 0; i < record.fallTimes.Length; i++)
        {
            float d = time - record.fallTimes[i];
            if (d < 0f) continue;
            if (d < record.fallDurations[i]) return 0f;
            d -= record.fallDurations[i];
            if (d < falls.slowTime) return Mathf.Min(speed, falls.slowSpeed);
            d -= falls.slowTime;
            if (d < falls.recoverTime)
            {
                float slow = Mathf.Min(speed, falls.slowSpeed);
                return Mathf.Lerp(slow, speed, d / Mathf.Max(0.01f, falls.recoverTime));
            }
        }
        return speed;
    }

    // Uid から 32bit の値を作る（FNV-1a）。string.GetHashCode は環境によって値が変わりうるため使わない
    private static uint HashUid(string uid)
    {
        uint hash = 2166136261;
        if (!string.IsNullOrEmpty(uid))
        {
            foreach (char c in uid)
            {
                hash ^= c;
                hash *= 16777619;
            }
        }
        return hash == 0 ? 1u : hash;
    }

    // xorshift32 による乱数（min〜max）
    private static float Range(ref uint state, float min, float max)
    {
        state ^= state << 13;
        state ^= state >> 17;
        state ^= state << 5;
        float t = (state & 0xFFFFFF) / (float)0x1000000;
        return Mathf.Lerp(min, max, t);
    }
}
