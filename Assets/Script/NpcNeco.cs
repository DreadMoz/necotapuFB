using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class NpcNeco : MonoBehaviour
{
    private Animator animator;
    private NavMeshAgent agent; // NavMeshAgentへの参照
    private Rigidbody rb;
    public float radius = 16f; // ランダムな目的地を探す範囲
    private float speed = 5f;
    public float interval = 8f;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        animator.SetInteger("animation", 10);

        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
        agent.stoppingDistance = 0.4f; // 停止と判定する距離

        // ◯秒後に最初の移動を開始し、その後◯秒ごとに繰り返し実行
        InvokeRepeating("SetRandomDestination", Random.Range(1f, interval), interval);
    }

    void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += transform.position; // 現在位置からの相対位置を計算

        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            finalPosition = hit.position;
            agent.SetDestination(finalPosition);

            // 距離に基づいてアニメーションを設定
            float distance = Vector3.Distance(transform.position, finalPosition);
            if (distance < 0.5f * radius)        // 待機
            {
                // 目的地を解除し、エージェントの移動を停止します。
                agent.ResetPath(); // 目的地を解除する
                agent.velocity = Vector3.zero; // 速度を0にする
                // ランダムな整数を生成して アクションを決める。
                int randomIndex = Random.Range(0, 12);
                if (randomIndex == 0)
                {
                    animator.SetTrigger("dig");
                }
                else if (randomIndex <= 2)
                {
                    animator.SetTrigger("sit");
                }
                else if (randomIndex <= 5)
                {
                    animator.SetTrigger("idol3");
                }
                else if (randomIndex <= 6)
                {
                    animator.SetTrigger("idolB");
                }
                else
                {
                    animator.SetBool("idol", true);
                }
                return;
            }
            else if (0.95f * radius < distance)   // 走る
            {
                animator.SetBool("idol", false);
                animator.SetTrigger("run");
                agent.speed = speed;
            }
            else                        // 歩く
            {
                animator.SetBool("idol", false);
                animator.SetTrigger("walk");
                agent.speed = speed / 3;
            }
        }
    }
    void Update()
    {
        // 目的地に到着したかどうかを確認
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            animator.SetBool("idol", true);
            agent.velocity = Vector3.zero;
            rb.velocity = Vector3.zero; // 速度を0にする
            rb.angularVelocity = Vector3.zero; // 角速度を0にする
        }
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name != "Terrain")
        {
            rb.velocity = Vector3.zero; // 速度を0にする
            rb.angularVelocity = Vector3.zero; // 角速度を0にする
            agent.ResetPath(); // 目的地を解除する
            if (col.gameObject.name == "Player")
            {
                // ランダムな整数を生成
                int randomIndex = Random.Range(0, 5);
                animator.SetInteger("animation", randomIndex);
            }
            else if (col.gameObject.CompareTag("InvisibleFence"))
            {
                animator.SetInteger("animation", 3);    // 怒る
            }
            else
            {
                // 現在の向きから少し後ろの位置を計算
                Vector3 stepBackDestination = transform.position - transform.forward * 0.7f;

                // エージェントに新しい目的地を設定して、後ろに歩かせる
                agent.SetDestination(stepBackDestination);       // 少し戻る（向きを反転）
            }
        }
    }

    void OnCollisionExit(Collision col)
    {
        animator.SetInteger("animation", 10);
    }
}
