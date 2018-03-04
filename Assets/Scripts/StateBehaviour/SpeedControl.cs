using UnityEngine;

namespace PlayerStateMachineBehaviour
{
    public class SpeedControl : StateMachineBehaviour
    {
        [Range(0.1f, 5f)]
        public float TargetSpeed;

        private float _oldSpeed;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _oldSpeed = animator.speed;

            animator.speed = TargetSpeed;
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.speed = _oldSpeed;
        }
    }
}
