// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Auth.OAuth2.Flows;
// using Google.Apis.Oauth2.v2;
// using Google.Apis.Oauth2.v2.Data;
// using Google.Apis.Services;
// using Google.Apis.Util.Store;
// using System.Threading;
// using System.Threading.Tasks;
// using UnityEngine;
// using UnityEngine.Networking;
// using System.Collections;
// using System;
// using System.IO;

// public class GoogleAuth : MonoBehaviour
// {
//     [SerializeField]
//     private string clientId;
//     [SerializeField]
//     private string clientSecret;

//     private void Awake()
//     {
//         LoadCredentialsDesktop();
//     }

//     private void LoadCredentialsDesktop()
//     {/*
//         string filePath = Path.Combine(Application.streamingAssetsPath, "necoOAuthDesktop.json");

//         if (File.Exists(filePath))
//         {
//             string dataAsJson = File.ReadAllText(filePath);
//             ProcessCredentials(dataAsJson);
//         }
//         else
//         {
//             Debug.LogError("Cannot find credentials file.");
//         }*/
//     }

//     private void ProcessCredentials(string dataAsJson)
//     {
//         GoogleCredentials credentials = JsonUtility.FromJson<GoogleCredentials>(dataAsJson);
        
//         clientId = credentials.installed.client_id;
//         clientSecret = credentials.installed.client_secret;

//         Debug.Log("Credentials loaded successfully.");
//     }

//     // プロフィール画像をロードして表示
//     public IEnumerator LoadProfileImage(string imageUrl, Action<Texture2D> onCompleted)
//     {
//         UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
//         yield return request.SendWebRequest();

//         if (request.result == UnityWebRequest.Result.Success)
//         {
//             Texture2D texture = DownloadHandlerTexture.GetContent(request);
//             onCompleted?.Invoke(texture);
//         }
//         else
//         {
//             Debug.LogError("画像のダウンロードに失敗しました: " + request.error);
//             onCompleted?.Invoke(null);
//         }
//     }

//     // 認証プロセスを非同期で行い、アクセストークンを含むユーザー情報を返します。
//     public async Task<(Userinfo userInfo, string accessToken)> Authenticate()
//     {
//         try
//         {
//             // スコープを設定
//             string[] scopes = new string[] { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile, "https://www.googleapis.com/auth/drive" };

//             // ClientSecrets オブジェクトを作成
//             ClientSecrets secrets = new ClientSecrets
//             {
//                 ClientId = clientId,
//                 ClientSecret = clientSecret
//             };

//             UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
//                 secrets,
//                 scopes,
//                 "user",
//                 CancellationToken.None);
//             Debug.Log("GoogleWebAuthorizationBroker.AuthorizeAsync done.");

//             // アクセストークンを取得
//             string accessToken = credential.Token.AccessToken;

//             // Oauth2サービスを初期化してユーザー情報を取得
//             var service = new Oauth2Service(new BaseClientService.Initializer()
//             {
//                 HttpClientInitializer = credential,
//                 ApplicationName = "Unity Google Auth"
//             });
//             Debug.Log("Oauth2Service done.");

//             Userinfo userInfo = await service.Userinfo.Get().ExecuteAsync();
//             Debug.Log("service.Userinfo.Get().ExecuteAsync() done.");

//             // ユーザー情報とアクセストークンの両方を返します。
//             return (userInfo, accessToken);
//         }
//         catch (Exception ex)
//         {
//             // 例外をキャッチした場合、エラーメッセージをログに記録またはコンソールに出力
//             Console.WriteLine($"Authentication failed: {ex.Message}");
//             // 必要に応じて、エラー情報を含む例外をスロー
//             throw new ApplicationException("Authentication failed.", ex);
//         }
//     }
//     public async Task<(Userinfo userInfo, string accessToken)> ReAuthenticate()
//     {
//         // スコープを設定
//         string[] scopes = new string[] { Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile, "https://www.googleapis.com/auth/drive" };

//         // ClientSecrets オブジェクトを作成
//         ClientSecrets secrets = new ClientSecrets
//         {
//             ClientId = clientId,
//             ClientSecret = clientSecret
//         };

//         // 既存の認証情報を破棄して新しい認証プロセスを強制
//         var initializer = new GoogleAuthorizationCodeFlow.Initializer
//         {
//             ClientSecrets = secrets
//         };
//         var flow = new GoogleAuthorizationCodeFlow(initializer);
//         var token = await flow.LoadTokenAsync("user", CancellationToken.None);
//         if (token != null)
//         {
//             await flow.DeleteTokenAsync("user", CancellationToken.None);
//         }

//         UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
//             secrets,
//             scopes,
//             "user",
//             CancellationToken.None,
//             new NullDataStore());

//         // アクセストークンを取得
//         string accessToken = credential.Token.AccessToken;

//         // Oauth2サービスを初期化してユーザー情報を取得
//         var service = new Oauth2Service(new BaseClientService.Initializer()
//         {
//             HttpClientInitializer = credential,
//             ApplicationName = "Unity Google Auth"
//         });

//         Userinfo userInfo = await service.Userinfo.Get().ExecuteAsync();

//         // ユーザー情報とアクセストークンを返します。
//         return (userInfo, accessToken);
//     }

// }

// // ここで NullDataStore を使用してキャッシュされた認証情報を使用しないように設定
// [System.Serializable]
// public class GoogleCredentials
// {
//     public Installed installed;

//     [System.Serializable]
//     public class Installed
//     {
//         public string client_id;
//         public string client_secret;
//         // 他のフィールドも必要に応じて追加...
//     }
// }

// [System.Serializable]
// public class GASResponse
// {
//     public string email;
//     public string orgUnitPath;
//     public string contentSheet;
// }

// [System.Serializable]
// public class ServiceAccountData
// {
//     public string type;
//     public string project_id;
//     public string private_key_id;
//     public string private_key;
//     public string client_email;
//     public string client_id;
//     public string auth_uri;
//     public string token_uri;
//     public string auth_provider_x509_cert_url;
//     public string client_x509_cert_url;
//     public string universe_domain;
// }