using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCam : MonoBehaviour
{
    public Camera camera1;
    public Camera camera2;

    private AudioListener audioListener1;
    private AudioListener audioListener2;

    void Start()
    {
        camera2.enabled = false;

        // AudioListenerコンポーネントへの参照を取得
        audioListener1 = camera1.GetComponent<AudioListener>();
        audioListener2 = camera2.GetComponent<AudioListener>();

        // 最初のカメラセットアップ
        if (audioListener1 != null) audioListener1.enabled = true;
        if (audioListener2 != null) audioListener2.enabled = false;
    }

    public void SwitchCamera()
    {
        camera1.enabled = !camera1.enabled;
        camera2.enabled = !camera2.enabled;

        // AudioListenerの有効/無効を切り替える
        if (audioListener1 != null) audioListener1.enabled = camera1.enabled;
        if (audioListener2 != null) audioListener2.enabled = camera2.enabled;
    }
}