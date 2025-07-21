using UnityEngine;

public class FirebaseExample : MonoBehaviour
{
    [SerializeField] private FirebaseConfig firebaseConfig;
    [SerializeField] private GameManager gameManager;
    
    void Start()
    {
        // Firebase設定の検証
        if (firebaseConfig != null && firebaseConfig.IsValid())
        {
            Debug.Log("Firebase設定が有効です");
            
            // FirebaseManagerを初期化
            FirebaseManager.Instance.InitializeFirebase();
            
            // プレイヤーデータの保存例
            if (gameManager != null && gameManager.savedata != null)
            {
                string playerId = gameManager.savedata.Email;
                string jsonData = gameManager.savedata.SerializeForFirebase();
                
                FirebaseManager.Instance.SavePlayerData(playerId, jsonData);
            }
        }
        else
        {
            Debug.LogError("Firebase設定が無効です。FirebaseConfigアセットを確認してください。");
        }
    }
    
    // 設定のテスト用メソッド
    public void TestFirebaseConfig()
    {
        if (firebaseConfig == null)
        {
            Debug.LogError("FirebaseConfigが設定されていません");
            return;
        }
        
        Debug.Log($"API Key: {firebaseConfig.apiKey}");
        Debug.Log($"Auth Domain: {firebaseConfig.authDomain}");
        Debug.Log($"Project ID: {firebaseConfig.projectId}");
        Debug.Log($"Database URL: {firebaseConfig.databaseURL}");
        Debug.Log($"Collection Name: {firebaseConfig.collectionName}");
        
        if (firebaseConfig.IsValid())
        {
            Debug.Log("設定は有効です");
        }
        else
        {
            Debug.LogError("設定が不完全です");
        }
    }
} 