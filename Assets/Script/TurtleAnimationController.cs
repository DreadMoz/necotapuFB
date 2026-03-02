using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleAnimationController : MonoBehaviour
{
    private Animator animator;

    [Header("カメが隠れている（Hide）時間")]
    public float minHideTime = 8f;
    public float maxHideTime = 12f;

    [Header("カメが顔を出している（UnHide）時間")]
    public float minShowTime = 0.5f;
    public float maxShowTime = 2f;

    private float timer = 0f;
    private float nextActionTime = 0f;
    private bool isUnHide = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // 最初の状態をHide（UnHide=false）に設定し、最初の待機時間を決める
        isUnHide = false;
        if (animator != null)
        {
            animator.SetBool("UnHide", isUnHide);
        }
        
        SetNextActionTime();
    }

    void Update()
    {
        if (animator == null) return;

        timer += Time.deltaTime;

        if (timer >= nextActionTime)
        {
            timer = 0f;
            isUnHide = !isUnHide; // 状態を反転
            
            animator.SetBool("UnHide", isUnHide);

            SetNextActionTime();
        }
    }

    private void SetNextActionTime()
    {
        if (isUnHide)
        {
            // 顔を出している状態の待機時間を設定
            nextActionTime = Random.Range(minShowTime, maxShowTime);
        }
        else
        {
            // 隠れている状態の待機時間を設定
            nextActionTime = Random.Range(minHideTime, maxHideTime);
        }
    }
}
