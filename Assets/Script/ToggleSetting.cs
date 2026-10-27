using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class ToggleSetting : MonoBehaviour
{
    [SerializeField]
    private TMP_Text dispText;

    [SerializeField]
    private Slider slider;
    [SerializeField]
    private Setting setting;

    void Start()
    {
        // スライダーの値が変わらないと ChangeValue が呼ばれず、シーンに置いた文字のまま残ることがあるため、
        // 開始時にスライダーの値に合わせて文字を書いておく（音は鳴らさない）
        RefreshText();
    }

    public void ChangeValue()
    {
        setting.sayColtu(0);
        RefreshText();
    }

    // スライダーの値に合わせて ON / OFF の文字を書く
    private void RefreshText()
    {
        if (dispText == null || slider == null) return;
        dispText.text = (slider.value == 0) ? "OFF" : "ON";
    }

    public void tapButton()
    {
        if (slider.value == 0)
        {
            slider.value = 1;
        }
        else
        {
            slider.value = 0;
        }
        
    }
}
