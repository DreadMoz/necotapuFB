using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// シンプルな認証テスト
/// </summary>
public class SimpleAuthTest : MonoBehaviour
{
    [Header("認証ボタン")]
    [SerializeField] private Button googleAuthButton;
    [SerializeField] private Button microsoftAuthButton;
    [SerializeField] private Button guestAuthButton;
    [SerializeField] private Button logoutButton;

    [Header("認証情報表示")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI userInfoText;
    [SerializeField] private TextMeshProUGUI authMethodText;

    [Header("ローディング")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("エラーメッセージ")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorText;

    private necotapuFB.AuthManager authManager;

    private void Start()
    {
        Debug.Log("シンプル認証テスト開始");
        
        // AuthManagerを取得または作成
        authManager = FindObjectOfType<necotapuFB.AuthManager>();
        if (authManager == null)
        {
            Debug.Log("AuthManagerが見つからないため作成します");
            GameObject authManagerObj = new GameObject("AuthManager");
            authManager = authManagerObj.AddComponent<necotapuFB.AuthManager>();
        }

        // イベントを登録
        authManager.OnAuthenticationSuccess += OnAuthSuccess;
        authManager.OnAuthenticationFailed += OnAuthFailed;
        authManager.OnLogout += OnLogout;

        // ボタンイベントを設定
        SetupButtons();

        // 初期状態を設定
        UpdateUI();
        
        // Firebase設定を送信
        SendFirebaseConfig();
        
        Debug.Log("シンプル認証テスト初期化完了");
    }

    /// <summary>
    /// Firebase設定をJavaScript側に送信
    /// </summary>
    private void SendFirebaseConfig()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            // Firebase設定を直接JSON文字列で作成
            string configJson = @"{
                ""apiKey"": ""AIzaSyAlz5pYr-M5MzY_es94f2zNphBBMiJ5TXs"",
                ""authDomain"": ""necotapufb.firebaseapp.com"",
                ""projectId"": ""necotapufb"",
                ""storageBucket"": ""necotapufb.firebasestorage.app"",
                ""messagingSenderId"": ""794942101336"",
                ""appId"": ""1:794942101336:web:e244937b77bb056a43772d"",
                ""databaseURL"": ""https://necotapufb.firebaseio.com""
            }";
            
            Debug.Log($"Firebase設定を送信: {configJson}");
            
            // JavaScript側に設定を送信
            setFirebaseConfigJslib(configJson);
        #else
            Debug.Log("エディタ環境: Firebase設定送信をスキップ");
        #endif
    }

    // JavaScript側との連携メソッド
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void setFirebaseConfigJslib(string config);

    /// <summary>
    /// Firebase初期化完了時のコールバック
    /// </summary>
    public void OnFirebaseInitialized(string result)
    {
        Debug.Log($"Firebase初期化完了: {result}");
        if (result == "success")
        {
            Debug.Log("Firebase初期化成功");
            // 初期化成功時の処理
        }
        else
        {
            Debug.LogError("Firebase初期化失敗");
            ShowError("Firebase初期化に失敗しました");
        }
    }

    /// <summary>
    /// ボタンイベントを設定
    /// </summary>
    private void SetupButtons()
    {
        if (googleAuthButton != null)
        {
            googleAuthButton.onClick.AddListener(() => {
                Debug.Log("=== Google認証ボタンがクリックされました ===");
                Debug.Log($"AuthManager: {authManager}");
                Debug.Log($"AuthManager.IsAuthenticated: {authManager.IsAuthenticated}");
                Debug.Log($"AuthManager.IsAuthenticating: {authManager.IsAuthenticating}");
                authManager.SignInWithGoogle();
            });
        }

        if (microsoftAuthButton != null)
        {
            microsoftAuthButton.onClick.AddListener(() => {
                Debug.Log("Microsoft認証ボタンがクリックされました");
                authManager.SignInWithMicrosoft();
            });
        }

        if (guestAuthButton != null)
        {
            guestAuthButton.onClick.AddListener(() => {
                Debug.Log("ゲスト認証ボタンがクリックされました");
                authManager.SignInAsGuest();
            });
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(() => {
                Debug.Log("ログアウトボタンがクリックされました");
                authManager.Logout();
            });
        }
    }

    /// <summary>
    /// 認証成功時の処理
    /// </summary>
    private void OnAuthSuccess(necotapuFB.AuthInfo authInfo)
    {
        Debug.Log($"認証成功: {authInfo.authMethod} - {authInfo.email}");
        
        // ローディングを非表示
        SetLoading(false);
        
        // エラーパネルを非表示
        SetError(false);
        
        // UIを更新
        UpdateUI();
        
        // 認証情報を表示
        ShowAuthInfo(authInfo);
        
        // ステータスを更新
        if (statusText != null)
        {
            statusText.text = $"✅ 認証成功: {authInfo.authMethod}";
            statusText.color = Color.green;
        }
    }

