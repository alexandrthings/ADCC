using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class Clamber : MoveType
    {
        public Vector3 target;
        public Vector3 direction;

        public override void Begin()
        {
            base.Begin();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void OnFixedUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, target, 0.1f);
            rb.velocity = Vector3.zero;

            if (TimeInState > 0.6f)
            {
                character.SwitchToNeutralState();
            }
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target, 0.1f);
            Gizmos.DrawLine(target, target + direction*2f);
        }
    }
}