using UnityEngine;

namespace PlayerStateMachineBehaviour {
    public class Idle : StateMachineBehaviour
    {
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Input.GetButtonDown("Push"))
                animator.SetBool("isPushing", true);
        }
    }
}
