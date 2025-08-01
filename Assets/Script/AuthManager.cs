using UnityEngine;
using System;
using System.Collections.Generic;

namespace necotapuFB
{
    /// <summary>
    /// 認証結果
    /// </summary>
    public enum AuthResult
    {
        Success,
        Failed,
        Cancelled,
        NotSupported
    }

    /// <summary>
    /// 認証方法
    /// </summary>
    public enum AuthMethod
    {
        Google,
        Microsoft,
        Guest
    }

    /// <summary>
    /// 認証情報
    /// </summary>
    [System.Serializable]
    public class AuthInfo
    {
        public string userId;
        public string email;
        public string displayName;
        public string authMethod;
        public bool isGuest;
        public string authenticatedAt;
    }

    /// <summary>
    /// 3種類認証マネージャー
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        [Header("認証設定")]
        [SerializeField] private bool enableGoogleAuth = true;
        [SerializeField] private bool enableMicrosoftAuth = false; // 現在無効
        [SerializeField] private bool enableGuestAuth = true;

        [Header("認証状態")]
        [SerializeField] private bool isAuthenticated = false;
        [SerializeField] private AuthInfo currentAuthInfo;

        // イベント
        public event Action<AuthInfo> OnAuthenticationSuccess;
        public event Action<AuthResult> OnAuthenticationFailed;
        public event Action OnLogout;

        // 認証中フラグ
        private bool isAuthenticating = false;
        
        // 認証状態の永続化キー
        private const string AUTH_INFO_KEY = "FirebaseAuthInfo";
        private const string AUTH_TIMESTAMP_KEY = "FirebaseAuthTimestamp";

        // プロパティ
        public bool IsAuthenticated => isAuthenticated;
        public AuthInfo CurrentAuthInfo => currentAuthInfo;
        public bool IsAuthenticating => isAuthenticating;

        private void Start()
        {
            Debug.Log("AuthManager初期化開始");
            
            // 新しいアーキテクチャでは、認証はindex.htmlで行われるため、
            // Unity起動時は認証状態の復元のみを行う
            RestoreAuthState();
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                // WebGL環境での初期化（認証はindex.htmlで行われるため、最小限の処理）
                InitializeWebGLAuth();
            #else
                // エディタ環境ではスキップ
                Debug.Log("エディタ環境: 認証をスキップ");
            #endif
        }

        /// <summary>
        /// 保存された認証状態を復元
        /// </summary>
        private void RestoreAuthState()
        {
            try
            {
                string savedAuthInfo = PlayerPrefs.GetString(AUTH_INFO_KEY, "");
                string savedTimestamp = PlayerPrefs.GetString(AUTH_TIMESTAMP_KEY, "");
                
                if (!string.IsNullOrEmpty(savedAuthInfo) && !string.IsNullOrEmpty(savedTimestamp))
                {
                    long timestamp = long.Parse(savedTimestamp);
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    double hoursSinceAuth = (now - timestamp) / (1000.0 * 60 * 60);
                    
                    Debug.Log($"保存された認証情報をチェック中...");
                    Debug.Log($"認証時刻: {DateTimeOffset.FromUnixTimeMilliseconds(timestamp)}");
                    Debug.Log($"経過時間: {hoursSinceAuth:F2}時間");
                    
                    // 24時間以内の認証情報は有効とする
                    if (hoursSinceAuth < 24)
                    {
                        currentAuthInfo = JsonUtility.FromJson<AuthInfo>(savedAuthInfo);
                        isAuthenticated = true;
                        
                        Debug.Log($"認証状態を復元: {currentAuthInfo.authMethod} - {currentAuthInfo.email}");
                        
                        // 認証成功イベントを発火
                        OnAuthenticationSuccess?.Invoke(currentAuthInfo);
                        
                        // 認証状態復元完了 - 新しいアーキテクチャでは画面遷移は不要
                        Debug.Log("認証状態復元完了 - 新しいアーキテクチャでは画面遷移は不要");
                    }
                    else
                    {
                        Debug.Log("認証情報が古いため削除");
                        ClearAuthState();
                    }
                }
                else
                {
                    Debug.Log("保存された認証情報なし");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"認証状態の復元に失敗: {e.Message}");
                ClearAuthState();
            }
        }
        
        /// <summary>
        /// 認証状態復元後に適切な画面に遷移
        /// </summary>
        private System.Collections.IEnumerator TransitionToAuthenticatedScene()
        {
            Debug.Log("認証状態復元後の画面遷移を開始");
            
            // 少し待機してから遷移
            yield return new WaitForSeconds(0.5f);
            
            // 現在のシーン名を取得
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"現在のシーン: {currentSceneName}");
            
