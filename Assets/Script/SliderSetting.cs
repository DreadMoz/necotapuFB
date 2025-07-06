using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // EventTriggerを使用するために必要
using TMPro;

public class SliderSetting : MonoBehaviour, IPointerUpHandler
{
    [SerializeField]
    private TMP_Text dispText;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Setting setting;
    private bool changeFlg = false;

    void Start()
    {
        slider.value = float.Parse(dispText.text);
    }

    public void ChangeValue()
    {
        dispText.text = slider.value.ToString();
        changeFlg = true;
    }
    // ポインタが離れたときに呼ばれる関数（IPointerUpHandlerインターフェースの実装）
    public void OnPointerUp(PointerEventData eventData)
    {
        if (changeFlg)
        {
            changeFlg = false;
            setting.sayColtu(0);
        }
    }
}
