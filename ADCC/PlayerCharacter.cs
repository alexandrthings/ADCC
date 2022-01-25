using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class PlayerCharacter : Character
    {
        public static PlayerCharacter pc;

        public override void Start()
        { 
            base.Start();

            inputModule = gameObject.GetComponent<PlayerInputModule>();
        }

        public override void Update()
        {
            TargetForward = Vector3.Scale(TargetDir, Vector3.one - Vector3.up).normalized;
            TargetRight = Vector3.Cross(Vector3.up, TargetForward).normalized;

            ActionChecks();

            debugState = state;
        }

        // You can write player-specific functions in this

#if UNITY_EDITOR
        public override void OnDrawGizmos()
        {
            Gizmos.DrawSphere(target, 0.4f);
            base.OnDrawGizmos();
        }
#endif
    }
}
