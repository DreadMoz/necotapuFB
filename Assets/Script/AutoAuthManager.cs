using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 3段階自動認証マネージャー
/// 1. Google認証（最優先）
/// 2. Microsoft認証（次優先）
/// 3. 匿名認証（フォールバック）
/// </summary>
public class AutoAuthManager : MonoBehaviour
{
    [SerializeField] private FirebaseManager firebaseManager;
    [SerializeField] private FirebaseConfig firebaseConfig;
    
    // FirebaseManagerを取得するためのプロパティ
    public FirebaseManager FirebaseManager => firebaseManager;
    
    // 認証状態
    public AuthInfo CurrentAuthInfo { get; private set; }
    public bool IsAuthenticated { get; private set; } = false;
    
    // イベント
    public event Action<AuthInfo> OnAuthenticationSuccess;
    public event Action<AuthResult> OnAuthenticationFailed;
    
    // 認証試行中フラグ
    private bool isAuthenticating = false;
    
    void Start()
    {
        // 起動時は自動認証しない
        // StartAutoAuthentication();
    }

    /// <summary>
    /// ボタン等から明示的に呼び出す用の認証開始メソッド
    /// </summary>
    public void StartAuthFromButton()
    {
        StartAutoAuthentication();
    }
    
    /// <summary>
    /// 自動認証を開始
    /// </summary>
    public void StartAutoAuthentication()
    {
        if (isAuthenticating)
        {
            Debug.LogWarning("認証が既に進行中です");
            return;
        }
        
        Debug.Log("自動認証開始");
        isAuthenticating = true;
        
        // 1. Firebase初期化
        InitializeFirebase();
    }
    
    /// <summary>
    /// Firebase初期化
    /// </summary>
    private void InitializeFirebase()
    {
        Debug.Log("Firebase初期化開始...");
        
        // FirebaseManagerを確実に取得
        if (firebaseManager == null)
        {
            firebaseManager = FirebaseManager.Instance;
            Debug.Log($"FirebaseManagerを動的に取得: {firebaseManager.name}");
        }
        
        if (firebaseManager == null)
        {
            Debug.LogError("FirebaseManagerが設定されていません");
            OnAuthenticationFailed?.Invoke(AuthResult.Failed);
            isAuthenticating = false;
            return;
        }
        
        if (firebaseConfig == null)
        {
            Debug.LogError("FirebaseConfigが設定されていません");
            OnAuthenticationFailed?.Invoke(AuthResult.Failed);
            isAuthenticating = false;
            return;
        }
        
        Debug.Log($"FirebaseManager: {firebaseManager.name}");
        Debug.Log($"FirebaseConfig: {firebaseConfig.name}");
        
        firebaseManager.InitializeFirebase();
        
        // 初期化完了を待ってから認証を開始
        StartCoroutine(WaitForFirebaseInitialization());
    }
    
    /// <summary>
    /// Firebase初期化完了を待つ
    /// </summary>
    private IEnumerator WaitForFirebaseInitialization()
    {
        float timeout = 30f; // 30秒に延長
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            if (firebaseManager.IsInitialized)
            {
                Debug.Log("Firebase初期化完了。認証を開始します");
                // 1. Google認証を試行
                TryGoogleAuthentication();
                yield break;
            }
            
            elapsed += Time.deltaTime;
            if (elapsed % 5f < Time.deltaTime) // 5秒ごとにログ
            {
                Debug.Log($"Firebase初期化待機中... {elapsed:F1}/{timeout}秒");
            }
            yield return null;
        }
        
