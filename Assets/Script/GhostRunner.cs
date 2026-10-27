using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// タイピング画面でゴースト（ランキングで 1 つ上のねこ）を表示する係。
/// 進み具合は GhostRecord から受け取り、自分の打鍵数との差でゴーストの前後の位置を決める。
/// TypingSoft から Setup → StartRace → UpdateRace → FinishRace の順に呼ばれる。
/// スイッチ（GhostSwitch）とどちらが先に初期化されても同じ結果になるように、状態を持って Refresh でまとめて反映する。
/// </summary>
public class GhostRunner : MonoBehaviour
{
    [SerializeField] private GameManager gm;
    [SerializeField] private GameObject ghostCat;       // ゴーストのねこ（typeNeco）
    [SerializeField] private Transform player;          // 自分のねこ
    [SerializeField] private Camera targetCamera;       // タイピング画面のカメラ
    [SerializeField] private Material ghostMaterial;    // 半透明用のマテリアル（Standard・Fade）
    [SerializeField] private TMP_Text messageText;      // ゴーストのメッセージ表示

    [SerializeField] private float ghostAlpha = 0.45f;      // ゴーストの不透明度
    [SerializeField] private float depthOffset = 1.5f;      // 自分のねこより奥へずらす距離
    [SerializeField] private float heightOffset = 0f;       // 自分のねこからの上下のずれ（+で上）
    [SerializeField] private float sideOffset = 0f;         // いつも左右にずらす量（+で右）
    [SerializeField] private float noGhostMessageTime = 3f; // 「まだ いないよ」を表示する秒数

    [Header("ゴーストの速さの変化")]
    [SerializeField] private float wobbleStrength = 1.0f;    // 速さのゆらぎの大きさ（0 で一定、1 で標準、2 でかなり大きい）

    [Header("カウントダウン")]
    [SerializeField] private float kamaeDelay = 0.5f;          // 自分のねこが構えてから、ゴーストが構えるまでの秒数

    [Header("こける")]
    [SerializeField] private int maxFalls = 2;                  // 1 回のゲームでこける最大の回数（毎回 0〜この回数）
    // こけ方は直前の速さで決まる（自分のねこのミスと同じ考え方：速いほど大きくこける）
    [SerializeField] private float fallSpeed = 1.0f;            // 直前の速さ（ふだんの何倍か）がこれ以上なら「こける」
    [SerializeField] private float hardFallSpeed = 1.25f;       // これ以上なら「おもいっきりこける」。fallSpeed 未満は「つまずく」
    [SerializeField] private string[] fallStateNames = { "Damage", "DieA", "DieB" };    // つまずく・こける・おもいっきりこける のアニメ
    [SerializeField] private float[] fallDurations = { 1f, 2f, 2.5f };                  // それぞれ止まっている秒数
    [SerializeField] private float afterFallSpeed = 0.4f;       // こけたあとの速さ（ふだんの何倍か）
    [SerializeField] private float afterFallSlowTime = 3f;      // こけたあと、遅い速さが続く秒数
    [SerializeField] private float afterFallRecoverTime = 2f;   // 遅い速さから元に戻るまでの秒数

    [Header("アニメ（ゴーストの今の速さで決める）")]
    // 速さの比＝今のペース ÷ そのねこの平均ペース（Kpm）
    [SerializeField] private float walkBelowRatio = 0.85f;   // 速さの比がこれより小さいと「歩く（Walk）」
    [SerializeField] private float runAboveRatio = 1.15f;    // 速さの比がこれより大きいと「全力で走る（Run）」。あいだは「走る（Move）」
    [SerializeField] private float animSpeedMin = 0.6f;      // アニメの速さの下限
    [SerializeField] private float animSpeedMax = 2.0f;      // アニメの速さの上限
    [SerializeField] private float animHoldTime = 1.0f;      // アニメを切り替えたあと、次に切り替えるまでの最短の秒数（ちらつき防止）

    private const string MsgNoGhost = "ゴーストは まだ いないよ";
    private const string MsgWin = "ゴーストに かった！";
    private const string MsgLose = "ゴーストに まけた…";
    private const string MsgDraw = "ひきわけ！";

