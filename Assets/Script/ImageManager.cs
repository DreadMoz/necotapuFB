using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using System.Collections; // この行を追加

public class ImageManager : MonoBehaviour
{
    public RawImage imageInstance;

    public IEnumerator UpdateImage(string newImageUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(newImageUrl);

        // 画像を取得できるまで待つ
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            // 取得した画像のテクスチャをRawImageのテクスチャに設定する
            imageInstance.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }
}