        // タイムアウト
        Debug.LogError("Firebase初期化がタイムアウトしました");
        OnAuthenticationFailed?.Invoke(AuthResult.Failed);
        isAuthenticating = false;
    }
    
    /// <summary>
    /// Google認証を試行
    /// </summary>
    private void TryGoogleAuthentication()
    {
        if (!firebaseConfig.enableGoogleAuth)
        {
            Debug.Log("Google認証が無効です。Microsoft認証を試行します");
            TryMicrosoftAuthentication();
            return;
        }
        
        Debug.Log("Google認証を試行中...");
        firebaseManager.SignInWithGoogle();
        
        // 5秒後に結果をチェック
        StartCoroutine(CheckAuthResult(AuthPriority.Google, () => TryMicrosoftAuthentication()));
    }
    
    /// <summary>
    /// Microsoft認証を試行
    /// </summary>
    private void TryMicrosoftAuthentication()
    {
        if (!firebaseConfig.enableMicrosoftAuth)
        {
            Debug.Log("Microsoft認証が無効です。匿名認証を試行します");
            TryAnonymousAuthentication();
            return;
        }
        
        Debug.Log("Microsoft認証を試行中...");
        firebaseManager.SignInWithMicrosoft();
        
        // 5秒後に結果をチェック
        StartCoroutine(CheckAuthResult(AuthPriority.Microsoft, () => TryAnonymousAuthentication()));
    }
    
    /// <summary>
    /// 匿名認証を試行
    /// </summary>
    private void TryAnonymousAuthentication()
    {
        if (!firebaseConfig.enableAnonymousAuth)
        {
            Debug.LogError("すべての認証方法が無効です");
            OnAuthenticationFailed?.Invoke(AuthResult.NotSupported);
            isAuthenticating = false;
            return;
        }
        
        Debug.Log("匿名認証を試行中...");
        firebaseManager.SignInAnonymously();
        
        // 5秒後に結果をチェック
        StartCoroutine(CheckAuthResult(AuthPriority.Anonymous, () => {
            Debug.LogError("すべての認証が失敗しました");
            OnAuthenticationFailed?.Invoke(AuthResult.Failed);
            isAuthenticating = false;
        }));
    }
    
    /// <summary>
    /// 認証結果をチェック
    /// </summary>
    private IEnumerator CheckAuthResult(AuthPriority authMethod, Action onTimeout)
    {
        float timeout = 5f;
        float elapsed = 0f;
        
        while (elapsed < timeout)
        {
            if (IsAuthenticated)
            {
                // 認証成功
                Debug.Log($"{authMethod}認証が成功しました");
                isAuthenticating = false;
                yield break;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // タイムアウト
        Debug.Log($"{authMethod}認証がタイムアウトしました");
        onTimeout?.Invoke();
    }
    
    /// <summary>
    /// 認証成功時の処理
    /// </summary>
    public void OnAuthSuccess(string authData)
    {
        try
        {
            var authInfo = JsonConvert.DeserializeObject<AuthInfo>(authData);
            CurrentAuthInfo = authInfo;
            IsAuthenticated = true;
            isAuthenticating = false;
            
            Debug.Log($"認証成功: {authInfo.AuthMethod} - {authInfo.DisplayName}");
            OnAuthenticationSuccess?.Invoke(authInfo);
        }
        catch (Exception ex)
        {
            Debug.LogError($"認証情報の解析に失敗: {ex.Message}");
            OnAuthenticationFailed?.Invoke(AuthResult.Failed);
        }
    }
    
    /// <summary>
    /// 認証失敗時の処理
    /// </summary>
    public void OnAuthFailed(string error)
    {
        Debug.LogError($"認証失敗: {error}");
        OnAuthenticationFailed?.Invoke(AuthResult.Failed);
        isAuthenticating = false;
    }
    
    /// <summary>
    /// 認証キャンセル時の処理
    /// </summary>
    public void OnAuthCancelled()
    {
        Debug.Log("認証がキャンセルされました");
        OnAuthenticationFailed?.Invoke(AuthResult.Cancelled);
        isAuthenticating = false;
    }
    
    /// <summary>
    /// ログアウト
    /// </summary>
    public void Logout()
    {
        CurrentAuthInfo = null;
        IsAuthenticated = false;
        Debug.Log("ログアウトしました");
    }
    
    /// <summary>
    /// Firestore書き込み成功時の処理
    /// </summary>
    public void OnFirestoreWriteSuccess(string resultData)
    {
        try
        {
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(resultData);
            Debug.Log("✅ Firestore書き込み成功！");
            if (result.ContainsKey("documentId"))
            {
                Debug.Log($"ドキュメントID: {result["documentId"]}");
            }
            if (result.ContainsKey("data"))
            {
                Debug.Log($"データ: {result["data"]}");
            }
            Debug.Log("Firebaseアクセス証明完了！");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firestore結果の解析に失敗: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Firestore書き込み失敗時の処理
    /// </summary>
    public void OnFirestoreWriteFailed(string error)
    {
        Debug.LogError($"❌ Firestore書き込み失敗: {error}");
        Debug.LogError("Firebaseアクセス失敗の証明");
    }
} 