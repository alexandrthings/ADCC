using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class RootMotionAttack : MoveType
    {
        private Vector3 startOffset;
        private Vector3 lastPos;

        private Vector3 LastMove;

        public override void Begin()
        {
            if (character == null)
                Start();

            lastPos = character.animator.Root.localPosition;

            TimeInState = 0;
        }

        public override void Start()
        {
            base.Start();

            character = transform.GetComponent<Character>();
            rb = transform.GetComponent<Rigidbody>();

            startOffset = character.animator.Root.localPosition;
            //animator = character.animator;
        }

        public override void OnFixedUpdate()
        {
            if (Mathf.Abs(character.WASD.x) < 0.9f && Mathf.Abs(character.WASD.y) < 0.9f)
                return;

            LastMove = character.TargetForward * character.WASD.y + character.TargetRight * character.WASD.x;

            rb.AddForce(-rb.velocity * MoveForce * Time.fixedDeltaTime);
        }

        public void LateUpdate()
        {
            TimeInState += Time.deltaTime;

            if (!Run)
                return;

            //Vector3 offset = character.animator.Root.position - lastPos;
            //lastPos = character.animator.Root.position;

            //transform.position += offset;

            character.animator.Root.localPosition = startOffset;
        }
    }
}