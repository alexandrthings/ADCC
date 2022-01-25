using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    /// <summary>
    /// Base class for all input modules. Input can come from an ai or a player.
    /// </summary>
    public class InputModule : MonoBehaviour
    {
        protected Character myCharacter;

        public virtual void Start()
        {
            myCharacter = transform.GetComponent<Character>();
        }

        // Should override this
        public virtual void Update()
        {
            myCharacter.WASDQE = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1));
        }
    }
}