using UnityEngine;

namespace PlayerStateMachineBehaviour
{
    public class Push : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("isPushing", false);
        }
    }
}
