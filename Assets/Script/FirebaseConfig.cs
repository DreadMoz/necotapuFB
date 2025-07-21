using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = "FirebaseConfig", menuName = "FirebaseConfig")]
public class FirebaseConfig : ScriptableObject
{
    [Header("Firebase Configuration")]
    [SerializeField] public string apiKey;
    [SerializeField] public string authDomain;
    [SerializeField] public string projectId;
    [SerializeField] public string storageBucket;
    [SerializeField] public string messagingSenderId;
    [SerializeField] public string appId;
    
    [Header("Database Settings")]
    [SerializeField] public string databaseURL;
    [SerializeField] public string collectionName = "playerData";
    
    [Header("Authentication Settings")]
    [SerializeField] public bool enableGoogleAuth = true;      // 必須（自治体採用率高）
    [SerializeField] public bool enableMicrosoftAuth = true;   // 推奨（全国展開対応）
    [SerializeField] public bool enableAnonymousAuth = true;   // フォールバック（イベント用）
    [SerializeField] public string googleClientId = "";        // Google OAuth Client ID
    [SerializeField] public string microsoftClientId = "";     // Microsoft OAuth Client ID
    
    // 設定が有効かチェック
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(apiKey) && 
               !string.IsNullOrEmpty(authDomain) && 
               !string.IsNullOrEmpty(projectId);
    }
    
    // 設定をJSON形式で取得（Firebase初期化用）
    public string GetFirebaseConfigJson()
    {
        var config = new
        {
            apiKey = apiKey,
            authDomain = authDomain,
            projectId = projectId,
            storageBucket = storageBucket,
            messagingSenderId = messagingSenderId,
            appId = appId
        };
        
        return JsonConvert.SerializeObject(config);
    }
} 