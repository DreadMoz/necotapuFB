using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();  // アニメーターを取得
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTest1()
    {
        animator.SetTrigger("dice1");
    }
}
