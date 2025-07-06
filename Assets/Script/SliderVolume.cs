using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class SliderVolume : MonoBehaviour
{
    public Slider volumeSlider; // ボリュームスライダーの参照
    public Slider muteSlider; // ミュートスライダーの参照

    void Start()
    {
        // スライダーの初期状態に基づいて設定
        UpdateMute(muteSlider.value);

        // スライダーの値が変更された時に呼ばれるリスナーを設定
        muteSlider.onValueChanged.AddListener(delegate {
            UpdateMute(muteSlider.value);
        });
    }

    // スライダーの状態を更新するメソッド
    public void UpdateMute(float muteValue)
    {
        bool isMuted = muteValue == 1; // muteValueが1ならミュート、それ以外なら非ミュート
        volumeSlider.interactable = !isMuted; // ミュート状態に応じてインタラクションを制御
        Image fillAreaImage = volumeSlider.fillRect.GetComponent<Image>(); // Fill領域のImageコンポーネントを取得
        Color fillColor = isMuted ? new Color(0.5f, 0.5f, 0.5f, 1.0f) : new Color(0.4f, 0.9f, 0.53f, 1.0f); // グレーアウトまたは緑色
        fillAreaImage.color = fillColor; // Fill領域の色を設定
    }
}