    /// <summary>
    /// 認証失敗時の処理
    /// </summary>
    private void OnAuthFailed(necotapuFB.AuthResult result)
    {
        Debug.Log($"認証失敗: {result}");
        
        // ローディングを非表示
        SetLoading(false);
        
        // エラーメッセージを表示
        string errorMessage = GetErrorMessage(result);
        ShowError(errorMessage);
        
        // ステータスを更新
        if (statusText != null)
        {
            statusText.text = $"❌ 認証失敗: {result}";
            statusText.color = Color.red;
        }
    }

    /// <summary>
    /// ログアウト時の処理
    /// </summary>
    private void OnLogout()
    {
        Debug.Log("ログアウト完了");
        
        // UIを更新
        UpdateUI();
        
        // 認証情報を非表示
        HideAuthInfo();
        
        // ステータスを更新
        if (statusText != null)
        {
            statusText.text = "ℹ️ 未認証状態";
            statusText.color = Color.white;
        }
    }

    /// <summary>
    /// UIを更新
    /// </summary>
    private void UpdateUI()
    {
        bool isAuthenticated = authManager.IsAuthenticated;
        bool isAuthenticating = authManager.IsAuthenticating;

        // 認証ボタンの有効/無効を設定
        SetAuthButtonsEnabled(!isAuthenticated && !isAuthenticating);

        // ログアウトボタンの有効/無効を設定
        if (logoutButton != null)
        {
            logoutButton.interactable = isAuthenticated && !isAuthenticating;
        }

        // ローディングの表示/非表示
        SetLoading(isAuthenticating);
    }

    /// <summary>
    /// 認証ボタンの有効/無効を設定
    /// </summary>
    private void SetAuthButtonsEnabled(bool enabled)
    {
        if (googleAuthButton != null)
        {
            googleAuthButton.interactable = enabled;
        }

        if (microsoftAuthButton != null)
        {
            microsoftAuthButton.interactable = enabled;
        }

        if (guestAuthButton != null)
        {
            guestAuthButton.interactable = enabled;
        }
    }

    /// <summary>
    /// ローディングの表示/非表示
    /// </summary>
    private void SetLoading(bool show)
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(show);
        }

        if (loadingText != null)
        {
            loadingText.text = show ? "認証中..." : "";
        }
    }

    /// <summary>
    /// エラーメッセージを表示
    /// </summary>
    private void ShowError(string message)
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(true);
        }

        if (errorText != null)
        {
            errorText.text = message;
        }

        // 3秒後にエラーパネルを非表示
        Invoke(nameof(HideError), 3f);
    }

    /// <summary>
    /// エラーパネルを非表示
    /// </summary>
    private void HideError()
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(false);
        }
    }

    /// <summary>
    /// エラーパネルを非表示
    /// </summary>
    private void SetError(bool show)
    {
        if (errorPanel != null)
        {
            errorPanel.SetActive(show);
        }
    }

    /// <summary>
    /// 認証情報を表示
    /// </summary>
    private void ShowAuthInfo(necotapuFB.AuthInfo authInfo)
    {
        if (userInfoText != null)
        {
            string userInfo = $"ユーザーID: {authInfo.userId}\n";
            userInfo += $"メール: {authInfo.email}\n";
            userInfo += $"表示名: {authInfo.displayName}";
            userInfoText.text = userInfo;
        }

        if (authMethodText != null)
        {
            string methodText = $"認証方法: {authInfo.authMethod}";
            if (authInfo.isGuest)
            {
                methodText += " (ゲスト)";
            }
            authMethodText.text = methodText;
        }
    }

    /// <summary>
    /// 認証情報を非表示
    /// </summary>
    private void HideAuthInfo()
    {
        if (userInfoText != null)
        {
            userInfoText.text = "";
        }

        if (authMethodText != null)
        {
            authMethodText.text = "";
        }
    }

    /// <summary>
    /// エラーメッセージを取得
    /// </summary>
    private string GetErrorMessage(necotapuFB.AuthResult result)
    {
        switch (result)
        {
            case necotapuFB.AuthResult.Cancelled:
                return "認証がキャンセルされました";
            case necotapuFB.AuthResult.NotSupported:
                return "この認証方法はサポートされていません";
            case necotapuFB.AuthResult.Failed:
            default:
                return "認証に失敗しました";
        }
    }

    private void OnDestroy()
    {
        // イベントを解除
        if (authManager != null)
        {
            authManager.OnAuthenticationSuccess -= OnAuthSuccess;
            authManager.OnAuthenticationFailed -= OnAuthFailed;
            authManager.OnLogout -= OnLogout;
        }
    }
} 