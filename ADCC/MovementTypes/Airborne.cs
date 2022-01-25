using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class Airborne : MoveType
    {
        public bool Strafe;

        public float MaxAccelSpeed;
        public float extraGravity;

        public bool CheckForClamber = false;

        public Vector3 wallCheckSize;
        public LayerMask wallCheckMask;

        public float wallCheckForward;
        public float wallCheckUp;

        public float wallCheckDown;

        public override void OnFixedUpdate()
        {
            if (!Strafe)
            {
                Vector3 dirInput = (transform.forward * character.WASD.y + transform.right * character.WASD.x);

                if (character.WASD.magnitude < 0.2f)
                    dirInput = transform.forward;

                Quaternion targetRot =
                    Quaternion.LookRotation(
                        new Vector3(rb.velocity.x, 0, rb.velocity.z) + dirInput*2, Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 0.2f);
            }
            else
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(character.TargetForward, Vector3.up), 0.2f);

            // change velocity
            Forces();

            if (character.Grounded && rb.velocity.y < -0.1f)
            {
                character.SwitchToState(CharacterState.Walking);
                return;
            }

            if (CheckForClamber)
                CheckForLedge();
        }

        void Forces()
        {
            // movement
            float curaccel = Mathf.Clamp01(Mathf.Abs(character.WASD.x) + Mathf.Abs(character.WASD.y));

            Vector3 move =
                ((character.TargetForward * character.WASD.y + character.TargetRight * character.WASD.x)
                    .normalized) * curaccel * MaxAccelSpeed - rb.velocity;

            move = Vector3.Scale(move, Vector3.forward + Vector3.right);

            if (rb.velocity.magnitude > MaxAccelSpeed && Vector3.Dot(rb.velocity, move) > 0)
            {
                move = Vector3.ProjectOnPlane(move, -rb.velocity.normalized);
            }

            rb.AddForce(move * MoveForce * Time.fixedDeltaTime);

            // extra gravity
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
        }

        public void CheckForLedge()
        {
            RaycastHit topHit;

            if (Physics.BoxCast(transform.position + transform.forward * wallCheckForward + transform.up * wallCheckUp, wallCheckSize, Vector3.down, out topHit, transform.rotation, wallCheckDown, wallCheckMask))
            {
                if (Vector3.Dot(topHit.normal, Vector3.up) < 0.8f)
                    return;

                Vector3 direction = Vector3.Scale(topHit.point - transform.position, Vector3.right + Vector3.forward);

                RaycastHit hitSide;
                Physics.Raycast(new Vector3(transform.position.x, topHit.point.y - 0.1f, transform.position.z), direction, out hitSide, wallCheckDown, wallCheckMask);

                character.Clamber(topHit.point, -hitSide.normal);
            }

        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + transform.forward * wallCheckForward + transform.up * wallCheckUp, wallCheckSize*2);
            Gizmos.DrawLine(transform.position + transform.forward * wallCheckForward + transform.up * wallCheckUp, transform.position + transform.forward * wallCheckForward + transform.up * wallCheckUp - transform.up * wallCheckDown);
        }
    }
}