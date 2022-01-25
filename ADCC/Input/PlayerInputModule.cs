using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Characters
{
    public class PlayerInputModule : InputModule
    {
        public LayerMask CameraLookMask;
        public bool DebugMode;
        public PlayerCharacter myPC;

        public override void Start()
        {
            CameraLookMask = LayerMask.GetMask("Default");

            myCharacter = transform.GetComponent<PlayerCharacter>();
            myPC = transform.GetComponent<PlayerCharacter>();
        }

        public override void Update()
        {
            myPC.WASD = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            myPC.WASDQE = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0); // would add QE here
            myPC.TargetDir = UnityEngine.Camera.main.transform.forward;

            var ray = UnityEngine.Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit camhit;

            Vector3 tgt;

            // setting target to where player is looking
            if (Physics.Raycast(ray, out camhit, 1000, CameraLookMask))
            {
                tgt = camhit.point;
            }
            else
            {
                tgt = UnityEngine.Camera.main.gameObject.transform.position +
                      UnityEngine.Camera.main.gameObject.transform.forward * 1000;
            }

            myPC.target = tgt;

            // write your animation queueing here
            if (Input.GetButtonDown("Fire1"))
                myPC.QueueAnimation("Attack");

            if (Input.GetKeyDown(KeyCode.LeftShift))
                myPC.ToggleSprint();
                
            if (Input.GetButtonDown("Jump"))
                myPC.QueueAnimation("Jump");

        }
    }
}