    private bool switchOn = false;      // スイッチの状態
    private bool timerMode = false;     // 時間制限ありのステージかどうか（Setup で true）
    private GhostRecord record;         // ゴーストの進み具合（相手がいなければ null）
    private bool appearanceApplied = false;
    private bool running = false;
    private bool finished = false;
    private int myCount = 0;
    private float ghostCount = 0f;
    private TypingCourse course;        // 自分のねこと同じコース（TypingSoft から受け取る）
    private float messageHideTime = -1f;
    private Animator ghostAnimator;
    private ExRank target;
    private int animState = -1;         // 今のアニメ（-1 なし、0 Walk、1 Move、2 Run、3 こける）
    private const int AnimFall = 3;
    private Coroutine kamaeRoutine;
    private float animChangedAt = -999f;
    private static readonly string[] AnimStateNames = { "Walk", "Move", "Run" };
    private static readonly string[] AnimSpeedParams = { "walkSpeed", "moveSpeed", "runSpeed" };
    private float lastElapsed = 0f;

    void Awake()
    {
        if (ghostCat != null)
        {
            ghostAnimator = ghostCat.GetComponentInChildren<Animator>(true);
            ghostCat.SetActive(false);
        }
        SetMessage("");
    }

    /// <summary>
    /// 時間制限ありのステージの開始時に呼ぶ。ゴーストの相手を決め、進み具合を作る。
    /// </summary>
    public void Setup(float totalTime)
    {
        timerMode = true;
        target = FindTarget();
        var falls = new KpmGhostGenerator.FallSettings
        {
            maxCount = maxFalls,
            durations = fallDurations,
            fallSpeed = fallSpeed,
            hardFallSpeed = hardFallSpeed,
            slowSpeed = afterFallSpeed,
            slowTime = afterFallSlowTime,
            recoverTime = afterFallRecoverTime,
            seed = System.Environment.TickCount,    // こける回数・場所は毎回変える
        };
        // こけて止まる様子を位置に出すため、記録の間隔は 0.5 秒にする（ネット対戦の 5 秒とは別）
        record = (target != null) ? KpmGhostGenerator.Create(target.Kpm, totalTime, target.Uid, 0.5f, wobbleStrength, falls) : null;
        if (record != null && record.fallTimes.Length > 0)
        {
            var info = new string[record.fallTimes.Length];
            for (int i = 0; i < info.Length; i++) info[i] = $"{record.fallTimes[i]:0.0}秒・{fallStateNames[Mathf.Clamp(record.fallKinds[i], 0, fallStateNames.Length - 1)]}";
            Debug.Log($"ゴースト対戦：こける回数={info.Length}（{string.Join("、", info)}）");
        }
        if (target != null)
        {
            Debug.Log($"ゴースト対戦：相手のKpm={target.Kpm}、終了時の打鍵数={record.FinalCount}");
        }
        else
        {
            Debug.Log("ゴースト対戦：相手となるねこがいません。");
        }
        Refresh(true);      // スイッチがオンで相手がいなければメッセージを出す（GhostSwitch との初期化順に依らない）
    }

    /// <summary>
    /// スイッチの状態を反映する。showMessage が true のとき、相手がいなければメッセージを出す。
    /// </summary>
    public void SetSwitch(bool on, bool showMessage)
    {
        switchOn = on;
        Refresh(showMessage);
    }

    /// <summary>
    /// カウントダウン終了時（タイマー開始時）に呼ぶ。
    /// </summary>
    /// <summary>
    /// カウントダウンの開始時に呼ぶ。少し遅れてゴーストも構える（Alert）。
    /// </summary>
    public void OnCountdownStart()
    {
        if (kamaeRoutine != null) StopCoroutine(kamaeRoutine);
        kamaeRoutine = StartCoroutine(KamaeAfterDelay());
    }

    private IEnumerator KamaeAfterDelay()
    {
        yield return new WaitForSeconds(kamaeDelay);
        kamaeRoutine = null;
        if (running || !IsVisible() || ghostAnimator == null || !ghostAnimator.isActiveAndEnabled) yield break;
        ghostAnimator.SetBool("walk", false);
        ghostAnimator.SetBool("move", false);
        ghostAnimator.SetBool("run", false);
        // Alert は move が立つまで次へ進まないので、スタートまで構えたまま待つ
        ghostAnimator.CrossFadeInFixedTime("Alert", 0.25f, 0);
    }

    public void StartRace()
    {
        if (kamaeRoutine != null)
        {
            StopCoroutine(kamaeRoutine);
            kamaeRoutine = null;
        }
        running = true;
        finished = false;
        if (IsVisible())
        {
            SetRunAnimation(true);
        }
    }

