using UnityEngine;
using UnityEngine.UI;

public class AutoAuthTest : MonoBehaviour
{
    [SerializeField] private AutoAuthManager autoAuthManager;
    [SerializeField] private Button startAuthButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Text statusText;
    [SerializeField] private Text authInfoText;
    
    void Start()
    {
        Debug.Log("=== AutoAuthTest.Start() が呼ばれました ===");
        
        if (startAuthButton != null)
        {
            startAuthButton.onClick.AddListener(StartAutoAuth);
        }
        
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(Logout);
        }
        
        // イベントを登録
        autoAuthManager.OnAuthenticationSuccess += OnAuthSuccess;
        autoAuthManager.OnAuthenticationFailed += OnAuthFailed;
        
        UpdateStatus("自動認証テスト準備完了");
        UpdateAuthInfo("未認証");
        
        // Firebase初期化を開始
        if (autoAuthManager != null && autoAuthManager.FirebaseManager != null)
        {
            Debug.Log("FirebaseManagerが見つかりました。初期化を開始します。");
            UpdateStatus("Firebase初期化開始...");
            autoAuthManager.FirebaseManager.InitializeFirebase();
        }
        else
        {
            Debug.LogError("FirebaseManagerが見つかりません！");
            UpdateStatus("FirebaseManagerが見つかりません");
        }
        
        // 自動的に認証を開始しない
        // Debug.Log("AutoAuthTest: 自動認証開始...");
        // StartAutoAuth();
    }
    
    public void StartAutoAuth()
    {
        UpdateStatus("自動認証開始...");
        autoAuthManager.StartAutoAuthentication();
    }
    
    public void Logout()
    {
        autoAuthManager.Logout();
        UpdateStatus("ログアウトしました");
        UpdateAuthInfo("未認証");
    }
    
    private void OnAuthSuccess(AuthInfo authInfo)
    {
        string method = authInfo.AuthMethod.ToString();
        string displayName = authInfo.DisplayName ?? "不明";
        string email = authInfo.Email ?? "なし";
        
        UpdateStatus($"認証成功: {method}");
        UpdateAuthInfo($"方法: {method}\n名前: {displayName}\nメール: {email}\nゲスト: {(authInfo.IsGuest ? "はい" : "いいえ")}");
        
        Debug.Log($"認証成功 - 方法: {method}, 名前: {displayName}, メール: {email}");
    }
    
    private void OnAuthFailed(AuthResult result)
    {
        string resultText = result switch
        {
            AuthResult.Failed => "認証失敗",
            AuthResult.Cancelled => "認証キャンセル",
            AuthResult.NotSupported => "認証方法がサポートされていません",
            _ => "不明なエラー"
        };
        
        UpdateStatus($"認証失敗: {resultText}");
        UpdateAuthInfo("認証失敗");
        
        Debug.LogError($"認証失敗: {result}");
    }
    
    private void UpdateStatus(string message)
    {
        Debug.Log($"AutoAuthTest: {message}");
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
    
    private void UpdateAuthInfo(string info)
    {
        if (authInfoText != null)
        {
            authInfoText.text = info;
        }
    }
    
    void OnDestroy()
    {
        // イベントの登録解除
        if (autoAuthManager != null)
        {
            autoAuthManager.OnAuthenticationSuccess -= OnAuthSuccess;
            autoAuthManager.OnAuthenticationFailed -= OnAuthFailed;
        }
    }
} 