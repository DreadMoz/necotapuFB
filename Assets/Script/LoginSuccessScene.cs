using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// ログイン成功画面
/// </summary>
public class LoginSuccessScene : MonoBehaviour
{
    [Header("UI要素")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI userInfoText;
    [SerializeField] private Button backToTitleButton;
    [SerializeField] private Button continueButton;

    private necotapuFB.AuthInfo currentAuthInfo;

    private void Start()
    {
        Debug.Log("ログイン成功画面を開始");
        
        // 保存された認証情報を取得
        LoadAuthInfo();
        
        // UIを設定
        SetupUI();
        
        // ボタンイベントを設定
        SetupButtons();
    }

    /// <summary>
    /// 認証情報を読み込み
    /// </summary>
    private void LoadAuthInfo()
    {
        try
        {
            string savedAuthInfo = PlayerPrefs.GetString("FirebaseAuthInfo", "");
            if (!string.IsNullOrEmpty(savedAuthInfo))
            {
                currentAuthInfo = JsonUtility.FromJson<necotapuFB.AuthInfo>(savedAuthInfo);
                Debug.Log($"認証情報を読み込み: {currentAuthInfo.authMethod} - {currentAuthInfo.email}");
            }
            else
            {
                Debug.LogWarning("保存された認証情報が見つかりません");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"認証情報の読み込みに失敗: {e.Message}");
        }
    }

    /// <summary>
    /// UIを設定
    /// </summary>
    private void SetupUI()
    {
        if (messageText != null)
        {
            messageText.text = "✅ ログインしました";
            messageText.color = Color.green;
        }

        if (userInfoText != null && currentAuthInfo != null)
        {
            string userInfo = $"認証方法: {currentAuthInfo.authMethod}\n";
            if (!string.IsNullOrEmpty(currentAuthInfo.email))
            {
                userInfo += $"メール: {currentAuthInfo.email}\n";
            }
            if (!string.IsNullOrEmpty(currentAuthInfo.displayName))
            {
                userInfo += $"表示名: {currentAuthInfo.displayName}\n";
            }
            userInfo += $"ユーザーID: {currentAuthInfo.userId}";
            
            userInfoText.text = userInfo;
        }
    }

    /// <summary>
    /// ボタンイベントを設定
    /// </summary>
    private void SetupButtons()
    {
        if (backToTitleButton != null)
        {
            backToTitleButton.onClick.AddListener(() => {
                Debug.Log("タイトル画面に戻る");
                SceneManager.LoadScene("TitleScene");
            });
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(() => {
                Debug.Log("メイン画面に進む");
                SceneManager.LoadScene("WorldScene");
            });
        }
    }
} 