using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basic{
    [RequireComponent(typeof(Rigidbody))]
	public class CharacterMotor : MonoBehaviour {

        #region Constants and Structs
        public const float INPUT_DEAD_ZONE = 0.05f;                 //The percentage the input value of any axis has to exceed in order
                                                                    //to pass a dead zone check
        
        public const string IN_ACTION_ID = "InAction";              //String containing the param for the InAction ID of the animator

        //Struct containing all the current input states for the motor
        [System.Serializable]
        public struct MotorInput {
            public Vector3 moveDir;
            public float inputMagnitude;
            public float currentMoveMagnitude;
            public float currentDirection;
            public bool isGrounded;
            public bool inAction;
            public bool jumpTrigger;
            public bool isHanging;
            //public bool inCopter;
            public bool isEquipped;
            public bool toggleEquip;
        }
        #endregion

        #region Control Variables
        [Header("Locomotion Controls")]
        public float moveSpeed = 8.0f;                              //The motors base movement speed
        public float runMultiplier = 1.5f;                          //A multiplier applied to the motors base speed while running
        public float runStaminaCost = 3.0f;                         //The stamina cost for the motor to maintain the running state
        public float turnSpeed = 5.0f;                              //Turning speed of the motor
        public float moveAcceleration = 5.0f;                       //How quickly the motor accelerates while grounded

        [Header("Aerial Controls")]
        public float aerialMoveSpeed = 3.0f;                        //The maximum aerial speed of the motor
        public float aerialDamper = 0.3f;                           //How quickly the motor dampens to maximum aerial speed while airborne
        public float aerialTurnSpeed = 0.2f;                        //The turn speed of the motor while airborne

        public float jumpHeight = 2.0f;                             //The maximum height relative to the starting jump point the motor can reach
                                                                    //when jumping
        public float jumpDistanceMultiplier = 2.0f;                 //XZ-Force multiplier that is applied to the characters forward velocity when
                                                                    //jumping
        public float groundDistance = 0.2f;                         //Raycast distance from the grounding point to check if the motor is currently
                                                                    //grounded
        public float fallSpeedAdditive = 8.0f;                      //Extra force added to the player while falling, utilizied to prevent the motor
                                                                    //from having a percieved floating effect
        public LayerMask ground;                                    //The physics layers that the motor checks for when performing physics calculations

        //[Header("Copter Controls")]
        //public float copterUpdraftForce;                            //The initial force applied to the motor at the start of a copter manuever
        //public float copterPassiveForce;                            //The passive force applied to the motor during a copter manuever
        //public float copterInitialStaminaCost = 5.0f;               //The stamina cost to start a copter manuever
        //public float copterSustainStaminaCost = 3.0f;               //The stamina cost per second to maintain a copter manuever

        public Transform focalPoint;                                //The point on the motor that the camera focuses on for look at calculations

        [Header("Attack Controls")]
        public LayerMask attackLayers;

        [SerializeField]
        private CharacterAnimator charAnimator;                     //Reference to the child animator component of the motor

        [Header("Grounding Controls")]
        //public float groundClampForce;
        public float slopeLimit;                                    //Maximum angle that the motor can climb without slipping
        [SerializeField]
        private Transform groundCaster;                             //The transform that the raycast are performed from for calculating grounding
        #endregion

        #region Private Variables
        private Player player;                                      //Reference to the players stats script
        private Rigidbody rigidBody;                                //The rigidbody attached to the motor
        private new CapsuleCollider collider;                       //The collider of the motor
        private Vector3 jumpForce;                                  //The jump force applied to the motor when jumping, this is calculated at runtime based
                                                                    //upon the jump height relative to the initial gravity
        private bool isGrounded = true;                             
        private MotorInput currentInputs;                           //The current inputs of the motor calculated each frame
        private bool isRunning;
        //private bool inAction;

        private Vector3 groundNormal;                               //The normal of the surface the motor is positioned on top of (if any)
        private Vector3 slopeDirection;                             //The direction of the slope of the surface
        private Vector3 slideDirection;                             //The direction the motor will slide relative to the slope direction
        private bool isSliding;

        private bool holdingWeaponButton;
        private float weaponButtonTimer;

        private Vector3 snapLocation;                               //The position that the motor will snap toward for actions such as climbing
                                                                    //and grabbing ledges
        private Quaternion snapRotation;                            //The rotation the motor will snap towards for certain actions

        //private float copterPower;                                  //The internal value of the copter manuever, the manuever will stop when this reaches zero

        private List<Skill> skills;
        private Skill currentSkill;
        #endregion

        public MotorInput CurrentFrameInput {
            get { return currentInputs; }
        }

        public bool IsGrounded {
            get { return isGrounded; }
        }

        void Awake(){
            //Initialize all the default values of the motor
            rigidBody = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();
            player = GetComponent<Player>();

            //Calculate the jump force required for the motor to reach peak height relative to the force of gravity
            jumpForce = Vector3.up * Mathf.Sqrt(jumpHeight * -2.0f * Physics.gravity.y);

            isSliding = false;
        }

		// Use this for initialization
		void Start(){

            //Pass a reference of the motor to the animator and register both scripts with each other to allow for proper communication between scripts
            charAnimator.RegisterRoot(this);
        }

        public void RegisterSkill(Skill newSkill) {
            if(skills == null) {
                skills = new List<Skill>();
            }

            skills.Add(newSkill);
        }

		// Update is called once per frame
		void Update() {

            //Reset all non persistent inputs
            //Stuff that is not required to carry over between frames is discarded
            ProccessVitals();
            ResetInputs();

            //Read the in action state from the animator. This is done so the motor can know if the animator is currently perfomring any action animations
            //Utilizing this allows the motor to opperate without interrupting any vital animations
            currentInputs.inAction = charAnimator.Anim.GetBool(IN_ACTION_ID);

            //Reset the inputs each frame before calculating new inputs
            currentInputs.moveDir = Vector3.zero;

            //Read the current inputs of the horizontal and vertical axis
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            //Calculate some basic velocity inputs based upon the vertical and horizontal values
            if (Mathf.Abs(horizontal) > INPUT_DEAD_ZONE || Mathf.Abs(vertical) > INPUT_DEAD_ZONE) {
                currentInputs.moveDir = GetCameraRelative(horizontal, vertical, out currentInputs.currentDirection);
                currentInputs.inputMagnitude = currentInputs.moveDir.magnitude;
                if (isSliding) {
                    currentInputs.currentDirection = 0.0f;
                }
            } else {
                currentInputs.moveDir = Vector3.zero;
                currentInputs.currentDirection = 0.0f;
                currentInputs.inputMagnitude = 0.0f;
            }

            //Calculate a desired movement magnitude based upon inputs
            //States such as running and sliding are also taken into consideration in order to produce smooth transitions in movement between states
            currentInputs.currentMoveMagnitude = Mathf.Lerp(currentInputs.currentMoveMagnitude,
                (currentInputs.inputMagnitude > INPUT_DEAD_ZONE && !isSliding) ? (isRunning ? runMultiplier : 1.0f) * currentInputs.inputMagnitude : 0.0f,
                moveAcceleration * Time.deltaTime);

            if (currentInputs.inAction) {
                ActionBehaviour(horizontal, vertical);
            } else {
                NormalBehaviour(horizontal, vertical);
            }

            charAnimator.Tick(Time.deltaTime, currentInputs, rigidBody.velocity);
        }

        /// <summary>
        /// The behaviour of the motor that is utilized after inputs are calculated if the motor is currently not in an action state
        /// This calculated how the motors basic locomotion will function if not currently in actions and will capture inputs that transition
        /// the motor into an available action
        /// </summary>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        private void NormalBehaviour(float horizontal, float vertical) {

            currentSkill = null;

            if (isGrounded) {
                if (Input.GetButtonDown("X Button")) {
                    //currentInputs.toggleEquip = true;
                    holdingWeaponButton = true;
                    weaponButtonTimer = Time.time;
                }

                if (holdingWeaponButton) {
                    if(Input.GetButtonUp("X Button")){
                        holdingWeaponButton = false;
                        weaponButtonTimer = 0.0f;

                        //charAnimator.PushAnimAction("Attack Basic", 0.1f);
                        WeaponAnimator.WeaponAction attackAction;

                        attackAction.filterEvent = (Collider other) => {
                            return attackLayers == (attackLayers | (1 << other.gameObject.layer));
                        };

                        attackAction.weaponEvent = (Collider other, WeaponAnimator.WeaponStats stats) => {
                            Damagable dmgObj = other.GetComponent<Damagable>();

                            if(dmgObj != null && other.gameObject != this.gameObject) {
                                dmgObj.Damage(stats.damageBase);
                            }
                        };

                        charAnimator.Attack("Attack Basic", attackAction);
                        PushNewEquipState(true);
                    } else if (Time.time - weaponButtonTimer > 0.5f) {
                        holdingWeaponButton = false;
                        weaponButtonTimer = 0.0f;
                        currentInputs.toggleEquip = true;
                    }
                }

                if (isRunning == false && Input.GetButtonDown("B Button") && player.StaminaRatio() > 0.15f) {
                    isRunning = true;
                }

                if (Input.GetButtonDown("A Button") && isGrounded) {
                    currentInputs.jumpTrigger = true;
                }
            }

            foreach(Skill sk in skills) {
                if(sk.Evaluate(charAnimator, rigidBody)) {
                    currentSkill = sk;
                    break;
                }
            }

            //Calculate the rotation of the motor based upon the passed in inputs
            Quaternion targetRot = Quaternion.LookRotation(
                (isSliding ? slideDirection : (currentInputs.inputMagnitude > INPUT_DEAD_ZONE) ? currentInputs.moveDir : transform.forward),
                Vector3.up
            );

            float targetRotationSpeed = (isGrounded ? turnSpeed * Mathf.Abs(currentInputs.currentDirection * 0.015f) : aerialTurnSpeed) * (3.0f - currentInputs.currentMoveMagnitude);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRot,
                (isSliding ? 3.0f : targetRotationSpeed) * Time.deltaTime
            );            
        }

        /// <summary>
        /// The behaviour of the motor that is utilized after inputs are calculated if the motor is currently in an action state
        /// This calculats how the motor behaves while in an action and handled how these actions are maintained and transitioned
        /// out of based upon the calculated input
        /// </summary>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        private void ActionBehaviour(float horizontal, float vertical) {
            if (charAnimator.Anim.GetBool("Hanging")) {
                if (Input.GetButtonDown("A Button")) {
                    snapLocation = Vector3.zero;
                    charAnimator.Anim.SetTrigger("ClimbUp");
                    charAnimator.Anim.SetBool("Hanging", false);
                    currentInputs.currentMoveMagnitude = 0.0f;
                }

                if (Input.GetButtonDown("B Button")) {
                    snapLocation = Vector3.zero;
                    charAnimator.Anim.SetBool("Hanging", false);
                    //rigidBody.isKinematic = false;
                }
            }

            if(currentSkill != null) {
                currentSkill.InUse(charAnimator, rigidBody);
            }

            currentInputs.currentDirection = 0.0f;
        }

        public void FixedUpdate() {
            //TODO: Fix this cross state influence later
            rigidBody.isKinematic = charAnimator.Anim.GetBool("IsKinematic");
            float delta = Time.fixedDeltaTime;

            //Check the current state of grounding for the motor before applying further physics calculations
            Transform motorRoot = null;
            CheckGrounding(out motorRoot);

            if (!currentInputs.inAction) {
                transform.SetParent(null);
                transform.localScale = Vector3.one;
                transform.SetParent(motorRoot);

                if (isGrounded) {
                    if (!isSliding) {
                        Vector3 moveDelta = (transform.forward * currentInputs.currentMoveMagnitude * moveSpeed);
                        //Debug.Log(moveDelta);
                        float oldY = Mathf.Min(rigidBody.velocity.y, 0.0f);
                        Vector3 refactoredVelocity = new Vector3(0.0f, oldY, 0.0f);
                        rigidBody.velocity = refactoredVelocity;
                        rigidBody.AddForce(moveDelta, ForceMode.VelocityChange);
                    }
                } else {
                    //TODO: Expand upon aerial controls later
                    PhysicsFalling(delta);
                }
            } else {
                if (rigidBody.isKinematic && snapLocation != Vector3.zero) {
                    transform.position = Vector3.Slerp(transform.position, snapLocation, delta * 15.0f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, snapRotation, delta * 15.0f);
                } else if (currentSkill != null /*currentInputs.inCopter && copterPower > 0.0f*/) {
                    if (!IsGrounded) {
                        PhysicsFalling(delta);
                    }
                        
                    currentSkill.PhysicsEffects(rigidBody, delta);
                    //rigidBody.velocity += Vector3.up * delta * copterPassiveForce * Mathf.Clamp(copterPower, 0.0f, 2.0f);
                }
            }
        }

        /// <summary>
        /// The basic physics applied to the motor while in the falling state. This handles the basic movement of the motor while falling
        /// </summary>
        /// <param name="delta"></param>
        public void PhysicsFalling(float delta) {
            float oldY = rigidBody.velocity.y;
            if (oldY <= 0) {
                oldY -= delta * fallSpeedAdditive;
            }

            Vector3 currentVelocity = new Vector3(rigidBody.velocity.x, 0.0f, rigidBody.velocity.z);
            Vector3 targetVelocity = Vector3.Lerp(currentVelocity, currentInputs.moveDir * aerialMoveSpeed, delta * aerialDamper);
            targetVelocity.y = oldY;
            rigidBody.velocity = targetVelocity;
        }

        /// <summary>
        /// Checks the current vital state of the motor and determines if the motor is able to continue operating
        /// </summary>
        public void ProccessVitals() {
            if (isRunning) {
                if (player.UseStamina(runStaminaCost * Time.deltaTime) == false) {
                    isRunning = false;
                }
            }

            /*if(copterPower > 0.0f) {
                if(player.UseStamina(copterSustainStaminaCost * Time.deltaTime) == false) {
                    copterPower = 0.0f;
                    currentInputs.inCopter = false;
                }
            }*/
        }

        /// <summary>
        /// Resets the inputs of the motor
        /// </summary>
        public void ResetInputs() {
            currentInputs.jumpTrigger = false;
            currentInputs.toggleEquip = false;

            if (isRunning && Input.GetButtonUp("B Button")) {
                isRunning = false;
            }

            /*if(copterPower <= 0.0f) {
                currentInputs.inCopter = false;
            }*/
        }

        /// <summary>
        /// Checks the state of the motors current grounding state for the current physics iteration. Slope values and ground normals are
        /// also calculated that can be utilized by the motor for movement purposes. This function also passes back the transform of the
        /// object that the motor is directly on top of as an out parameter
        /// </summary>
        /// <param name="groundObject"></param>
        private void CheckGrounding(out Transform groundObject) {
            isGrounded = Physics.CheckSphere(groundCaster.position, collider.radius - 0.005f, ground, QueryTriggerInteraction.Ignore);
            if (isRunning && !isGrounded) {
                isRunning = false;
            }

            RaycastHit hit;

            Debug.DrawRay(groundCaster.position, Vector3.down * groundDistance, Color.red);

            if(Physics.Raycast(groundCaster.position, Vector3.down, out hit, groundDistance, ground, QueryTriggerInteraction.Ignore)) {
                groundNormal = hit.normal;
                groundObject = hit.transform;
            } else {
                groundNormal = Vector3.up;
                groundObject = null;
            }

            if (isGrounded && Vector3.Angle(Vector3.up, groundNormal) > slopeLimit) {
                Vector3 biTangent = Vector3.Cross(Vector3.up, groundNormal);
                slopeDirection = Vector3.Cross(biTangent, groundNormal).normalized;
                slideDirection = slopeDirection;
                slideDirection.y = 0.0f;
                slideDirection.Normalize();
                isSliding = true;
            } else {
                isSliding = false;
            }

            currentInputs.isGrounded = isGrounded;

            charAnimator.Anim.SetFloat("NormalAngle", Vector3.Angle(Vector3.up, groundNormal));

            //Debug rays for the slide and normal directions of a slope
            //Debug.DrawRay(transform.position, groundNormal, Color.green);
            //Debug.DrawRay(transform.position + Vector3.up, slideDirection, Color.cyan);
        }

        /// <summary>
        /// Converts the given input into local space of the motor relative to the camera. Passed out the direction as
        /// an angle between the desired direction and the current motors forwards vector.
        /// </summary>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 GetCameraRelative(float horizontal, float vertical, out float direction) {
            Vector3 rootDirection = transform.forward;
            Vector3 stickDirection = new Vector3(horizontal, 0.0f, vertical);

            //Cache this later to increase performance
            Vector3 cameraTemp = Camera.main.transform.forward;
            cameraTemp.y = 0.0f;
            Quaternion relativeRotation = Quaternion.FromToRotation(Vector3.forward, Vector3.Normalize(cameraTemp));

            Vector3 moveDirection = relativeRotation * stickDirection;
            Vector3 axisSign = Vector3.Cross(moveDirection, rootDirection);

            direction = Vector3.Angle(rootDirection, moveDirection) * (axisSign.y >= 0 ? -1.0f : 1.0f);

            return moveDirection;
        }

        /// <summary>
        /// The internal call of the motor used by the animator for root motion.
        /// </summary>
        /// <param name="deltaRot"></param>
        /// <param name="deltaPos"></param>
        public void ControllerMove(Quaternion deltaRot, Vector3 deltaPos) {
            if (rigidBody.isKinematic) {
                transform.position += deltaPos;
            } else {
                rigidBody.MovePosition(transform.position + deltaPos);
            }

            transform.rotation *= deltaRot;
        }

        /// <summary>
        /// Jump event called by the animator with the passed in forward speed value
        /// </summary>
        /// <param name="forwardSpeed"></param>
        public void Jump(float forwardSpeed) {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0.0f, rigidBody.velocity.z);
            rigidBody.AddForce(jumpForce + (transform.forward * forwardSpeed * jumpDistanceMultiplier), ForceMode.VelocityChange);
            //rigidBody.velocity = jumpDelta;
        }        

        /// <summary>
        /// Pushes an action animation to the animator and defines snapping points for the motor
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="snapLocation"></param>
        /// <param name="snapRotation"></param>
        public void PushAction(string actionName, Vector3 snapLocation, Quaternion snapRotation) {
            this.snapLocation = snapLocation;
            this.snapRotation = snapRotation;
            charAnimator.PushAnimAction(actionName);
        }

        /// <summary>
        /// Utilized for toggling the current equip state of the character
        /// </summary>
        /// <param name="equipState"></param>
        public void PushNewEquipState(bool equipState) {
            currentInputs.isEquipped = equipState;
        }

        /// <summary>
        /// Gizmos for use inside the editor
        /// </summary>
        public void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            if (collider != null) {
                Gizmos.DrawWireSphere(groundCaster.position, collider.radius);
            }
        }
    }
}
