using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;  // UIコンポーネントを扱うために必要
using UnityEngine.EventSystems; // EventTriggerを使用するために必要

public class TypingVoice : MonoBehaviour, IPointerUpHandler
{
    public GameManager gm;
    public Image muteIcon; // インスペクターからアサイン
    public Sprite voiceSprite; // 音声ありの画像
    public Sprite muteSprite; // ミュートの画像
    public Slider slider;
    public bool changeFlg;
    public AudioSource typingAudio;  // タイピング音専用のAudioSource
    public AudioSource eventAudio;   // イベント音専用のAudioSource (追加)
    [SerializeField] private AudioClip nya;
    [SerializeField] private AudioClip[] dia;
    [SerializeField] private AudioClip dice;
    [SerializeField] private AudioClip coin;
    [SerializeField] private AudioClip coin3;
    [SerializeField] private AudioClip countDown;
    [SerializeField] private AudioClip coltu;

    // Start is called before the first frame update

    void Start()
    {
        // 1. typingAudio の確認と取得/追加
        if (typingAudio == null)
        {
            AudioSource[] existingAudioSources = GetComponents<AudioSource>();
            if (existingAudioSources.Length > 0)
            {
                // 既存のAudioSourceがあればそれをtypingAudioに設定
                typingAudio = existingAudioSources[0];
                Debug.LogWarning("TypingVoice: typingAudioがアサインされていなかったため、既存のAudioSourceを自動取得しました。");
            }
            else
            {
                // AudioSourceがなければ新しく追加
                typingAudio = gameObject.AddComponent<AudioSource>();
                Debug.LogWarning("TypingVoice: typingAudioがアサインされていなかったため、新しいAudioSourceを作成しました。");
            }
        }

        // 2. eventAudio の確認と取得/追加
        if (eventAudio == null)
        {
            // typingAudio とは別のAudioSourceを探す
            AudioSource[] allAudioSources = GetComponents<AudioSource>();
            bool foundSeparateAudioSource = false;
            foreach (AudioSource source in allAudioSources)
            {
                if (source != typingAudio)
                {
                    eventAudio = source;
                    foundSeparateAudioSource = true;
                    Debug.LogWarning("TypingVoice: eventAudioがアサインされていなかったため、別の既存AudioSourceを自動取得しました。");
                    break;
                }
            }

            // 別のAudioSourceが見つからなければ新しく追加
            if (!foundSeparateAudioSource)
            {
                eventAudio = gameObject.AddComponent<AudioSource>();
                Debug.LogWarning("TypingVoice: eventAudioがアサインされておらず、別のAudioSourceが見つからなかったため、新しいAudioSourceを作成しました。");
            }
        }
        
        int mute = gm.savedata.Settings[se.Mute];
        initVolume();
        gm.savedata.Settings[se.Mute] = mute;
        dispMute();
    }

    public void ToggleMute()
    {
        // ミュート状態を切り替える
        gm.savedata.Settings[se.Mute] = 1 - gm.savedata.Settings[se.Mute];
        dispMute();
    }

    private void dispMute()
    {
        // アイコンの更新
        if (gm.savedata.Settings[se.Mute] == 0) // ミュートでなければ
        {
            typingAudio.mute = false;             // ミュート解除
            muteIcon.sprite = voiceSprite;      // 口アイコン設定
            slider.fillRect.GetComponent<Image>().color = new Color(0.502848f, 0.7884344f, 0.9433962f, 1);
        }
        else
        {
            typingAudio.mute = true;             // ミュート
            muteIcon.sprite = muteSprite;       // マスクアイコン設定
            slider.fillRect.GetComponent<Image>().color = new Color(0.7075472f, 0.5416017f, 0.4438857f, 1);
        }
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);    // EventSystemのフォーカスをクリア
    }
    public void updateVolume()      // ボリューム変更時
    {
        gm.savedata.Settings[se.Mute] = 0;      // ミュート解除
        gm.savedata.Settings[se.Volume] = (int)slider.value;    // スライダー値をセーブデータに代入
        typingAudio.volume = slider.value * 0.01f;    // スライダー値をボリュームに
        typingAudio.mute = false;             // ミュート解除
        
        changeFlg = true;
        dispMute();
    }
    // ポインタが離れたときに呼ばれる関数（IPointerUpHandlerインターフェースの実装）
    public void OnPointerUp(PointerEventData eventData)
    {
        sayColtu();
        // ミュート状態を切り替える
        gm.savedata.Settings[se.Mute] = 1 - gm.savedata.Settings[se.Mute];
        dispMute();
    }
    public void initVolume()
    {
        slider.value = gm.savedata.Settings[se.Volume];
    }

    public void sayNya()
    {
        if (typingAudio != null && nya != null)
        {
            typingAudio.Stop(); // 現在再生中の音を停止
            typingAudio.PlayOneShot(nya);
        }
        else
        {
            Debug.LogWarning("TypingVoice: nya AudioClip または typingAudio がアサインされていません。");
        }
    }
    public void sayDia(int no)
    {
        if (typingAudio != null && dia != null && no >= 0 && no < dia.Length && dia[no] != null)
        {
            typingAudio.Stop(); // 現在再生中の音を停止
            typingAudio.PlayOneShot(dia[no]);
        }
        else
        {
            Debug.LogWarning($"TypingVoice: dia AudioClip (no:{no}) または typingAudio がアサインされていないか、インデックスが無効です。");
        }
    }
    public void sayDice()
    {
        if (eventAudio != null && dice != null)
        {
            eventAudio.PlayOneShot(dice);
        }
        else
        {
            Debug.LogWarning("TypingVoice: dice AudioClip または eventAudio がアサインされていません。");
        }
    }
    public void sayCoin()
    {
        if (eventAudio != null && coin != null)
        {
            eventAudio.PlayOneShot(coin);
        }
        else
        {
            Debug.LogWarning("TypingVoice: coin AudioClip または eventAudio がアサインされていません。");
        }
    }
    public void sayCoin3()
    {
        if (eventAudio != null && coin3 != null)
        {
            eventAudio.PlayOneShot(coin3);
        }
        else
        {
            Debug.LogWarning("TypingVoice: coin3 AudioClip または eventAudio がアサインされていません。");
        }
    }
    public void sayCountDown()
    {
        if (eventAudio != null && countDown != null)
        {
            eventAudio.PlayOneShot(countDown);
        }
        else
        {
            Debug.LogWarning("TypingVoice: countDown AudioClip または eventAudio がアサインされていません。");
        }
    }
    public void sayColtu()
    {
        if (eventAudio != null && coltu != null)
        {
            eventAudio.PlayOneShot(coltu);
        }
        else
        {
            Debug.LogWarning("TypingVoice: coltu AudioClip または eventAudio がアサインされていません。");
        }
    }
}