using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kinoko : MonoBehaviour
{
    private Animator kAnimator;
    public Camera mainCamera;
    public int offsetX = 70; // 左端からのオフセット（ピクセル単位）

    void Start()
    {
        kAnimator = GetComponent<Animator>(); // kinokoのアニメーターを取得
        // スクリーン座標での位置を計算（YとZは現在のオブジェクトの座標を使用）
/*        Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position);
        screenPosition.x = offsetX;

        // スクリーン座標をワールド座標に変換
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        // オブジェクトの位置を設定（YとZは元の値を保持）
        transform.position = new Vector3(worldPosition.x, transform.position.y, transform.position.z);
*/    }

/*    void Update()
    {
        // スクリーン座標での位置を計算（YとZは現在のオブジェクトの座標を使用）
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position);
        screenPosition.x = offsetX;

        // スクリーン座標をワールド座標に変換
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        // オブジェクトの位置を設定（YとZは元の値を保持）
        transform.position = new Vector3(worldPosition.x, transform.position.y, transform.position.z);

    }
*/    
    void OnMouseDown()
    {
        kAnimator.SetTrigger("jump");
    }
}
