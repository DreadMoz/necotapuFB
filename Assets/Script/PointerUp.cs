using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // EventTriggerを使用するために必要

public class PointerUp : MonoBehaviour, IPointerUpHandler
{
    public TypingVoice typingVoice;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // ポインタが離れたときに呼ばれる関数（IPointerUpHandlerインターフェースの実装）
    public void OnPointerUp(PointerEventData eventData)
    {
        if (typingVoice.changeFlg)
        {
            typingVoice.changeFlg = false;
            typingVoice.sayColtu();
        }
    }
}
