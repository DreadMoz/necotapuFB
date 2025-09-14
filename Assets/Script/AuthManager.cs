using UnityEngine;
using System;
using System.Collections.Generic;
using necotapuFB; // AppVersionManagerにアクセスするために追加

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
        public string photoURL;
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
        [SerializeField] private bool enableMicrosoftAuth = true; // Microsoft認証を有効化
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
        private const string LAST_AUTH_METHOD_KEY = "LastAuthMethod";

        // プロパティ
        public bool IsAuthenticated => isAuthenticated;
        public AuthInfo CurrentAuthInfo => currentAuthInfo;
        public bool IsAuthenticating => isAuthenticating;

        [SerializeField] private GameManager gm;
        
        private void Start()
        {
            Debug.Log("AuthManager初期化開始");
            
            // GameManagerのインスタンスを取得
            gm = FindObjectOfType<GameManager>();
            if (gm == null)
            {
                Debug.LogError("AuthManager: GameManagerのインスタンスが見つかりません。");
            }

            // 認証フラグをリセット
            isAuthenticating = false;
            
            // 新しいアーキテクチャでは、認証はindex.htmlで行われるため、
            // Unity起動時は認証状態の復元のみを行う
            RestoreAuthState();
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                // WebGL環境での初期化（認証はindex.htmlで行われるため、最小限の処理）
                InitializeWebGLAuth();
            #else
                // エディタ環境でも認証機能を有効にする
                Debug.Log("エディタ環境: 認証機能を有効化");
                InitializeEditorAuth();
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
        /// エディタ環境での認証初期化
        /// </summary>
        private void InitializeEditorAuth()
        {
            Debug.Log("エディタ環境認証初期化開始");
            
            // エディタ環境ではゲスト認証を有効にする
            enableGuestAuth = true;
            Debug.Log("エディタ環境: ゲスト認証を有効化");
        }

        /// <summary>
        /// エディタ環境でのゲスト認証シミュレーション
        /// </summary>
        private void SimulateGuestAuth()
        {
            Debug.Log("エディタ環境: ゲスト認証をシミュレート");
            
            // ゲスト認証情報を作成
            var guestAuthInfo = new AuthInfo
            {
                userId = "guest_" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
                email = "guest@necotapu.local",
                displayName = "ゲストユーザー",
                authMethod = "guest",
                isGuest = true,
                authenticatedAt = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            
            // 認証成功をシミュレート
            currentAuthInfo = guestAuthInfo;
            isAuthenticated = true;
            isAuthenticating = false;
            
            Debug.Log($"ゲスト認証成功: {guestAuthInfo.userId}");
            
            // 認証状態を保存
            SaveAuthState(guestAuthInfo);
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
            
            // 認証フラグを強制的にリセット（デバッグ用）
            isAuthenticating = false;
            Debug.Log("認証フラグをリセットしました");
            
            if (isAuthenticating) {
                Debug.LogWarning("SignInWithGoogle: 既に認証中です");
                return;
            }
            
            Debug.Log("Google認証開始");
            isAuthenticating = true;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("WebGL環境でGoogle認証を実行");
                try
                {
                    signInWithGoogleJslib();
                    Debug.Log("signInWithGoogleJslib()呼び出し完了");
                }
                catch (Exception e)
                {
                    Debug.LogError($"signInWithGoogleJslib()呼び出しエラー: {e.Message}");
                    isAuthenticating = false;
                    OnAuthenticationFailed?.Invoke(AuthResult.Failed);
                }
            #else
                Debug.Log("エディタ環境: ダミーデータでGoogle認証をシミュレート");
                // WebGL環境ではダミー認証を実行しない
                #if UNITY_EDITOR
                    StartCoroutine(SimulateGoogleAuth());
                #else
                    Debug.Log("WebGL環境: ダミー認証をスキップ");
                    OnAuthenticationFailed?.Invoke(AuthResult.Failed);
                    isAuthenticating = false;
                #endif
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
                Debug.LogWarning("SignInWithMicrosoft: 既に認証中です");
                return;
            }
            
            Debug.Log("Microsoft認証開始");
            isAuthenticating = true;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                signInWithMicrosoftJslib();
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
                Debug.LogWarning("SignInAsGuest: 既に認証中です");
                return;
            }
            
            Debug.Log("ゲスト認証開始");
            isAuthenticating = true;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("WebGL環境: signInAsGuestJslib()を呼び出し");
                signInAsGuestJslib();
            #else
                Debug.Log("エディタ環境: ゲスト認証をシミュレート");
                // エディタ環境ではゲスト認証をシミュレート
                SimulateGuestAuth();
            #endif
        }
        
        /// <summary>
        /// メールアドレスとパスワードで認証
        /// </summary>
        public void SignInWithEmail(string email, string password)
        {
            if (isAuthenticating) {
                Debug.LogWarning("SignInWithEmail: 既に認証中です");
                return;
            }
            
            Debug.Log($"メール認証開始: {email}");
            isAuthenticating = true;
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                signInWithEmailJslib(email, password);
            #else
                Debug.Log("エディタ環境: メール認証をスキップ");
                OnAuthenticationFailed?.Invoke(AuthResult.NotSupported);
                isAuthenticating = false;
            #endif
        }

        /// <summary>
        /// 認証成功時の処理 JSから呼ばれる
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
                
                OnAuthenticationSuccess?.Invoke(currentAuthInfo);
                
                // 認証方法を履歴に保存
                SaveAuthMethodHistory(currentAuthInfo.authMethod.ToLower());
                
                // 認証状態を保存
                SaveAuthState(currentAuthInfo);
                
                Debug.Log("認証成功処理完了 - ログイン情報表示を実行");

                // 認証成功後にアプリバージョンをチェック
                // if (AppVersionManager.Instance != null)
                // {
                //     AppVersionManager.Instance.CheckFirebaseVersion();
                // }
                // else
                // {
                //     Debug.LogError("AuthManager: AppVersionManager.Instance が見つかりません");
                // }

                Debug.Log($"ログイン成功: {currentAuthInfo.email} - Firestoreチェックを開始");
                
                // 新しいアカウントでログインした場合、既存データをクリア
                if (!string.IsNullOrEmpty(gm.savedata.Uid) && gm.savedata.Uid != currentAuthInfo.userId)
                {
                    Debug.Log($"アカウントが変更されました: '{gm.savedata.Uid}' → '{currentAuthInfo.userId}' - データをクリア");
                    FindObjectOfType<TitleSky>()?.ClearGameData(); // TitleSkyのClearGameDataを呼び出す
                }
                else
                {
                    Debug.Log("同じアカウントでのログイン - データクリアをスキップ");
                }
                
                // SaveDataにUIDを設定（新しいフィールド）
                gm.savedata.Uid = currentAuthInfo.userId;
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
            
            // ログイン情報表示のための待機時間を延長
            yield return new WaitForSeconds(3.0f);
            
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
                    return "WorldScene"; // タイトルからワールドシーンへ
                case "AuthTest":
                case "FirebaseTest":
                    return "WorldScene"; // 認証テストからワールドシーンへ
                default:
                    // デフォルトはワールドシーン
                    return "WorldScene";
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
        /// 自動認証を実行
        /// </summary>
        public void AutoLogin()
        {
            // 認証フラグを強制的にリセット（デバッグ用）
            isAuthenticating = false;
            Debug.Log("AutoLogin: 認証フラグをリセットしました");
            
            // サイレント認証が既に完了している場合はログイン情報を表示
            if (isAuthenticated && currentAuthInfo != null)
            {
                Debug.Log("AutoLogin: 既に認証済み - ログイン情報を表示");
                Debug.Log($"currentAuthInfo: {JsonUtility.ToJson(currentAuthInfo)}");
                
                Debug.Log("AutoLogin: 既に認証済み - ログイン情報を表示完了");
                return;
            }
            
            if (isAuthenticating)
            {
                Debug.Log("AutoLogin: 既に認証中です");
                return;
            }
            
            Debug.Log("自動認証開始");
            isAuthenticating = true;
            
            // 環境検出と履歴確認
            string authMethod = DetermineAuthMethod();
            
            switch (authMethod)
            {
                case "google":
                    Debug.Log("Google認証を実行");
                    SignInWithGoogle();
                    break;
                case "microsoft":
                    Debug.Log("Microsoft認証を実行");
                    SignInWithMicrosoft();
                    break;
                case "guest":
                    Debug.Log("ゲスト認証を実行");
                    SignInAsGuest();
                    break;
                default:
                    Debug.Log("デフォルトでGoogle認証を実行");
                    SignInWithGoogle();
                    break;
            }
        }
        
        /// <summary>
        /// 認証方法を決定
        /// </summary>
        private string DetermineAuthMethod()
        {
            // Chromebook環境の確認
            if (IsChromebookJslib())
            {
                Debug.Log("Chromebook環境を検出");
                return "google";
            }
            
            // Microsoft環境の確認
            if (IsMicrosoftEnvironmentJslib())
            {
                Debug.Log("Microsoft環境を検出");
                return "microsoft";
            }
            
            // 前回の認証方法を確認
            #if UNITY_WEBGL && !UNITY_EDITOR
                string lastMethod = GetLastAuthMethodJslib();
                if (!string.IsNullOrEmpty(lastMethod))
                {
                    Debug.Log($"前回の認証方法を使用: {lastMethod}");
                    return lastMethod;
                }
            #else
                Debug.Log("エディタ環境: 前回の認証方法確認をスキップ");
            #endif
            
            // デフォルトでGoogle認証を実行
            Debug.Log("デフォルトでGoogle認証を実行");
            return "google";
        }
        
        /// <summary>
        /// 認証成功時に履歴を保存
        /// </summary>
        private void SaveAuthMethodHistory(string method)
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
                SaveLastAuthMethodJslib(method);
                Debug.Log($"認証方法を履歴に保存: {method}");
            #else
                Debug.Log($"エディタ環境: 認証方法を履歴に保存（スキップ）: {method}");
            #endif
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
        /// ログアウト処理を実行
        /// </summary>
        public void Logout()
        {
            Debug.Log("=== Logout() が呼び出されました ===");
            
            if (!isAuthenticated)
            {
                Debug.LogWarning("ログアウト: 既にログアウト済みです");
                return;
            }
            
            Debug.Log("ログアウト処理開始");
            
            #if UNITY_WEBGL && !UNITY_EDITOR
                Debug.Log("WebGL環境でログアウトを実行");
                try
                {
                    signOutJslib();
                    Debug.Log("signOutJslib()呼び出し完了");
                }
                catch (Exception e)
                {
                    Debug.LogError($"signOutJslib()呼び出しエラー: {e.Message}");
                }
            #else
                Debug.Log("エディタ環境: ダミーログアウトを実行");
                StartCoroutine(SimulateLogout());
            #endif
        }
        
        /// <summary>
        /// エディタ環境でログアウトをシミュレート
        /// </summary>
        private System.Collections.IEnumerator SimulateLogout()
        {
            Debug.Log("ダミーログアウトを開始");
            
            // ログアウト処理をシミュレート（少し待機）
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("✅ ダミーログアウト成功");
            
            // 認証状態をクリア
            ClearAuthState();
            
            // ログアウト成功イベントを発火
            OnLogout?.Invoke();
            
            Debug.Log("ダミーログアウト完了");
        }

        /// <summary>
        /// 認証フラグをリセット（JavaScript側から呼び出し）
        /// </summary>
        public void ResetAuthFlag()
        {
            Debug.Log("ResetAuthFlag: 認証フラグをリセットしました");
            isAuthenticating = false;
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
                
                Debug.Log("認証情報の保存完了");
            }
            catch (Exception e)
            {
                Debug.LogError($"認証情報の保存に失敗: {e.Message}");
            }
        }

        // JavaScript側との連携メソッド
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInWithGoogleJslib();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInWithMicrosoftJslib();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInWithEmailJslib(string email, string password);
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInAsGuestJslib();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signOutJslib();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool IsChromebookJslib();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool IsMicrosoftEnvironmentJslib();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string GetLastAuthMethodJslib();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void SaveLastAuthMethodJslib(string method);
        
        /// <summary>
        /// エディタ環境でGoogle認証をシミュレート
        /// </summary>
        private System.Collections.IEnumerator SimulateGoogleAuth()
        {
            Debug.Log("ダミーGoogle認証を開始");
            
            // 認証処理をシミュレート（少し待機）
            yield return new WaitForSeconds(1.0f);
            
            // ダミーデータを作成
            var dummyAuthInfo = new AuthInfo
            {
                userId = "LPnhMuDyRidyaxXM1GsT73byuwy1",
                email = "rochy2moo@gmail.com",
                displayName = "Ryosuke Mori",
                photoURL = "https://lh3.googleusercontent.com/a/ACg8ocKFmG805MguO-IriFHcPjSKiohDN1pax3ktnHmC3HhHJfE_Wi1KxA=s96-c",
                authMethod = "Google",
                isGuest = false,
                authenticatedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };
            
            Debug.Log("✅ ダミーGoogle認証成功");
            Debug.Log($"ユーザーID: {dummyAuthInfo.userId}");
            Debug.Log($"メールアドレス: {dummyAuthInfo.email}");
            Debug.Log($"表示名: {dummyAuthInfo.displayName}");
            Debug.Log($"プロフィール画像URL: {dummyAuthInfo.photoURL}");
            
            // 認証情報を保存
            currentAuthInfo = dummyAuthInfo;
            isAuthenticated = true;
            isAuthenticating = false;
            
            // 認証状態を保存
            SaveAuthState(dummyAuthInfo);
            
            // 認証方法を履歴に保存
            SaveAuthMethodHistory("google");
            
            Debug.Log("ダミー認証成功処理完了");
            
            // 認証成功イベントを発火（これだけで十分）
            OnAuthenticationSuccess?.Invoke(dummyAuthInfo);
            Debug.Log("OnAuthenticationSuccessイベントを発火しました");
            
        }
    }
} 