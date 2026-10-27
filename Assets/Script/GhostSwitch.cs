using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// タイピング画面の「ゴースト表示」スイッチ。
/// 画面の開始時は保存値に合わせて表示を変えるだけで、設定値は書き換えない。
/// スイッチを動かしたときだけ Settings[se.Ghost] を書き換える。
/// 保存（saveGameData）はここでは呼ばず、ステージクリア時などの次の保存にまとめる（Firestore の書き込みを増やさないため）。
/// </summary>
public class GhostSwitch : MonoBehaviour
{
    [SerializeField] private GameManager gm;
    [SerializeField] private Slider slider;
    [SerializeField] private GhostRunner ghostRunner;
    [SerializeField] private TypingVoice voice;

    // Start で保存値を表示に反映するまでは true にならない。
    // エディタでは Slider.Rebuild が Start より前に OnValueChanged を呼ぶことがあり、
    // そのときシーンの初期値で設定値を上書きしないようにするためのフラグ。
    private bool initialized = false;

    void Start()
    {
        int saved = gm.savedata.Settings[se.Ghost];
        slider.SetValueWithoutNotify(saved);    // OnValueChanged を呼ばずに表示だけ合わせる
        if (ghostRunner != null)
        {
            ghostRunner.SetSwitch(saved == 1, true);
        }
        initialized = true;
    }

    /// <summary>
    /// スライダーの OnValueChanged から呼ばれる。
    /// </summary>
    public void OnSwitchChanged()
    {
        if (!initialized)
        {
            return;     // 開始前の自動呼び出しは無視する（設定値を書き換えない）
        }
        int value = (int)slider.value;
        if (gm.savedata.Settings[se.Ghost] == value)
        {
            return;
        }
        gm.savedata.Settings[se.Ghost] = value;
        if (voice != null)
        {
            voice.sayColtu();
        }
        if (ghostRunner != null)
        {
            ghostRunner.SetSwitch(value == 1, true);
        }
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);    // EventSystemのフォーカスをクリア
        }
    }
}