    /// <summary>
    /// タイマー実行中に毎フレーム呼ぶ。
    /// </summary>
    public void UpdateRace(float elapsed, int correctCount)
    {
        if (record == null) return;
        myCount = correctCount;
        ghostCount = record.GetCount(elapsed);
        lastElapsed = elapsed;
        UpdateAnimSpeed();
    }

    // ゴーストの今の打つペース（前後 1 秒の打鍵数から求めた 1 分あたりの打鍵数）に合わせて、アニメの速さを変える
    private void UpdateAnimSpeed()
    {
        if (ghostAnimator == null || !ghostAnimator.isActiveAndEnabled || record == null || !running) return;

        // こけている最中はこけるアニメ、起き上がったらまず歩く
        int fallKind = record.GetFallKind(lastElapsed);
        if (fallKind >= 0)
        {
            if (animState != AnimFall)
            {
                ghostAnimator.SetBool("walk", false);
                ghostAnimator.SetBool("move", false);
                ghostAnimator.SetBool("run", false);
                ghostAnimator.CrossFadeInFixedTime(fallStateNames[Mathf.Clamp(fallKind, 0, fallStateNames.Length - 1)], 0.1f, 0);
                animState = AnimFall;
                animChangedAt = Time.time;
            }
            return;
        }
        if (animState == AnimFall)
        {
            PlayAnimState(0, 0.25f);
        }

        float ratio = GetPaceRatio();
        int wanted = ChooseAnimState(ratio);
        if (wanted != animState && Time.time - animChangedAt >= animHoldTime)
        {
            PlayAnimState(wanted, 0.25f);
        }
        if (animState >= 0 && animState < AnimSpeedParams.Length)
        {
            ghostAnimator.SetFloat(AnimSpeedParams[animState], Mathf.Clamp(ratio, animSpeedMin, animSpeedMax));
        }
    }

    // 今のペース（前後 1 秒の打鍵数）÷ そのねこの平均ペース
    private float GetPaceRatio()
    {
        if (record == null || record.totalTime <= 0f || record.FinalCount <= 0) return 1f;
        float t0 = Mathf.Max(0f, lastElapsed - 0.5f);
        float t1 = Mathf.Min(record.totalTime, lastElapsed + 0.5f);
        if (t1 <= t0) return 1f;
        float pace = (record.GetCount(t1) - record.GetCount(t0)) / (t1 - t0);
        float average = record.FinalCount / record.totalTime;
        return pace / average;
    }

    // 速さの比からアニメを選ぶ。今のアニメの境目には少し余裕（0.05）を持たせ、行ったり来たりしないようにする
    private int ChooseAnimState(float ratio)
    {
        const float margin = 0.05f;
        float walkLine = walkBelowRatio + (animState == 0 ? margin : 0f);
        float runLine = runAboveRatio - (animState == 2 ? margin : 0f);
        if (ratio < walkLine) return 0;
        if (ratio > runLine) return 2;
        return 1;
    }

    // Animator の遷移（Run から Move へ戻れないなど）を通らず、直接そのアニメへ切り替える
    private void PlayAnimState(int state, float fade)
    {
        if (ghostAnimator == null || !ghostAnimator.isActiveAndEnabled) return;
        ghostAnimator.SetBool("walk", false);
        ghostAnimator.SetBool("move", false);
        ghostAnimator.SetBool("run", false);
        ghostAnimator.CrossFadeInFixedTime(AnimStateNames[state], fade, 0);
        animState = state;
        animChangedAt = Time.time;
    }

    /// <summary>
    /// 時間切れのときに呼ぶ。勝ち負けを表示する（保存はしない）。
    /// </summary>
    public void FinishRace(int correctCount)
    {
        if (!running || finished) return;
        running = false;
        finished = true;
        if (record == null) return;

        myCount = correctCount;
        ghostCount = record.FinalCount;
        SetRunAnimation(false);
        if (!IsVisible()) return;

        if (myCount > record.FinalCount) SetMessage(MsgWin);
        else if (myCount < record.FinalCount) SetMessage(MsgLose);
        else SetMessage(MsgDraw);
        messageHideTime = -1f;      // 結果は消さずに表示したままにする
    }

