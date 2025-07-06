using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    public bool firstFadeInComp;

    [SerializeField]
    private float fadeInSpeed = 1;
    [SerializeField]
    private float fadeOutSpeed = 1;

    private Image img = null;
    private int frameCount = 0;
    private float timer = 0.0f;
    private bool fadeIn = false;
    private bool fadeOut = false;
    private bool compFadeIn = false;
    private bool compFadeOut = false;

    public void StartFadeIn()
    {
        if (fadeIn || fadeOut)
        {
            return;
        }
        fadeIn = true;
        compFadeIn = false;
        timer = 0.0f;
        img.color = new Color(1, 1, 1, 1);
        img.fillAmount = 1;
        img.raycastTarget = true;
    }

    public bool IsFadeInComplete()
    {
        return compFadeIn;
    }
    public void StartFadeOut()
    {
        if (fadeIn || fadeOut)
        {
            return;
        }
        fadeOut = true;
        compFadeOut = false;
        timer = 0.0f;
        img.color = new Color(1, 1, 1, 0);
        img.fillAmount = 1;
        img.raycastTarget = true;
    }

    public bool IsFadeOutComplete()
    {
        return compFadeOut;
    }

    // Start is called before the first frame update
    void Awake()
    {
        img = GetComponent<Image>();
        if (firstFadeInComp)
        {
            FadeInComplete();
        }
        else
        {
            firstFadeInComp = true;
            StartFadeIn();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (frameCount > 2)
        {
            if (fadeIn)
            {
                FadeInUpdate();
            }
            else if (fadeOut)
            {
                FadeOutUpdate();
            }
        }
        ++frameCount;

    }
    private void FadeInUpdate()
    {
        if (timer < 1f)
        {
            img.color = new Color(1, 1, 1, 1 - timer);
            img.fillAmount = 1 - timer;
        }
        else
        {
            FadeInComplete();
        }
        timer += Time.deltaTime * fadeInSpeed;
    }
    private void FadeOutUpdate()
    {
        if (timer < 1f)
        {
            img.color = new Color(1, 1, 1, timer);
            img.fillAmount = timer;
        }
        else
        {
            FadeOutComplete();
        }
        timer += Time.deltaTime * fadeOutSpeed ;
    }

    private void FadeInComplete()
    {
        img.color = new Color(1, 1, 1, 0);
        img.fillAmount = 1;
        img.raycastTarget = false;
        timer = 0.0f;
        fadeIn = false;
        compFadeIn = true;
    }

    private void FadeOutComplete()
    {
        img.color = new Color(1, 1, 1, 1);
        img.fillAmount = 1;
        img.raycastTarget = false;
        timer = 0.0f;
        fadeOut = false;
        compFadeOut = true;
        
#if !UNITY_EDITOR
        // 現在の時刻を取得
        int currentHour = System.DateTime.Now.Hour;

        // 許可された時間範囲内かどうかをチェック
        if (currentHour < GameManager.openHour || currentHour >= GameManager.closeHour)
        {
            GameManager.SceneNo = scene.Night;
            SceneManager.LoadScene("TitleScene"); // タイトルシーンに遷移
            return;
        }
#endif
    }
}
