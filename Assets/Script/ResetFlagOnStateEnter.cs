using UnityEngine;

/// <summary>
/// アニメーションステートに入ったときにフラグをリセットするStateMachineBehaviour
/// </summary>
public class ResetFlagOnStateEnter : StateMachineBehaviour
{
    [Tooltip("リセットするAnimatorのIntegerパラメータ名")]
    public string parameterName = "yubi";

    /// <summary>
    /// ステートに入ったときに呼ばれる
    /// </summary>
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 指定されたパラメータを0にリセット
        animator.SetInteger(parameterName, 0);
    }
}
