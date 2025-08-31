using UnityEngine;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = "FirebaseConfig", menuName = "Firebase/Firebase Config")]
public class FirebaseConfig : ScriptableObject
{
    [Header("Firebase Web GL Config")]
    [SerializeField] private string apiKey;
    [SerializeField] private string authDomain;
    [SerializeField] private string projectId;
    [SerializeField] private string storageBucket;
    [SerializeField] private string messagingSenderId;
    [SerializeField] private string appId;
    [SerializeField] private string databaseUrl; // Realtime Database URL
    [SerializeField] private bool isProduction = true; // 新しく追加

    public string ApiKey => apiKey;
    public string AuthDomain => authDomain;
    public string ProjectId => projectId;
    public string StorageBucket => storageBucket;
    public string MessagingSenderId => messagingSenderId;
    public string AppId => appId;
    public string DatabaseUrl => databaseUrl;
    public bool IsProduction => isProduction; // 新しく追加

    public string GetFirebaseConfigJson()
    {
        var config = new
        {
            apiKey = apiKey,
            authDomain = authDomain,
            projectId = projectId,
            storageBucket = storageBucket,
            messagingSenderId = messagingSenderId,
            appId = appId,
            databaseURL = databaseUrl
        };
        
        return JsonConvert.SerializeObject(config, Formatting.Indented);
    }
} 