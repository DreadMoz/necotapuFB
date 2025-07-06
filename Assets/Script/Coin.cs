using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{    public PhysicsMaterial2D lowBouncinessMaterial; // 反発係数が低いマテリアル
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("CoinBase")) { 
            // 反発係数が低いマテリアルを適用する
            GetComponent<Collider2D>().sharedMaterial = lowBouncinessMaterial;
        }
    }
}
