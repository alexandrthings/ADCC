using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class Dodge : MoveType
    {
        public float maxSpeed;
        public float duration;
        /*[HideInInspector]*/ public Vector2 entryInput;

        public LayerMask HaltMask;

        public override void Begin()
        {
            base.Begin();

            entryInput = character.WASD;

            if (entryInput.magnitude < 0.1f)
            {
                entryInput = transform.forward;
            }

            rb.AddForce((character.TargetForward * entryInput.y + character.TargetRight * entryInput.x) * MoveForce /
                        2);

            StartCoroutine(AccelerateForTime((character.TargetForward * entryInput.y + character.TargetRight * entryInput.x) * maxSpeed, duration));
        }

        public override void Start()
        {
            base.Start();

        }

        public override void OnUpdate()
        {
            Quaternion targetRot = Quaternion.LookRotation(character.TargetRight * entryInput.x + character.TargetForward * entryInput.y, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 0.2f);
        }
    }
}