            // 認証済みの場合の遷移先を決定
            string authenticatedSceneName = DetermineAuthenticatedScene(currentSceneName);
            Debug.Log($"認証済み遷移先シーン: {authenticatedSceneName}");
            
            if (!string.IsNullOrEmpty(authenticatedSceneName) && authenticatedSceneName != currentSceneName)
            {
                // シーン遷移を実行
                UnityEngine.SceneManagement.SceneManager.LoadScene(authenticatedSceneName);
            }
            else
            {
                Debug.Log("既に適切なシーンにいるか、遷移先が決定できませんでした");
            }
        }
        
        /// <summary>
        /// 認証済みの場合の適切なシーンを決定
        /// </summary>
        private string DetermineAuthenticatedScene(string currentSceneName)
        {
            switch (currentSceneName)
            {
                case "TitleScene":
                    return "WorldScene"; // タイトルからワールドシーンへ
                case "AuthTest":
                case "FirebaseTest":
                    return "WorldScene"; // 認証テストからワールドシーンへ
                case "WorldScene":
                case "TypingStage":
                    return ""; // 既に適切なシーンにいる
                default:
                    // デフォルトはワールドシーン
                    return "WorldScene";
            }
        }

        /// <summary>
        /// 認証状態を保存
        /// </summary>
        private void SaveAuthState(AuthInfo authInfo)
        {
            try
            {
                string authInfoJson = JsonUtility.ToJson(authInfo);
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                
                PlayerPrefs.SetString(AUTH_INFO_KEY, authInfoJson);
                PlayerPrefs.SetString(AUTH_TIMESTAMP_KEY, timestamp);
                PlayerPrefs.Save();
                
                Debug.Log("認証状態を保存しました");
            }
            catch (Exception e)
            {
                Debug.LogError($"認証状態の保存に失敗: {e.Message}");
            }
        }

        /// <summary>
        /// 認証状態をクリア
        /// </summary>
        private void ClearAuthState()
        {
            PlayerPrefs.DeleteKey(AUTH_INFO_KEY);
            PlayerPrefs.DeleteKey(AUTH_TIMESTAMP_KEY);
            PlayerPrefs.Save();
            
            isAuthenticated = false;
            currentAuthInfo = null;
            
            Debug.Log("認証状態をクリアしました");
        }

        /// <summary>
        /// WebGL認証初期化
        /// </summary>
        private void InitializeWebGLAuth()
        {
            Debug.Log("WebGL認証初期化開始");
            
            // JavaScript側の認証初期化は自動で行われる
            Debug.Log("Firebase初期化は自動で実行されます");
        }

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
            }
        }

        /// <summary>
        /// Google認証開始
        /// </summary>
        public void SignInWithGoogle()
        {
            Debug.Log("=== SignInWithGoogle() が呼び出されました ===");
            
            if (!enableGoogleAuth) {
                Debug.LogWarning("Google認証が無効です");
                OnAuthenticationFailed?.Invoke(AuthResult.NotSupported);
                return;
            }
            
            if (isAuthenticating) {
                Debug.LogWarning("既に認証中です");
                return;
            }
            
            Debug.Log("Google認証開始");
            isAuthenticating = true;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("WebGL環境でGoogle認証を実行");
                signInWithGoogleJS();
            #else
                Debug.Log("エディタ環境: Google認証をスキップ");
                OnAuthenticationFailed?.Invoke(AuthResult.NotSupported);
                isAuthenticating = false;
            #endif
        }

        /// <summary>
        /// Microsoft認証開始
        /// </summary>
        public void SignInWithMicrosoft()
        {
            if (!enableMicrosoftAuth) {
                Debug.LogWarning("Microsoft認証が無効です");
                OnAuthenticationFailed?.Invoke(AuthResult.NotSupported);
                return;
            }
            
            if (isAuthenticating) {
                Debug.LogWarning("既に認証中です");
                return;
            }
            
            Debug.Log("Microsoft認証開始");
            isAuthenticating = true;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                signInWithMicrosoftJS();
            #else
                Debug.Log("エディタ環境: Microsoft認証をスキップ");
                OnAuthenticationFailed?.Invoke(AuthResult.NotSupported);
                isAuthenticating = false;
            #endif
        }

        /// <summary>
        /// ゲスト認証開始
        /// </summary>
        public void SignInAsGuest()
        {
            if (!enableGuestAuth) {
                Debug.LogWarning("ゲスト認証が無効です");
                OnAuthenticationFailed?.Invoke(AuthResult.NotSupported);
                return;
            }
            
            if (isAuthenticating) {
                Debug.LogWarning("既に認証中です");
                return;
            }
            
            Debug.Log("ゲスト認証開始");
            isAuthenticating = true;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                signInAsGuestJS();
            #else
                Debug.Log("エディタ環境: ゲスト認証をスキップ");
                OnAuthenticationFailed?.Invoke(AuthResult.NotSupported);
                isAuthenticating = false;
            #endif
        }

        /// <summary>
        /// ログアウト
        /// </summary>
        public void Logout()
        {
            Debug.Log("ログアウト開始");
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                signOutJS();
            #else
                Debug.Log("エディタ環境: ログアウトをスキップ");
            #endif
            
            // 認証状態をクリア
            ClearAuthState();
        }

        /// <summary>
        /// 認証成功時の処理
        /// </summary>
        public void OnAuthSuccess(string authData)
        {
            try
            {
                Debug.Log($"認証成功データを受信: {authData}");
                
                // JSONデータをAuthInfoに変換
                currentAuthInfo = JsonUtility.FromJson<AuthInfo>(authData);
                isAuthenticated = true;
                isAuthenticating = false;
                
                Debug.Log($"認証成功: {currentAuthInfo.authMethod} - {currentAuthInfo.email}");
                
                // 認証状態を保存
                SaveAuthState(currentAuthInfo);
                
                // 成功イベントを発火
                OnAuthenticationSuccess?.Invoke(currentAuthInfo);
                
                // 認証成功後に次の画面に遷移
                StartCoroutine(TransitionToNextScene());
                
                Debug.Log("認証成功処理完了 - 画面遷移を実行");
            }
            catch (Exception e)
            {
                Debug.LogError($"認証成功データの解析に失敗: {e.Message}");
                OnAuthenticationFailed?.Invoke(AuthResult.Failed);
                isAuthenticating = false;
            }
        }
        
        /// <summary>
        /// 認証成功後に次の画面に遷移
        /// </summary>
        private System.Collections.IEnumerator TransitionToNextScene()
        {
            Debug.Log("認証成功後の画面遷移を開始");
            
            // 少し待機してから遷移
            yield return new WaitForSeconds(1.0f);
            
            // 現在のシーン名を取得
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Debug.Log($"現在のシーン: {currentSceneName}");
            
            // 認証成功後の遷移先を決定
            string nextSceneName = DetermineNextScene(currentSceneName);
            Debug.Log($"遷移先シーン: {nextSceneName}");
            
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                // シーン遷移を実行
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("遷移先シーンが決定できませんでした");
            }
        }
        
        /// <summary>
        /// 現在のシーンに基づいて次のシーンを決定
        /// </summary>
        private string DetermineNextScene(string currentSceneName)
        {
            switch (currentSceneName)
            {
                case "TitleScene":
                    return "LoginSuccessScene"; // タイトルからログイン成功画面へ
                case "AuthTest":
                case "FirebaseTest":
                    return "LoginSuccessScene"; // 認証テストからログイン成功画面へ
                default:
                    // デフォルトはログイン成功画面
                    return "LoginSuccessScene";
            }
        }

        /// <summary>
        /// 認証失敗時の処理
        /// </summary>
        public void OnAuthFailed(string error)
        {
            Debug.LogError($"認証失敗: {error}");
            isAuthenticating = false;
            
            // エラー内容に応じてAuthResultを決定
            AuthResult result = AuthResult.Failed;
            if (error.Contains("cancelled"))
            {
                result = AuthResult.Cancelled;
            }
            else if (error.Contains("not supported") || error.Contains("operation-not-allowed"))
            {
                result = AuthResult.NotSupported;
            }
            
            OnAuthenticationFailed?.Invoke(result);
        }

        /// <summary>
        /// ログアウト成功時の処理
        /// </summary>
        public void OnLogoutSuccess()
        {
            Debug.Log("ログアウト成功");
            ClearAuthState();
            OnLogout?.Invoke();
        }

        /// <summary>
        /// JavaScript側から認証情報を保存
        /// </summary>
        public void SaveAuthInfo(string authData)
        {
            try
            {
                Debug.Log($"認証情報を受信: {authData}");
                
                // JSONデータをAuthInfoに変換
                currentAuthInfo = JsonUtility.FromJson<AuthInfo>(authData);
                isAuthenticated = true;
                isAuthenticating = false;
                
                Debug.Log($"認証情報を保存: {currentAuthInfo.authMethod} - {currentAuthInfo.email}");
                
                // 認証状態を保存
                SaveAuthState(currentAuthInfo);
                
                // 成功イベントを発火
                OnAuthenticationSuccess?.Invoke(currentAuthInfo);
                
                Debug.Log("認証情報の保存完了");
            }
            catch (Exception e)
            {
                Debug.LogError($"認証情報の保存に失敗: {e.Message}");
            }
        }

        // JavaScript側との連携メソッド
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInWithGoogleJS();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInWithMicrosoftJS();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInAsGuestJS();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signOutJS();
    }
} 