    /// <summary>
    /// ゴーストの走るアニメを始める・止める。
    /// 始めるときは、今の速さに合うアニメ（Walk・Move・Run）へ直接切り替える（以後は UpdateAnimSpeed で切り替える）。
    /// 止めるときは、終わりのしぐさ（end3）で締める。
    /// </summary>
    private void SetRunAnimation(bool run)
    {
        if (ghostAnimator == null || !ghostAnimator.isActiveAndEnabled) return;

        if (run)
        {
            // 出し直したときは Animator が最初の状態に戻っているので、今の速さに合うアニメから始める
            PlayAnimState(ChooseAnimState(GetPaceRatio()), 0.25f);   // 構えから走り出しへなめらかに
        }
        else if (animState >= 0)
        {
            animState = -1;
            ghostAnimator.SetTrigger("end3");   // 終わりのしぐさ（AnyState から）
        }
    }

    /// <summary>
    /// 自分のねこと同じコースを受け取る（タイマー開始時）。
    /// </summary>
    public void SetCourse(TypingCourse newCourse)
    {
        course = newCourse;
    }

    /// <summary>
    /// ゴーストを表示しているときは最終打鍵数、表示していないときは -1 を返す（コースの長さを決めるのに使う）。
    /// </summary>
    public float GetVisibleFinalCount()
    {
        return IsVisible() ? record.FinalCount : -1f;
    }

    private bool IsVisible()
    {
        return switchOn && timerMode && record != null;
    }

    // 現在の状態に合わせて、ゴーストとメッセージの表示を切り替える
    private void Refresh(bool showMessage)
    {
        if (ghostCat == null) return;

        bool visible = IsVisible();
        if (visible && !appearanceApplied)
        {
            ghostCat.SetActive(true);   // ChibiCat の Awake を先に実行させる
            ApplyAppearance();
            appearanceApplied = true;
        }
        ghostCat.SetActive(visible);

        if (visible)
        {
            SetRunAnimation(running);
            if (!finished) SetMessage("");
        }
        else
        {
            if (switchOn && timerMode && record == null && showMessage)
            {
                SetMessage(MsgNoGhost);
                messageHideTime = Time.time + noGhostMessageTime;
            }
            else if (!switchOn)
            {
                SetMessage("");
            }
        }
    }

    void Update()
    {
        if (messageHideTime > 0f && Time.time >= messageHideTime)
        {
            messageHideTime = -1f;
            SetMessage("");
        }
    }

    void LateUpdate()
    {
        if (ghostCat == null || !ghostCat.activeSelf || player == null) return;

        // 自分のねこと同じコースの上を、ゴーストの打鍵数で進む（コースができる前はスタート地点＝自分のねこの位置）
        Vector3 basePos = player.position;
        if (course != null)
        {
            basePos.x = course.XAt(ghostCount);
        }

        // カメラから見て自分のねこの奥へずらす
        Vector3 away = Vector3.forward;
        if (targetCamera != null)
        {
            away = player.position - targetCamera.transform.position;
            away.y = 0f;
            away = (away.sqrMagnitude > 0.0001f) ? away.normalized : Vector3.forward;
        }

        Vector3 pos = basePos + Vector3.right * sideOffset + away * depthOffset + Vector3.up * heightOffset;

        ghostCat.transform.position = pos;
        ghostCat.transform.rotation = player.rotation;
    }

