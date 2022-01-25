using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework.Camera
{
    public class ScrollableCameraRig : MonoBehaviour
    {
        public static ScrollableCameraRig instance;

        [SerializeField] private Vector3 firstPersonPos;
        [SerializeField] private Vector3 thirdPersonPos;
        [SerializeField] private Transform interpolator;

        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float posLimit, negLimit;
        [SerializeField] private float sensitivity = 300;

        [SerializeField] private float posSmoothing = 0.3f;
        [SerializeField] private float smoothing = 0.3f;

        private float yaw, pitch, transition, transitionTgt;
        [SerializeField] private float transitionRef;
        private Vector2 recoilDebt = new Vector2(0,0);

        private bool thirdPerson = false;

        void Start()
        {
            instance = this;
            sensitivity = 250;
        }

        public void Update()
        {
            if (target == null)
                return;

            transform.position = Vector3.Slerp(transform.position, target.position + offset, posSmoothing);
            //transform.position = target.position + offset;

            yaw += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

            pitch = Mathf.Clamp(pitch, negLimit, posLimit);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(pitch + recoilDebt.y, yaw + recoilDebt.x, 0), smoothing);

            transitionTgt = Mathf.Clamp01(transitionTgt - Input.GetAxis("Mouse ScrollWheel"));

            if (!thirdPerson && transitionTgt > 0.1f)
            {
                thirdPerson = true;
                transitionTgt = 0.31f;
            }
            else if (thirdPerson && transitionTgt < 0.3f)
            {
                thirdPerson = false;
                transitionTgt = 0;
            }

            transition = Mathf.SmoothDamp(transition, transitionTgt, ref transitionRef, 0.1f);

            interpolator.localPosition = Vector3.Lerp(firstPersonPos, thirdPersonPos, transition);

            //transform.eulerAngles = new Vector3(pitch, yaw, 0);
        }

        public static void SetTarget(Transform target)
        {
            instance.target = target;
        }
    }
}