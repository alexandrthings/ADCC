using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class Sit : MoveType
    {
        private Transform seat;

        public override void Begin()
        {
            base.Begin();

            rb.isKinematic = true;

            transform.parent = seat.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            //character.SetIK(seat.leftIK, seat.rightIK);
        }

        public override void Start()
        {
            base.Start();
        }

        public override void End()
        {
            //character.SetIK(null, null);
        }
    }
}