using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class Jump : MoveType
    {
        [SerializeField] private int jumpCount;
        private int jumpsUsed;
        [SerializeField] private float jumpForce;
        [Tooltip("How many seconds after input the jump is")]
        [SerializeField] private float jumpDelay;
        [Tooltip("How many seconds after jump to transition to airborne")]
        [SerializeField] private float transitionDelay;

        public override void Begin()
        {
            base.Begin();

            if (character.Grounded)
                jumpsUsed = 0;

            if (jumpsUsed < jumpCount)
                StartCoroutine(DelayedJump());
            else
                character.SwitchToNeutralState();
        }

        IEnumerator DelayedJump()
        {
            yield return new WaitForSeconds(jumpDelay);

            jumpsUsed++;

            rb.velocity = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
            rb.AddForce(Vector3.up * jumpForce);

            yield return new WaitForSeconds(transitionDelay);

            character.SwitchToNeutralState();
        }
    }
}