    // ランキングで自分の 1 つ上のねこを探す（自分が 1 位なら 2 位）。見つからなければ null
    private ExRank FindTarget()
    {
        if (GameManager.guestMode || gm == null || gm.savedata == null) return null;
        List<ExRank> rankings = gm.savedata.ExRankings;
        string myUid = gm.savedata.Uid;
        int myKpm = gm.savedata.Status[st.Kpm];

#if UNITY_EDITOR
        // エディタでは Firebase からランキングが届かず、自分の Uid も空のため、ゴーストを確認できない。
        // ランキングに自分がいないときだけ、エディタ専用のテスト用ランキングを使う（WebGL ビルドには含まれない）。
        if (rankings == null || !rankings.Any(r => r != null && r.Uid == myUid))
        {
            rankings = CreateEditorTestRankings(myUid, myKpm);
            Debug.Log("GhostRunner: エディタ用のテストランキングを使用します。");
        }
#endif
        if (rankings == null || rankings.Count <= 1) return null;

        // 自分の Kpm は最新の値に置き換えて、Kpm の高い順に並べる（元のリストは変更しない）
        List<ExRank> sorted = rankings
            .Where(r => r != null)
            .OrderByDescending(r => r.Uid == myUid ? myKpm : r.Kpm)
            .ToList();

        int myIndex = sorted.FindIndex(r => r.Uid == myUid);
        if (myIndex < 0) return null;

        ExRank found = null;
        if (myIndex == 0)
        {
            if (sorted.Count > 1) found = sorted[1];
        }
        else
        {
            found = sorted[myIndex - 1];
        }
        if (found == null || found.Kpm <= 0) return null;
        return found;
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディタ専用のテスト用ランキングを作る。
    /// SaveData（ScriptableObject）の ExRankings には書き込まない（Play 中の変更がアセットに残るのを防ぐため）。
    /// 自分のほかに、自分より速いねこ 3 匹・遅いねこ 2 匹を入れる。体はダミーと同じ 201〜208 の範囲で変える。
    /// </summary>
    private List<ExRank> CreateEditorTestRankings(string myUid, int myKpm)
    {
        int baseKpm = (myKpm > 0) ? myKpm : 60;
        int[] offsets = { 45, 30, 15, -15, -30 };
        var list = new List<ExRank>();

        list.Add(new ExRank { Uid = myUid, FirstName = "editor_self", Kpm = baseKpm, CatBody = 201 });
        for (int i = 0; i < offsets.Length; i++)
        {
            list.Add(new ExRank
            {
                Uid = "editor_test_" + i,
                FirstName = "editor_test_" + i,
                Kpm = Mathf.Max(10, baseKpm + offsets[i]),
                CatBody = 202 + i,
            });
        }
        return list;
    }
#endif

    // 相手のねこの装備を反映し、半透明にする
    private void ApplyAppearance()
    {
        var chibiCat = ghostCat.GetComponentInChildren<ChibiCat>(true);
        if (chibiCat != null && target != null)
        {
            chibiCat.setChara(target.CatBody);
            int bagItem = (target.BackpackType == 25) ? 0x10 : 0;
            chibiCat.changeEquipHands(target.RightHand, target.LeftHand, bagItem);
            chibiCat.changeEquipHead(target.Head);
            chibiCat.changeEquipGlasses(target.Glasses);
        }

        // 名前は出さない
        foreach (var text in ghostCat.GetComponentsInChildren<TMP_Text>(true))
        {
            text.text = "";
        }

        if (ghostMaterial == null)
        {
            Debug.LogWarning("ゴースト対戦：半透明用のマテリアルが設定されていません。");
            return;
        }

        // 装備の表示を切り替えた後に、すべての Renderer のマテリアルを半透明用に差し替える
        // （非表示の装備も、あとで表示されたときのために差し替えておく）
        foreach (var rend in ghostCat.GetComponentsInChildren<Renderer>(true))
        {
            if (!(rend is MeshRenderer || rend is SkinnedMeshRenderer)) continue;
            if (rend.GetComponent<TMP_Text>() != null) continue;    // 3D 文字は対象外

            Material[] originals = rend.sharedMaterials;
            Material[] replaced = new Material[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                replaced[i] = CreateGhostMaterial(originals[i]);
            }
            rend.materials = replaced;
            rend.shadowCastingMode = ShadowCastingMode.Off;
            rend.receiveShadows = false;
        }
    }

    // 半透明用マテリアルを複製し、元のマテリアルのテクスチャと色を写す
    private Material CreateGhostMaterial(Material original)
    {
        Material mat = new Material(ghostMaterial);
        Color color = Color.white;
        if (original != null)
        {
            Texture tex = null;
            if (original.HasProperty("_MainTex")) tex = original.GetTexture("_MainTex");     // Toon シェーダー・Standard
            if (tex == null && original.HasProperty("_BaseMap")) tex = original.GetTexture("_BaseMap");
            if (tex != null)
            {
                mat.mainTexture = tex;
                if (original.HasProperty("_MainTex"))
                {
                    mat.mainTextureScale = original.GetTextureScale("_MainTex");
                    mat.mainTextureOffset = original.GetTextureOffset("_MainTex");
                }
            }

            if (original.HasProperty("_BaseColor")) color = original.GetColor("_BaseColor");
            else if (original.HasProperty("_Color")) color = original.GetColor("_Color");
        }
        color.a = ghostAlpha;
        mat.color = color;
        return mat;
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }
}
