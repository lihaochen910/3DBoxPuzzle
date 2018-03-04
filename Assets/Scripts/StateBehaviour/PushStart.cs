using UnityEngine;

namespace PlayerStateMachineBehaviour
{
    public class PushStart : StateMachineBehaviour
    {
        private bool _toPush;
        private bool _toPushStop;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _toPush = _toPushStop = false;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetBool("isPushing", false);

            //if (_toPush)
            //{
            //    animator.SetBool("isPushing", false);
            //    animator.SetTrigger("Push");
            //}
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (Input.GetButtonUp("Push") && !_toPush && !_toPushStop)
            {
                animator.SetBool("isPushing", false);
                _toPushStop = true;
            }

            if (Input.GetAxisRaw("Vertical") != 0 && !_toPushStop && !_toPush)
            {
                animator.SetTrigger("Push");
                _toPush = true;
            }
        }
    }
}
