using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    /// <summary>
    /// The base class for all movement types. You can override Begin(), End(), OnUpdate(), and OnFixedUpdate().
    /// </summary>
    public abstract class MoveType : MonoBehaviour
    {
        [HideInInspector] public Character character;
        [HideInInspector] public Rigidbody rb;

        public CharacterState ThisState;
        [HideInInspector] public CharacterState PrevState;

        //public float Softcap = 10f;
        //public float LinearDrag = 5f;
        public float MoveForce = 1500f;

        public float TimeInState;

        public bool Run;

        #region Unity Callbacks
        public virtual void Start()
        {
            character = transform.GetComponent<Character>();
            rb = transform.GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!Run)
                return;

            OnFixedUpdate();
        }

        private void Update()
        {
            if (!Run)
                return;

            TimeInState += Time.deltaTime;

            OnUpdate();
        }
        #endregion

        #region Movement State Callbacks
        /// <summary>
        /// Called when character enters state. Be sure to call base.Begin();
        /// </summary>
        public virtual void Begin()
        {
            TimeInState = 0;
        }

        /// <summary>
        /// Called when character exits state.;
        /// </summary>
        public virtual void End() { }

        /// <summary>
        /// Runs every normal frame update while the movement state is active.
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Runs every fixed frame update while the movement state is active.
        /// </summary>
        public virtual void OnFixedUpdate() { }
        #endregion

        /// <summary>
        ///  A possibly useful coroutine that accelerates a character at certain velocity for a specified time.
        /// </summary>
        public virtual IEnumerator AccelerateForTime(Vector3 velocity, float time)
        {
            float t = 0;
            while (t < time && Run)
            {
                // movement
                Vector3 move = velocity - rb.velocity;

                if (rb.velocity.magnitude > velocity.magnitude)
                {
                    move = Vector3.ProjectOnPlane(move, -rb.velocity.normalized);
                }

                rb.AddForce(move * MoveForce * Time.fixedDeltaTime);

                t += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }

    }

}
