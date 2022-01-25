using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class Walk : MoveType
    {
        // maximum speed we can accelerate to
        public bool Strafe;

        public float MaxAccelSpeed = 10;

        private float xRef;
        private float yRef;

        [SerializeField]
        private float turnaroundTimer;

        public override void OnFixedUpdate()
        {
            if (!Strafe)
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
            }
            else
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
            }

            if (!character.Grounded)
            {
                //rb.velocity += Vector3.up * 0.1f;
                character.SwitchToState(CharacterState.Airborne);
                return;
            }

            if (Strafe)
            {
                // if standing dont rotate
                if (rb.velocity.magnitude > 0.1f)
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(character.TargetForward, Vector3.up), 0.2f);
            }
            else
            {
                // rotation
                if (rb.velocity.magnitude > (turnaroundTimer > 0 ? 3f : 0.2f))
                {
                    Quaternion targetRot = Quaternion.LookRotation(Vector3.Scale(rb.velocity, Vector3.one - Vector3.up).normalized, Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 0.6f);

                    turnaroundTimer = 0.5f;
                }
                else if (character.WASD.magnitude > 0.1f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(
                        character.TargetForward * character.WASD.y + character.TargetRight * character.WASD.x,
                        Vector3.up);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 0.2f);
                }

                turnaroundTimer -= Time.deltaTime;
            }
        }
    }
}
