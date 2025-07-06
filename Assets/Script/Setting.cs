using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UIコンポーネントを使用するために必要
using TMPro;

public class Setting : MonoBehaviour
{
    private bool isWindowShown = false;

    [SerializeField]
    private GameManager gm;
    public Slider volumeSlider;
    public Slider muteSlider;
    public Slider mailCharSlider;
    public Slider necoNumSlider;
    public Slider capitalSlider;
    public Slider keyTypeSlider;
    public GameObject toGas;
    private bool showFlg = false;
    public AudioSource worldAudio;  // AudioSource コンポーネントへの参照
    [SerializeField] private AudioClip outDoor;
    [SerializeField] private AudioClip knock;
    [SerializeField] private AudioClip windowSetting;
    [SerializeField] private AudioClip windowOpen;
    [SerializeField] private AudioClip itemGet;
    [SerializeField] private AudioClip lessMoney;
    [SerializeField] private AudioClip kyanseru;
    [SerializeField] private AudioClip windowClose;
    [SerializeField] private AudioClip coltu;

    // Start is called before the first frame update
    void Start()
    {
        toGas.SetActive(false);
        gm.npcManager.UpdateNPCCount(gm.savedata.Settings[se.CatNum]);
        initVolume();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void switchWindow()
    {
        if (gm.dragging)
        {
            return;
        }
        if (isWindowShown)
        {
            sayWindowClose();
            hide();
        }
        else
        {
            sayWindowSetting();
            show();
        }
    }

    public void show()
    {
        showFlg = true;
        volumeSlider.value = gm.savedata.Settings[se.Volume];
        muteSlider.value = gm.savedata.Settings[se.Mute];
        necoNumSlider.value = gm.savedata.Settings[se.CatNum];
        mailCharSlider.value = gm.savedata.Settings[se.MailChar];
        capitalSlider.value = gm.savedata.Settings[se.Capital];
        keyTypeSlider.value = gm.savedata.Settings[se.KeyType];

        // 画面サイズを都度取得しないと途中での最大化などに対応できない
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        Debug.Log("Width:" + screenWidth + "  Height:" + screenHeight);
        transform.position = new Vector2(screenWidth * 0.5f, screenHeight * 0.5f);
        isWindowShown = true; // 表示に設定
    }
    
    public void hide()
    {
        if (showFlg)
        {
            showFlg = false;
            gm.npcManager.UpdateNPCCount((int)necoNumSlider.value);

            gm.savedata.Settings[se.Volume] = (int)volumeSlider.value;
            gm.savedata.Settings[se.Mute] = (int)muteSlider.value;
            gm.savedata.Settings[se.CatNum] = (int)necoNumSlider.value;
            gm.savedata.Settings[se.MailChar] = (int)mailCharSlider.value;
            gm.savedata.Settings[se.Capital] = (int)capitalSlider.value;
            gm.savedata.Settings[se.KeyType] = (int)keyTypeSlider.value;
            
            // 画面サイズを都度取得しないと途中での最大化などに対応できない
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            Debug.Log("Width:" + screenWidth + "  Height:" + screenHeight);
            transform.position = new Vector2(screenWidth * 0.5f, screenHeight * 2);
            isWindowShown = false; // 非表示に設定
            gm.exportLocal();

            worldAudio.volume = volumeSlider.value * 0.01f;    // スライダー値をボリュームに
            if (muteSlider.value == 1)
            {
                worldAudio.mute = true;
            }
            else
            {
                worldAudio.mute = false;
            }
        }
    }

    public void updateVolume()
    {
        if (showFlg)
        {
            gm.savedata.Settings[se.Volume] = (int)volumeSlider.value;
            gm.savedata.Settings[se.Mute] = (int)muteSlider.value;
            worldAudio.volume = gm.savedata.Settings[se.Volume] * 0.01f;
            if (gm.savedata.Settings[se.Mute] == 1)
            {
                worldAudio.mute = true;
            }
            else
            {
                worldAudio.mute = false;
            }
        }
    }
    public void initVolume()
    {
        volumeSlider.value = gm.savedata.Settings[se.Volume];
        muteSlider.value = gm.savedata.Settings[se.Mute];
        necoNumSlider.value = gm.savedata.Settings[se.CatNum];
        mailCharSlider.value = gm.savedata.Settings[se.MailChar];
        capitalSlider.value = gm.savedata.Settings[se.Capital];
        keyTypeSlider.value = gm.savedata.Settings[se.KeyType];
        
        worldAudio.volume = gm.savedata.Settings[se.Volume] * 0.01f;
        if (gm.savedata.Settings[se.Mute] == 1)
        {
            worldAudio.mute = true;
        }
        else
        {
            worldAudio.mute = false;
        }
    }

    public void sayOutDoor()
    {
        worldAudio.PlayOneShot(outDoor);
    }
    public void sayKnock()
    {
        worldAudio.PlayOneShot(knock);
    }
    public void sayWindowSetting()
    {
        worldAudio.PlayOneShot(windowSetting);
    }
    public void sayWindowOpen()
    {
        worldAudio.PlayOneShot(windowOpen);
    }
    public void sayItemGet()
    {
        worldAudio.PlayOneShot(itemGet);
    }
    public void sayLessMoney()
    {
        worldAudio.PlayOneShot(lessMoney);
    }
    public void sayCancel()
    {
        worldAudio.PlayOneShot(kyanseru);
    }
    public void sayWindowClose()
    {
        worldAudio.PlayOneShot(windowClose);
    }
    public void sayColtu(int flg)
    {
        if (showFlg || flg == 1)
        {
            worldAudio.PlayOneShot(coltu);
        }
    }
}
