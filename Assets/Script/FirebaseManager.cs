using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class FirebaseManager : MonoBehaviour
{
    [SerializeField] private FirebaseConfig config;
    [SerializeField] private GameManager gameManager;
    
    // Firebase接続状態
    public bool IsInitialized { get; private set; } = false;
    public bool IsConnected { get; private set; } = false;
    
    // シングルトンパターン
    private static FirebaseManager instance;
    public static FirebaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FirebaseManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("FirebaseManager");
                    instance = go.AddComponent<FirebaseManager>();
                }
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    // Firebase初期化
    public void InitializeFirebase()
    {
        Debug.Log("FirebaseManager.InitializeFirebase() が呼ばれました");
        
        if (config == null)
        {
            Debug.LogError("FirebaseConfigが設定されていません");
            return;
        }
        
        Debug.Log($"FirebaseConfig: {config.name}");
        
        if (!config.IsValid())
        {
            Debug.LogError("Firebase設定が不完全です");
            return;
        }
        
        Debug.Log("Firebase設定が有効です");
        Debug.Log("Firebase初期化開始...");
        
        // WebGL環境でのFirebase初期化
        #if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("WebGL環境: Firebase初期化を実行");
            InitializeFirebaseWebGL();
        #else
            // エディタ環境でのテスト用
            Debug.Log("エディタ環境: Firebase初期化をスキップ");
            IsInitialized = true;
        #endif
    }
    
    // WebGL環境でのFirebase初期化
    private void InitializeFirebaseWebGL()
    {
        string configJson = config.GetFirebaseConfigJson();
        Debug.Log($"Firebase設定を送信: {configJson}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            // JavaScript側でFirebaseを初期化
            Debug.Log("WebGL環境: setFirebaseConfigを呼び出し");
            setFirebaseConfig(configJson);
            Debug.Log("WebGL環境: setFirebaseConfig呼び出し完了");
        #else
            // エディタ環境ではスキップ
            Debug.Log("エディタ環境: Firebase設定送信をスキップ");
        #endif
    }
    
    // プレイヤーデータをFirebaseに保存
    public void SavePlayerData(string playerId, string jsonData)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("Firebaseが初期化されていません");
            return;
        }
        
        Debug.Log($"プレイヤーデータを保存: {playerId}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            savePlayerDataJS(playerId, jsonData);
        #else
            Debug.Log($"エディタ環境: データ保存をスキップ - {jsonData}");
        #endif
    }
    
    // Firebaseからプレイヤーデータを読み込み
    public void LoadPlayerData(string playerId)
    {
        if (!IsInitialized)
        {
            Debug.LogWarning("Firebaseが初期化されていません");
            return;
        }
        
        Debug.Log($"プレイヤーデータを読み込み: {playerId}");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
            loadPlayerDataJS(playerId);
        #else
            Debug.Log("エディタ環境: データ読み込みをスキップ");
        #endif
    }
    
    // 認証状態をチェック
    public void CheckAuthState()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            checkAuthStateJS();
        #else
            Debug.Log("エディタ環境: 認証チェックをスキップ");
        #endif
    }
    
    // 匿名認証
    public void SignInAnonymously()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            signInAnonymouslyJS();
        #else
            Debug.Log("エディタ環境: 匿名認証をスキップ");
        #endif
    }
    
    // Google認証
    public void SignInWithGoogle()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            signInWithGoogleJS();
        #else
            Debug.Log("エディタ環境: Google認証をスキップ");
        #endif
    }
    
    // Microsoft認証
    public void SignInWithMicrosoft()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            signInWithMicrosoftJS();
        #else
            Debug.Log("エディタ環境: Microsoft認証をスキップ");
        #endif
    }
    
    // メール認証（ログイン）
    public void SignInWithEmail(string email, string password)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            signInWithEmailJS(email, password);
        #else
            Debug.Log("エディタ環境: メール認証をスキップ");
        #endif
    }
    
    // メール認証（新規登録）
    public void CreateUserWithEmail(string email, string password)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
            createUserWithEmailJS(email, password);
        #else
            Debug.Log("エディタ環境: メール新規登録をスキップ");
        #endif
    }
    
    // JavaScript側との連携メソッド
    #if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void setFirebaseConfig(string configJson);
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void savePlayerDataJS(string playerId, string jsonData);
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void loadPlayerDataJS(string playerId);
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void checkAuthStateJS();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInAnonymouslyJS();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInWithGoogleJS();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInWithMicrosoftJS();
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void signInWithEmailJS(string email, string password);
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void createUserWithEmailJS(string email, string password);
    #endif
    
    // JavaScript側からのコールバック
    public void OnFirebaseInitialized(string result)
    {
        Debug.Log($"Firebase初期化結果を受信: {result}");
        
        if (result == "success")
        {
            IsInitialized = true;
            IsConnected = true;
            Debug.Log("Firebase初期化完了 - IsInitialized = true");
        }
        else
        {
            IsInitialized = false;
            IsConnected = false;
            Debug.LogError("Firebase初期化失敗");
        }
    }
    
    public void OnPlayerDataSaved(string result)
    {
        Debug.Log($"プレイヤーデータ保存完了: {result}");
        if (gameManager != null)
        {
            // 保存完了の通知
        }
    }
    
    public void OnPlayerDataLoaded(string jsonData)
    {
        Debug.Log($"プレイヤーデータ読み込み完了: {jsonData}");
        if (gameManager != null && gameManager.savedata != null)
        {
            try
            {
                gameManager.savedata.DeserializeFromFirebase(jsonData);
                Debug.Log("Firebaseからデータを正常に読み込みました");
            }
            catch (Exception ex)
            {
                Debug.LogError($"データのデシリアライズに失敗: {ex.Message}");
            }
        }
    }
    
    public void OnAuthStateChanged(string authState)
    {
        Debug.Log($"認証状態変更: {authState}");
        // 認証状態に応じた処理
    }
} 