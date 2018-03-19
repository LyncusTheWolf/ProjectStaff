using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic {
	public class CameraRig : MonoBehaviour {

        public const float DEAD_ZONE = 0.05f;

        public CharacterMotor motor;

        public float startingZoom;
        public float zoomMinClamp = 5.0f;
        public float zoomMaxClamp = 20.0f;

        public float easing = 5.0f;
        public float yawEasing = 5.0f;

        public float yawControlSpeed = 3.0f;
        public float pitchControlSpeed = 3.0f;

        [Range(0.0f, 1.0f)]
        public float rotBias = 1.0f;

        public float startingPitch = 15.0f;
        public float pitchMinClamp = -15.0f;
        public float pitchMaxClamp = 60.0f;

        [Header("Occlusion Controls")]
        public LayerMask occlusionMask;
        public float normalOffset;

        private float currentYaw;
        private float currentPitch;
        private float currentZoom;
        private Transform followTarget;

        void Awake(){
            ResetCamera(motor, true);
        }

        public void Update() {
            CameraControl(Time.deltaTime);

            if (Input.GetButtonDown("R3")) {
                ResetCamera(motor, true);
            }
        }

        // Update is called once per frame
        void FixedUpdate() {
            CameraControl(Time.fixedDeltaTime);
        }

        private void CameraControl(float timeDelta) {
            float cameraHorizontal = -Input.GetAxis("RightHorizontal");
            float cameraVertical = -Input.GetAxis("RightVertical");
            float dHorizontal = Input.GetAxis("D Horizontal");
            float dVertical = Input.GetAxis("D Vertical");

            currentZoom = Mathf.Clamp(currentZoom + (dVertical * timeDelta * 3.0f), zoomMinClamp, zoomMaxClamp);

            if (Mathf.Abs(cameraHorizontal) > DEAD_ZONE) {
                currentYaw = Mathf.LerpAngle(currentYaw, currentYaw + cameraHorizontal * yawControlSpeed, timeDelta);
            } else {
                Vector3 viewTemp = transform.forward;
                viewTemp.y = 0;

                float val = Vector3.Dot(motor.CurrentFrameInput.moveDir, viewTemp);
                float factor = ((val + 1) * 0.5f * rotBias) + (1 - rotBias);

                currentYaw = Mathf.LerpAngle(currentYaw, followTarget.rotation.eulerAngles.y, yawEasing * timeDelta * motor.CurrentFrameInput.currentMoveMagnitude * factor);
            }

            if (Mathf.Abs(cameraVertical) > DEAD_ZONE) {
                currentPitch = Mathf.LerpAngle(currentPitch, currentPitch + cameraVertical * pitchControlSpeed, timeDelta);
            } else {
                currentPitch = Mathf.LerpAngle(currentPitch, startingPitch, yawEasing * timeDelta * motor.CurrentFrameInput.currentMoveMagnitude);
            }

            currentPitch = Mathf.Clamp(currentPitch, pitchMinClamp, pitchMaxClamp);

            Vector3 targetPosition = followTarget.position - (Quaternion.Euler(currentPitch, currentYaw, 0.0f) * Vector3.forward) * currentZoom;

            PushPosition(targetPosition, timeDelta);
            transform.LookAt(followTarget);
        }

        public void PushPosition(Vector3 targetPos, float timeDelta) {

            RaycastHit hit;

            Ray hitRay = new Ray(followTarget.position, targetPos - followTarget.position);

            Debug.DrawRay(followTarget.position, targetPos - followTarget.position, Color.blue);

            if (Physics.Raycast(hitRay, out hit, currentZoom, occlusionMask, QueryTriggerInteraction.Ignore)) {
                transform.position = hit.point + hit.normal * normalOffset;
            } else {
                transform.position = Vector3.Slerp(transform.position, targetPos, Time.deltaTime * easing);
            }

        }

        public void ResetCamera(CharacterMotor newMotor, bool snapToPosition) {
            if (newMotor != null) {
                motor = newMotor;
                followTarget = motor.focalPoint;
            }

            currentYaw = followTarget.transform.rotation.eulerAngles.y;
            currentPitch = startingPitch;
            currentZoom = startingZoom;

            if (snapToPosition) {
                transform.position = followTarget.position - (Quaternion.Euler(currentPitch, currentYaw, 0.0f) * Vector3.forward) * startingZoom;
                transform.LookAt(followTarget);
            }
        }
    }
}
