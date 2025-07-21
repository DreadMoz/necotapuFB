using UnityEngine;
using UnityEngine.UI;

public class FirebaseTest : MonoBehaviour
{
    [SerializeField] private FirebaseManager firebaseManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Button testButton;
    [SerializeField] private Text statusText;
    
    void Start()
    {
        Debug.Log("=== FirebaseTest.Start() が呼ばれました ===");
        
        if (testButton != null)
        {
            testButton.onClick.AddListener(TestFirebase);
        }
        
        UpdateStatus("Firebaseテスト準備完了");
        
        // Firebase初期化を開始
        if (firebaseManager != null)
        {
            Debug.Log("firebaseManagerが見つかりました。初期化を開始します。");
            UpdateStatus("Firebase初期化開始...");
            firebaseManager.InitializeFirebase();
        }
        else
        {
            Debug.LogError("firebaseManagerがnullです！");
            UpdateStatus("FirebaseManagerが見つかりません");
        }
    }
    
    public void TestFirebase()
    {
        if (firebaseManager == null)
        {
            UpdateStatus("FirebaseManagerが見つかりません");
            return;
        }
        
        UpdateStatus("Firebase初期化開始...");
        
        // Firebase初期化
        firebaseManager.InitializeFirebase();
        
        // 3秒後に認証テスト
        Invoke(nameof(TestAuthentication), 3f);
    }
    
    private void TestAuthentication()
    {
        UpdateStatus("匿名認証開始...");
        firebaseManager.SignInAnonymously();
        
        // 3秒後にデータ保存テスト
        Invoke(nameof(TestDataSave), 3f);
    }
    
    private void TestDataSave()
    {
        if (gameManager == null || gameManager.savedata == null)
        {
            UpdateStatus("GameManagerまたはSaveDataが見つかりません");
            return;
        }
        
        UpdateStatus("データ保存テスト開始...");
        
        // テストデータを設定
        gameManager.savedata.PlayerData.Email = "test@example.com";
        gameManager.savedata.PlayerData.UserName = "TestUser";
        gameManager.savedata.PlayerData.Gold = 1000;
        
        // Firebaseに保存
        string playerId = gameManager.savedata.PlayerData.Email;
        string jsonData = gameManager.savedata.SerializeForFirebase();
        
        firebaseManager.SavePlayerData(playerId, jsonData);
        
        // 3秒後にデータ読み込みテスト
        Invoke(nameof(TestDataLoad), 3f);
    }
    
    private void TestDataLoad()
    {
        UpdateStatus("データ読み込みテスト開始...");
        
        if (gameManager == null || gameManager.savedata == null)
        {
            UpdateStatus("GameManagerまたはSaveDataが見つかりません");
            return;
        }
        
        string playerId = gameManager.savedata.PlayerData.Email;
        firebaseManager.LoadPlayerData(playerId);
    }
    
    private void UpdateStatus(string message)
    {
        Debug.Log($"FirebaseTest: {message}");
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
    
    // FirebaseManagerからのコールバック
    public void OnFirebaseInitialized(string result)
    {
        UpdateStatus($"Firebase初期化: {result}");
    }
    
    public void OnPlayerDataSaved(string result)
    {
        UpdateStatus($"データ保存: {result}");
    }
    
    public void OnPlayerDataLoaded(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
        {
            UpdateStatus("データが見つかりませんでした");
            return;
        }
        
        UpdateStatus("データ読み込み成功");
        Debug.Log($"読み込まれたデータ: {jsonData}");
    }
    
    public void OnAuthStateChanged(string authState)
    {
        UpdateStatus($"認証状態: {authState}");
    }
} 