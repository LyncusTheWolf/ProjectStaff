using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic {

    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour {

        public const int LOCOMOTION_LAYER = 0;
        public const int UPPER_BODY_LAYER = 1;
        public const int ACTION_LAYER = 2;

        public const string LEFT_HAND_PARAM_NAME = "LeftHand";
        public const string RIGHT_HAND_PARAM_NAME = "RightHand";
        public const string LEFT_FOOT_PARAM_NAME = "LeftFoot";
        public const string RIGHT_FOOT_PARAM_NAME = "RightFoot";

        [System.Serializable]
        public struct IKSet {
            public Transform transform;
            public Vector3 point;
            public Quaternion rotation;
            public Vector3 hint;
            public float weight;
            public float offset;
            public AvatarIKGoal goalType;
            public AvatarIKHint hintType;
        }

        public float animationDamping = 0.4f;

        public float turnLeaning = 0.3f;
        public float turnEasing = 3.0f;

        public float handIKOffset = 0.2f;
        public float footIKOffset = 0.18f;
        public float snapRadius = 0.5f;
        //Default and Enviornment
        public LayerMask ikLayerMask;

        [Header("Weapon animation values")]
        [SerializeField]
        private WeaponAnimator weaponAnimator;
        [SerializeField]
        private Transform weaponPassiveSocket;
        [SerializeField]
        private Transform weaponActiveSocket;
        [SerializeField] [Range(0.0f, 1.0f)]
        private float weaponSnapCoefficient = 0.5f;

        private Transform weaponTransform;
        private Animator anim;
        private CharacterMotor motorRoot;

        private IKSet leftHandIKs;
        private IKSet rightHandIKs;
        private IKSet leftFootIKs;
        private IKSet rightFootIKs;

        private int speedParamHash;
        private int yVelocityHash;
        private int directionHash;
        private int groundedHash;
        private int jumpHash;
        private int weaponLengthHash;
        private int equipStateHash;
        private int leftHandIdHash;
        private int rightHandIdHash;
        private int leftFootIdHash;
        private int rightFootIdHash;

        public Animator Anim {
            get { return anim; }
        }

        void Awake() {
            anim = GetComponent<Animator>();
            weaponTransform = weaponAnimator.transform;

            speedParamHash = Animator.StringToHash("Speed");
            yVelocityHash = Animator.StringToHash("Y Velocity");
            directionHash = Animator.StringToHash("Direction");
            groundedHash = Animator.StringToHash("IsGrounded");
            jumpHash = Animator.StringToHash("Jump");
            weaponLengthHash = Animator.StringToHash("Weapon Length");
            equipStateHash = Animator.StringToHash("IsEquipped");
            leftHandIdHash = Animator.StringToHash(LEFT_HAND_PARAM_NAME);
            rightHandIdHash = Animator.StringToHash(RIGHT_HAND_PARAM_NAME);
            leftFootIdHash = Animator.StringToHash(LEFT_FOOT_PARAM_NAME);
            rightFootIdHash = Animator.StringToHash(RIGHT_FOOT_PARAM_NAME);
        }

        // Use this for initialization
        void Start(){
            InitIKs();
        }

		// Update is called once per frame
		void Update(){
            anim.applyRootMotion = anim.GetBool("UseRootMotion");
		}

        public void RegisterRoot(CharacterMotor newRoot) {
            motorRoot = newRoot;
        }


        private void InitIKs() {
            //anim.GetIKPosition(AvatarIKGoal.LeftFoot);
            SetupIKSet(out leftHandIKs, HumanBodyBones.LeftHand, AvatarIKGoal.LeftHand, AvatarIKHint.LeftElbow, handIKOffset);
            SetupIKSet(out rightHandIKs, HumanBodyBones.RightHand, AvatarIKGoal.RightHand, AvatarIKHint.RightElbow, handIKOffset);
            SetupIKSet(out leftFootIKs, HumanBodyBones.LeftFoot, AvatarIKGoal.LeftFoot, AvatarIKHint.LeftKnee, footIKOffset);
            SetupIKSet(out rightFootIKs, HumanBodyBones.RightFoot, AvatarIKGoal.RightFoot, AvatarIKHint.RightKnee, footIKOffset);
        }

        public void SetupIKSet(out IKSet ikInfo, HumanBodyBones boneType, AvatarIKGoal ikGoal, AvatarIKHint ikHint, float offset) {
            ikInfo.transform = anim.GetBoneTransform(boneType);

            if(ikInfo.transform == null) {
                Debug.Log(boneType + " is null");
            }

            //Set defaults to the current positions of each bone
            ikInfo.point = ikInfo.transform.position;
            ikInfo.rotation = ikInfo.transform.rotation;
            ikInfo.hint = anim.GetIKHintPosition(ikHint);

            //Set goal and hint types
            ikInfo.goalType = ikGoal;
            ikInfo.hintType = ikHint;

            ikInfo.weight = 0.0f;
            ikInfo.offset = offset;
        }

        public void OnAnimatorMove() {            
            if (anim.applyRootMotion) {
                //Debug.Log(anim.rootPosition);
                //Debug.Log("X: " + anim.deltaPosition.x);
                //Debug.Log("Y: " + anim.deltaPosition.y);
                //Debug.Log("Z: " + anim.deltaPosition.z);
                motorRoot.ControllerMove(anim.deltaRotation, anim.deltaPosition);
                //motorRoot.transform.position = anim.rootPosition;
            }
        }

        public void Tick(float delta, CharacterMotor.MotorInput inputs, Vector3 velocity) {
            //anim.ResetTrigger(jumpHash);

            if (inputs.isEquipped) {
                weaponTransform.position = weaponActiveSocket.position;
                weaponTransform.rotation = weaponActiveSocket.rotation;
            } else {
                weaponTransform.position = Vector3.Lerp(weaponTransform.position, weaponPassiveSocket.position, weaponSnapCoefficient);
                weaponTransform.rotation = Quaternion.Lerp(weaponTransform.rotation, weaponPassiveSocket.rotation, weaponSnapCoefficient);
            }

            transform.localRotation = Quaternion.Lerp(
                transform.localRotation, 
                (inputs.isGrounded || inputs.inAction) ? Quaternion.Euler(0.0f, 0.0f, inputs.currentDirection * -turnLeaning) : Quaternion.identity, 
                turnEasing * delta);

            if (inputs.toggleEquip) {
                if (inputs.isEquipped) {
                    UnEquipWeapon();
                } else {
                    EquipWeapon();
                }
            }

            anim.SetFloat(yVelocityHash, velocity.y, animationDamping, Time.deltaTime);
            anim.SetFloat(speedParamHash, inputs.currentMoveMagnitude);
            anim.SetBool(equipStateHash, inputs.isEquipped);
            //anim.SetFloat(directionHash, inputs.currentDirection);
            anim.SetBool(groundedHash, inputs.isGrounded);
            //anim.applyRootMotion = inputs.isGrounded;
            if (inputs.jumpTrigger) {
                //anim.Play("Jumps", ACTION_LAYER, 0.0f);
                anim.CrossFade("Jumps", 0.1f, ACTION_LAYER);
            }

            weaponAnimator.WeaponLength = anim.GetFloat(weaponLengthHash);
        }

        public void JumpEvent(AnimationEvent animEvent) {
            //Debug.Log("Jump event called");
            if (animEvent.animatorClipInfo.weight > 0.5f) {
                //Debug.Log((anim.deltaPosition / Time.deltaTime).magnitude);
                motorRoot.Jump((anim.deltaPosition / Time.deltaTime).magnitude);
            }
        }

        public void PushAnimAction(string actionName, float normalizedTransition = 0.1f) {
            Debug.Log("Setting anim action: " + actionName);
            anim.CrossFade(actionName, normalizedTransition, ACTION_LAYER);
        }

        public void Attack(string attackName, WeaponAnimator.WeaponAction attackAction) {
            PushAnimAction(attackName, 0.1f);

            weaponAnimator.SetAction(attackAction);
        }

        public void OnAnimatorIK(int layerIndex) {
            //For now handle all layers, exclude some later
            leftHandIKs.weight = anim.GetFloat(leftHandIdHash);
            rightHandIKs.weight = anim.GetFloat(rightHandIdHash);
            leftFootIKs.weight = anim.GetFloat(leftFootIdHash);
            rightFootIKs.weight = anim.GetFloat(rightFootIdHash);

            ApplyIK(leftHandIKs);
            ApplyIK(rightHandIKs);
            ApplyIK(leftFootIKs);
            ApplyIK(rightFootIKs);
        }

        public void ChangeEquipStateEvent(int newState) {
            motorRoot.PushNewEquipState(newState == 1);
        }

        public void EquipWeapon() {
            anim.Play("Idle To Combat", UPPER_BODY_LAYER);
        }

        public void UnEquipWeapon() {
            anim.Play("Combat to Idle", UPPER_BODY_LAYER);
        }

        public void PushCopter() {
            anim.CrossFade("Copter Loop", 0.1f, ACTION_LAYER);
        }

        public void OpenWeaponCollider() {
            weaponAnimator.OpenWeaponCollider();
        }

        public void CloseWeaponCollider() {
            weaponAnimator.CloseWeaponCollider();
        }

        public void ApplyIK(IKSet ikInfo) {
            //Don't bother performing ik checks for stuff that won't be IK blended
            if (ikInfo.weight <= 0.0f) {
                return;
            }

            RaycastHit hit;

            if(ikInfo.transform == null) {
                Debug.Log(ikInfo.goalType + " is null");
            }

            /*if(Physics.SphereCast(ikInfo.transform.position, ikInfo.offset, ikInfo.transform.forward, out hit, ikInfo.offset, ikLayerMask)) {
                anim.SetIKPosition(ikInfo.goalType, hit.point + hit.normal * ikInfo.offset);
                anim.SetIKRotation(ikInfo.goalType, Quaternion.LookRotation(hit.normal, ikInfo.transform.forward));
                ikInfo.weight = 1.0f;
            } else {
                Debug.Log("No collisions for " + ikInfo.transform.name);
            }*/

            if (Physics.Raycast(ikInfo.transform.position, Vector3.down, out hit, snapRadius, ikLayerMask)) {
                anim.SetIKPosition(ikInfo.goalType, hit.point + hit.normal * ikInfo.offset);
                ikInfo.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                anim.SetIKRotation(ikInfo.goalType, ikInfo.rotation);
                //ikInfo.weight = 1.0f;
            }

            anim.SetIKPositionWeight(ikInfo.goalType, ikInfo.weight);
            anim.SetIKRotationWeight(ikInfo.goalType, ikInfo.weight);
            anim.SetIKHintPositionWeight(ikInfo.hintType, ikInfo.weight);
        }

        public void OnDrawGizmos() {
            Gizmos.color = Color.red;
            RenderIKGizmos(leftHandIKs);
            RenderIKGizmos(rightHandIKs);
            RenderIKGizmos(leftFootIKs);
            RenderIKGizmos(rightFootIKs);
        }

        public void RenderIKGizmos(IKSet ikInfo) {
            if(ikInfo.transform == null) {
                return;
            }

            Gizmos.DrawWireSphere(ikInfo.transform.position, ikInfo.offset);
        }
    }
}
