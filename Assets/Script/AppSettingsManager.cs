using UnityEngine;

public class AppSettingsManager : MonoBehaviour
{
    [Header("Firebase設定")]
    [SerializeField] private FirebaseConfig firebaseConfig;
    
    void Start()
    {
        Debug.Log("=== AppSettingsManager Start ===");
        
        if (firebaseConfig == null)
        {
            Debug.LogError("FirebaseConfigが設定されていません！");
            return;
        }
        
        Debug.Log($"FirebaseConfig設定完了: {firebaseConfig.name}");
        Debug.Log($"AppCode: {firebaseConfig.AppId}");
        Debug.Log($"ServiceToken: {firebaseConfig.ApiKey}");
        
        // 本物のデータをJavaScript側に渡す
        SendRealDataToJS();
        
        Debug.Log("=== AppSettingsManager Start完了 ===");
    }
    
    // 本物のデータをJavaScript側に送信
    private void SendRealDataToJS()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        SetAppCode(firebaseConfig.AppId);
        SetServiceToken(firebaseConfig.ApiKey);
        SetProjectCode(firebaseConfig.ProjectId);
        SetAuthDomain(firebaseConfig.AuthDomain);
        SetStorageBucket(firebaseConfig.StorageBucket);
        SetMessagingSenderCode(firebaseConfig.MessagingSenderId);
        SetDatabaseURL(firebaseConfig.DatabaseUrl);
        SetProductionMode(firebaseConfig.IsProduction);
        #endif
    }
    

    
    // jslib関数呼び出し（本物データ）
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetAppCode(string appCode);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetServiceToken(string serviceToken);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetProjectCode(string projectCode);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetAuthDomain(string authDomain);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetStorageBucket(string storageBucket);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetMessagingSenderCode(string messagingSenderCode);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetDatabaseURL(string databaseURL);
    
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetProductionMode(bool isProduction);
    

    

}
