using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace necotapuFB
{
    /// <summary>
    /// アプリバージョン管理クラス
    /// UnityのbundleVersionとFirebaseのバージョンを比較し、強制再読み込みを制御
    /// </summary>
    public class AppVersionManager : MonoBehaviour
    {
        // jslib関数の宣言
        
        [DllImport("__Internal")]
        private static extern void LoadAllDataFromFirestoreWithLimitJslib(string limitHours);
        
        [Header("バージョン設定")]
        [SerializeField] private string currentVersion;
        
        [Header("Firebase設定")]
        [SerializeField] private string firebaseVersion;
        [SerializeField] private bool forceReload = false;
        
        // 重複実行防止フラグ
        public static AppVersionManager Instance { get; private set; }
        
        public event Action<string> OnVersionCheckComplete;
        public event Action OnForceReloadRequired;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // UnityのbundleVersionを取得
            currentVersion = Application.version;
            Debug.Log($"アプリバージョン: {currentVersion}");
            
        }
        
        /// <summary>
        /// Firebaseからバージョン情報を取得
        /// </summary>
        public void CheckFirebaseVersion()
        {
            Debug.Log("Firebaseからバージョン情報を取得開始");
            
            // FirebaseBridge.jslibを使用してバージョン情報を取得
            #if UNITY_WEBGL && !UNITY_EDITOR
                // WebGL環境ではjslibを使用
                LoadAllDataFromFirestoreWithLimitJslib("23");
            #else
                Debug.Log("エディタ環境: バージョンチェックをスキップ");
                OnVersionCheckComplete?.Invoke("editor");
            #endif
        }
        
        /// <summary>
        /// Firebaseから全データを一括取得（ユーザ情報、ランキング情報、バージョン情報）
        /// </summary>
        public void LoadAllDataFromFirebase()
        {
            Debug.Log("Firebaseから全データを一括取得開始");
            
            // FirebaseBridge.jslibを使用して一括データ取得
            #if UNITY_WEBGL && !UNITY_EDITOR
                // WebGL環境ではjslibを使用（23時間制限付き）
                LoadAllDataFromFirestoreWithLimitJslib("23");
            #else
                Debug.Log("エディタ環境: 一括データ取得をスキップ");
                OnVersionCheckComplete?.Invoke("editor");
            #endif
        }
        
        /// <summary>
        /// Firebaseからバージョン情報を受信（jslibから呼び出される）
        /// </summary>
        /// <param name="firebaseVersion">Firebaseのバージョン</param>
        public void OnFirebaseVersionReceived(string firebaseVersion)
        {
            Debug.Log($"Firebaseバージョン受信: {firebaseVersion}");
            
            this.firebaseVersion = firebaseVersion;
            
            // バージョン比較
            CompareVersions();
        }
        
        /// <summary>
        /// バージョン比較を実行
        /// </summary>
        private void CompareVersions()
        {
            Debug.Log($"バージョン比較: Unity={currentVersion}, Firebase={firebaseVersion}");
            
            if (string.IsNullOrEmpty(firebaseVersion))
            {
                Debug.LogWarning("Firebaseバージョンが取得できませんでした");
                OnVersionCheckComplete?.Invoke("error");
                return;
            }
            
            // バージョン比較（シンプルな文字列比較）
            if (IsVersionNewer(firebaseVersion, currentVersion))
            {
                Debug.LogWarning($"Firebaseバージョン({firebaseVersion})が新しいため、強制再読み込みが必要です");
                forceReload = true;
                OnForceReloadRequired?.Invoke();
                
                // 自動的に強制再読み込みを実行
                StartCoroutine(DelayedForceReload());
            }
            else
            {
                Debug.Log("バージョンは最新です");
                forceReload = false;
                
                // バージョンが最新の場合、データロードを実行
                // LoadAllDataFromFirebase(); // 無限ループの原因となるため、引き続きコメントアウト
            }
            
            OnVersionCheckComplete?.Invoke("success");
        }
        
        /// <summary>
        /// バージョンが新しいかチェック
        /// </summary>
        /// <param name="version1">比較対象1</param>
        /// <param name="version2">比較対象2</param>
        /// <returns>version1が新しい場合true</returns>
        private bool IsVersionNewer(string version1, string version2)
        {
            try
            {
                // バージョン文字列をパース
                var v1 = new Version(version1);
                var v2 = new Version(version2);
                
                return v1 > v2;
            }
            catch (Exception e)
            {
                Debug.LogError($"バージョン比較中にエラーが発生: {e.Message}");
                // エラーの場合は文字列比較
                return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase) > 0;
            }
        }
        
        /// <summary>
        /// 強制再読み込みを実行
        /// </summary>
        public void ForceReload()
        {
            if (forceReload)
            {
                Debug.Log("強制再読み込みを実行します");
                
                // WebGL環境ではページをリロード
                #if UNITY_WEBGL && !UNITY_EDITOR
                    // リロードフラグを保存してからリロード実行
                    Application.ExternalEval("localStorage.setItem('forceReloadInProgress', 'true'); location.reload();");
                #else
                    Debug.Log("エディタ環境: 強制再読み込みをスキップ");
                #endif
            }
        }
        
        /// <summary>
        /// 現在のバージョン情報を取得
        /// </summary>
        public string GetCurrentVersion()
        {
            return currentVersion;
        }
        
        /// <summary>
        /// Firebaseのバージョン情報を取得
        /// </summary>
        public string GetFirebaseVersion()
        {
            return firebaseVersion;
        }
        
        /// <summary>
        /// 強制再読み込みが必要かチェック
        /// </summary>
        public bool IsForceReloadRequired()
        {
            return forceReload;
        }
        
        /// <summary>
        /// 遅延付き強制再読み込み（ユーザーに通知してから実行）
        /// </summary>
        private IEnumerator DelayedForceReload()
        {
            Debug.Log("強制再読み込みの準備中... 3秒後に実行します");
            
            // 3秒待機（ユーザーに通知を表示する時間）
            yield return new WaitForSeconds(3f);
            
            // 強制再読み込みを実行
            ForceReload();
        }
    }
}
