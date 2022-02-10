using System;
using System.Collections.Generic;
using Galaxy;
using JetBrains.Annotations;
using Unity.LEGO.Minifig;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;

namespace Player
{
    /// <summary>
    /// A "galaxyfied" MinifigController.
    /// Inspired by the original MinifigController from LEGO Microgame.
    /// </summary>
    public class GalaxyMinifigController : MonoBehaviour
    {
        protected bool inputEnabled = true;
        protected bool airborne;
        protected int jumpsLeft;
        protected float speed;
        protected float rotateSpeed;
        protected bool stopSpecial;
        protected bool cancelSpecial;
        protected bool exploded;
        protected bool stepped;

        protected Minifig minifig;
        protected Rigidbody rb;
        protected Animator animator;
        [SerializeField] protected ParticleSystem particles;
        [SerializeField] public AudioSource audioSource;
        [SerializeField] protected Camera camera;

        public float Speed => speed;

        public float defaultGravityValue = 10; // Default gravity value when not in GalaxyPhysics object
        private GalaxyControl gc; // Galaxy Manager
        private float galaxyChangeVelocityMultiplier = 0.25f; // Change galaxy rotation to match x% of new one
        [CanBeNull] private GalaxyPhysics lastGalaxy; // Last GalaxyPhysics object before current

        private bool newInput; // Notify movement change in FixedUpdate from Update
        private Vector2 movementInput = Vector2.zero; // Input (Keyboard/Gamepad)
        private float forwardSpeed = 25; // Max speed forwards/backwards
        private float forwardInput = 0; // Reads converted forward/backwards input
        private float lastForwardInput = 0; // Last forward/backwards input before current
        private float inputFactor = 5.0f; // Velocity start value
        private float inputFactorTmp = 0; // Velocity modifier initialised with inputFactor
        private float inputFactorChange = 2.0f; // Linear velocity change applied on inputFactorTmp

        private float rotationDelta; // Reads converted left/right input
        private float rotationChangeVelocityThreshold = 0.2f; // When rotationDelta is above this threshold modify it
        private float rotationChangeVelocityMultiplier = 0.8f; // Velocity change applied on threshold trigger

        [SerializeField, Range(0.1f, 10)] public float jumpPowerMultiplier = 1.0f; // Modifies jump power
        private bool jumped; // Reads jump action input (Keyboard/Gamepad)
        private int jumps = 0;
        [SerializeField, Range(0, 10)] protected int maxJumpsInAir = 1; // Max jumps after initial one

        public List<AudioClip> stepAudioClips = new List<AudioClip>();
        public AudioClip jumpAudioClip;
        public AudioClip doubleJumpAudioClip;
        public AudioClip landAudioClip;
        public AudioClip explodeAudioClip;
        Action<bool> onSpecialComplete;

        private Collider collider; // Player collider
        public LayerMask collisionLayer; // Check collision events only on this layers

        // This attributes control the step movement
        public float stepOffset = 1.2f; // Max step size (seen from the bottom of the player)
        public float stepChange = 1.45f; // Up movement on detected step
        public float stepAngleUpCorrection = 0.15f; // Move angle Raycast start point up
        public float stepAngleForwardCorrection = 0.750f; // Move angle Raycast start point forward

        [SerializeField, Range(0.1f, 1.0f)] public float stepAngle = 0.50f; // Step movement angle 

        // Two rays are cast on the step check, bottom one should hit step, top one should hit nothing
        // When both hit it means the player stands before a wall
        private const float stepBottomRayDistance = 1.35f; // Ray length onto the step
        private const float stepTopRayDistance = 1.6f; // Ray length above the step
        private const float stepAngleRayDistance = 1.7f; // Ray length angled to the step
        public LayerMask stepCheckLayer; // Check steps only on this layers

        // This attributes control the ground check
        public float groundCheckOffset = 0.45f; // Ray length down
        public float groundCheckTopOffset = 0.3f; // Moves ray position a little bit up, useful on spheres
        public LayerMask groundCheckLayer; // Check ground only on this layers

        // Animation attributes
        protected static readonly int speedHash = Animator.StringToHash("Speed");
        protected static readonly int rotateSpeedHash = Animator.StringToHash("Rotate Speed");
        protected static readonly int groundedHash = Animator.StringToHash("Grounded");
        protected static readonly int jumpHash = Animator.StringToHash("Jump");
        protected static readonly int playSpecialHash = Animator.StringToHash("Play Special");
        protected static readonly int cancelSpecialHash = Animator.StringToHash("Cancel Special");
        protected static readonly int specialIdHash = Animator.StringToHash("Special Id");


        private PlayerConfiguration playerConfig;
        public int PlayerIndex => playerConfig.PlayerIndex;

        private PlayerControlls controls;

        protected virtual void Awake()
        {
            minifig = GetComponent<Minifig>();
            rb = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            collider = GetComponent<Collider>();
            // Setup Galaxy Manager
            gc = new GalaxyControl(collider);
            // Linear velocity modifier
            inputFactorTmp = inputFactor;
            // Camera
            // Parent camera to player
            // Why don`t do it in the hierarchy?
            // LEGO Minifigs are kind of weird and do not show the whole hierarchy...
            camera.transform.parent = transform;
            // Initialise animation.
            animator.SetBool(groundedHash, true);
            // Prevents Rigibody jitter
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            // Initializer controls
            controls = new PlayerControlls();
            // Setup particle system
            particles.transform.position = transform.position;
            particles.transform.parent = transform;
            particles.transform.localPosition = new Vector3(0, 2.5f, 0);
            particles.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            particles.transform.localRotation = Quaternion.Euler(180, 0, 0);
            particles.Stop(true);
            // var particlesEmission = particles.emission;
            // particlesEmission.enabled = true;
        }



        // =====
        // Input
        // =====
        public void initializeMyPlayer(PlayerConfiguration pc)
        {
            Debug.Log("Inside GalaxyMinifigController2 MyPlayer of " + pc.PlayerIndex);
            playerConfig = pc;
            playerConfig.Input.onActionTriggered += Input_onActionTriggered;
        }

        private void Input_onActionTriggered(InputAction.CallbackContext obj)
        {
            Debug.Log("Do an action");
            if (obj.action.name == controls.Player.Movement.name)
            {
                OnMove(obj);
            }
            if (obj.action.name == controls.Player.Jump.name)
            {
                OnJump(obj);
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            movementInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            // Context triggers multiple times
            // Jump only on performed event
            if (!context.performed) return;
            jumped = context.action.triggered;
        }

        protected virtual void Update()
        {
            // Get input and save it
            if (!inputEnabled) return;
            // Gamepad adjustments
            var inputY = movementInput.y;
            var inputX = movementInput.x;
            if (Math.Abs(inputY) < 0.3) inputY = 0;
            if (Math.Abs(inputX) < 0.2) inputX = 0;
            if (inputY < 0) inputX *= -1;
            // Forwards/Backwards
            forwardInput += inputY * 250 * Time.deltaTime;
            // Left/Right
            rotationDelta += inputX * 250 * Time.deltaTime;
            // Used for animation
            speed = forwardInput * 10;
            // Notify movement change
            newInput = true;
            // Check jump button & conditions
            if (!airborne) jumpsLeft = maxJumpsInAir + 1;
            if (!jumped) return;
            airborne = true;
            jumps++;
            jumped = false;
        }

        protected void FixedUpdate()
        {
            // Apply input from Update onto the player

            // Variables used throughout FixedUpdate 
            var minifigTransform = transform;
            var galaxyRotation = Quaternion.FromToRotation(minifigTransform.up, Vector3.up);
            var gravity = defaultGravityValue;
            GalaxyPhysics galaxy = null;
            var galaxyJumpModifier = 1.0f;

            // ==============            
            // Galaxy physics
            if (gc.Galaxy)
            {
                // Apply physics from current GalaxyPhysics object
                galaxy = gc.Galaxy;
                var galaxyTransform = galaxy.transform;

                var gravityUp = galaxy.type == GalaxyType.Fixed
                    ? galaxyTransform.up
                    : (minifigTransform.position - galaxyTransform.position).normalized;

                var rotation = Quaternion.FromToRotation(
                    minifigTransform.up, gravityUp
                );
                galaxyRotation = rotation;
                gravity = galaxy.gravity;
                galaxyJumpModifier = galaxy.jumpModifier;

                // Rotate Rigibody
                rb.rotation = Quaternion.Slerp(rb.rotation, rotation * rb.rotation, 35 * Time.fixedDeltaTime);
                // Apply gravity
                rb.AddForce(-gravityUp * galaxy.gravity * rb.mass);
            }
            else
            {
                // Keep player upright
                var rotation = Quaternion.FromToRotation(transform.up, Vector3.up);
                rb.rotation = Quaternion.Slerp(rb.rotation, rotation * rb.rotation, 35 * Time.fixedDeltaTime);
                // Apply gravity
                rb.AddForce(Vector3.down * defaultGravityValue * rb.mass);
            }

            // ================================
            // Velocity change on galaxy change
            // Change velocity rotation to match x% of new one in comparision to old one
            if (galaxy != lastGalaxy)
            {
                var lastUp = lastGalaxy != null ? lastGalaxy.transform.up : Vector3.up;
                var currentUp = galaxy != null ? galaxy.transform.up : Vector3.up;
                rb.velocity =
                    Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(lastUp, currentUp),
                        galaxyChangeVelocityMultiplier) *
                    rb.velocity;
                lastGalaxy = galaxy;
            }

            // ========
            // Movement
            // If no new input was given interpolate using last one
            if (!newInput) forwardInput = lastForwardInput;
            if (forwardInput >= 0.1 || forwardInput <= -0.1)
            {
                var localVelocity = transform.InverseTransformDirection(rb.velocity);
                // On reversed following input (forwards -> backwards, backwards -> forwards)
                // Set forward velocity to 0
                if (Math.Sign(lastForwardInput) != Math.Sign(forwardInput))
                {
                    localVelocity.z = 0;
                    // Reset velocity factor
                    inputFactorTmp = inputFactor;
                }
                // Max velocity forward (forwardSpeed) not reached yet? Increase it!
                else if (Math.Sign(forwardInput) * localVelocity.z < forwardSpeed)
                {
                    // Linear increase in velocity
                    inputFactorTmp += inputFactorChange;
                    // Increase velocity
                    localVelocity.z -= localVelocity.z -
                                       Math.Sign(forwardInput) * forwardSpeed * inputFactorTmp *
                                       Time.fixedDeltaTime;
                    // Clamp velocity when too much is given
                    if (Math.Sign(localVelocity.z) * localVelocity.z > forwardSpeed)
                    {
                        localVelocity.z = Math.Sign(localVelocity.z) * forwardSpeed;
                    }
                }

                var worldVelocity = transform.TransformDirection(localVelocity);
                rb.velocity = worldVelocity;
            }
            // No? Decrease velocity
            else
            {
                // Reset velocity factor
                inputFactorTmp = inputFactor;
                var localVelocity = transform.InverseTransformDirection(rb.velocity);
                if (localVelocity.z >= -0.3 || localVelocity.z <= 0.3)
                {
                    // Slowdown
                    localVelocity.z -= Math.Sign(localVelocity.z) * forwardSpeed * Time.fixedDeltaTime;
                    var worldVelocity = transform.TransformDirection(localVelocity);
                    rb.velocity = worldVelocity;
                }
            }

            // Reset input
            lastForwardInput = forwardInput;
            forwardInput = 0;
            newInput = false;


            // ========
            // Rotation
            var deltaQuaternion = Quaternion.Euler(0, rotationDelta, 0);
            // Reduce velocity when rotating
            if (rotationDelta > rotationChangeVelocityThreshold || rotationDelta < -rotationChangeVelocityThreshold)
            {
                var localVelocity = transform.InverseTransformDirection(rb.velocity);
                localVelocity.x *= rotationChangeVelocityMultiplier;
                localVelocity.z *= rotationChangeVelocityMultiplier;
                var worldVelocity = transform.TransformDirection(localVelocity);
                rb.velocity = worldVelocity;
            }

            rb.rotation *= deltaQuaternion;
            rotationDelta = 0;

            // ==========
            // Jump Logic
            // Only jump when in the air, input was given and given jumps are left
            if (!airborne && jumps > 0)
            {
                if (!IsGrounded()) airborne = true;
            }
            //Debug.Log("Jumped Fix: " + airborne +"  "+ jumps + " " + jumpsLeft);
            while (airborne && jumps > 0 && jumpsLeft > 0)
            {
                //Debug.Log("Jump Fix Enter");
                jumps--;
                jumpsLeft--;
                if (jumpsLeft == maxJumpsInAir)
                {
                    if (jumpAudioClip)
                    {
                        audioSource.PlayOneShot(jumpAudioClip);
                    }
                }
                else if (doubleJumpAudioClip)
                {
                    audioSource.PlayOneShot(doubleJumpAudioClip);
                }
                // Play particle system and schedule stop
                CancelInvoke(nameof(StopBoost));
                StartBoost();
                Invoke(nameof(StopBoost), 0.25f);
                animator.SetTrigger(jumpHash);
                // Jump is modified by many things:
                // current gravity, rigidbody mass, given jump modifier and jump modifier from galaxy (when entered)
                rb.AddForce(minifigTransform.up * gravity * rb.mass * jumpPowerMultiplier * galaxyJumpModifier,
                    ForceMode.Impulse);
            }

            jumps = 0;

            // ================================
            // Allow Rigibody to walk on stairs
            // See: https://forum.unity.com/threads/how-to-get-a-rigidbody-player-to-walk-on-any-type-of-stairs.133573/

            //Debug.DrawRay(minifigTransform.position, galaxyRotation * transform.forward, Color.red, 15f);
            // Debug.DrawRay(minifigTransform.position + minifigTransform.up * stepOffset,
            //     galaxyRotation * minifigTransform.forward, Color.red,
            //     15f);

            // Perform Raycast at bottom and little bit above
            // When bottom hits but above does not we have stair in front of us
            // It is time for a power lift!

            // Debug.DrawRay(rb.position + transform.up * stepAngleUpCorrection+ transform.forward * stepAngleForwardCorrection,
            //     galaxyRotation *
            //     Quaternion.Slerp(
            //         Quaternion.identity,
            //         Quaternion.FromToRotation(transform.forward, transform.up), stepAngle) *
            //     transform.forward * stepAngleRayDistance, Color.red, 15f);

            var rayCastBottom = Physics.Raycast(rb.position, galaxyRotation * transform.forward, stepBottomRayDistance,
                stepCheckLayer);
            var rayCastTop = Physics.Raycast(rb.position + minifigTransform.up * stepOffset,
                galaxyRotation * minifigTransform.forward, out var hit,
                stepTopRayDistance, stepCheckLayer);
            var angleRay = new Ray(
                rb.position + transform.up * stepAngleUpCorrection + transform.forward * stepAngleForwardCorrection,
                galaxyRotation * Quaternion.Slerp(
                    Quaternion.identity,
                    Quaternion.FromToRotation(transform.forward, transform.up), stepAngle) * transform.forward
            );
            var rayCastAngle = Physics.Raycast(angleRay, out hit, stepAngleRayDistance, stepCheckLayer);
            var rayCastAngleReverse = false;
            if (!rayCastAngle)
            {
                angleRay.origin = angleRay.GetPoint(stepAngleRayDistance);
                angleRay.direction = -angleRay.direction;
                rayCastAngleReverse = Physics.Raycast(angleRay, out hit, stepAngleRayDistance, stepCheckLayer);
            }

            // Debug.Log("=======");
            // Debug.Log(rayCastBottom);
            // Debug.Log(rayCastTop);
            // Debug.Log(rayCastAngle);
            // Debug.Log(rayCastAngleReverse);
            // if (hit.collider)
            //     Debug.Log(hit.collider.name);
            if (rayCastBottom && !rayCastTop && (rayCastAngle || rayCastAngleReverse))
            {
                // When stair height is lower than half of the given offset only boost up by 50%
                if (Physics.Raycast(rb.position + minifigTransform.up * stepOffset / 2,
                    galaxyRotation * minifigTransform.forward, stepBottomRayDistance, stepCheckLayer))
                {
                    Debug.Log("Half Power Stair Lift");
                    rb.MovePosition(rb.position + minifigTransform.up * stepChange / 2);
                }
                // Higher stair means full boost up
                else
                {
                    Debug.Log("Full Power Stair Lift");
                    //Debug.DrawRay(transform.position - new Vector3(0, .5f, 0), transform.TransformDirection(new Vector3(0, -1, 1).normalized), Color.red);
                    //Debug.DrawRay(transform.position, transform.forward, Color.red, 60f);
                    rb.MovePosition(rb.position + minifigTransform.up * stepChange);
                }
            }

            // ===============
            // Animation setup
            // Stop special if requested.
            cancelSpecial |= stopSpecial;
            stopSpecial = false;
            // Update animation - delay airborne animation slightly to avoid flailing arms when falling a short distance.
            animator.SetBool(cancelSpecialHash, cancelSpecial);
            animator.SetFloat(speedHash, speed);
            animator.SetFloat(rotateSpeedHash, rotateSpeed);
            animator.SetBool(groundedHash, !airborne);
        }

        // =============
        // Modify Galaxy
        // =============

        public void AddGalaxyPhysics(Galaxy.GalaxyPhysics physics) => gc.AddPhysics(physics);

        public void RemoveGalaxyPhysics(Galaxy.GalaxyPhysics physics) => gc.RemovePhysics(physics);

        // ========================
        // Collision & Ground Check
        // ========================
        private bool IsCollisionLayer(int layer)
        {
            // See: http://answers.unity.com/answers/1137700/view.html
            return collisionLayer == (collisionLayer | (1 << layer));
        }

        private bool IsGrounded()
        {
            // See: http://answers.unity.com/answers/196395/view.html
            // Set ray a little bit above transport position
            // with transform.up * groundCheckTopOffset so that round colliders are properly registered
            //Debug.DrawRay(transform.position + transform.up * groundCheckTopOffset, -transform.up * groundCheckOffset,
            //    Color.green, 30f);
            /*var res = Physics.Raycast(rb.position + transform.up * groundCheckTopOffset, -transform.up,
                out var hit, groundCheckOffset, groundCheckLayer);
            if (hit.collider)
            {
                Debug.Log("Grounded with: "+ hit.collider.gameObject.name);
            }
            return res;*/
            return Physics.Raycast(transform.position + transform.up * groundCheckTopOffset, -transform.up,
                groundCheckOffset, groundCheckLayer);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!IsCollisionLayer(other.gameObject.layer)) return;
            //Debug.Log("Collision with " + other.gameObject.name);
            if (IsGrounded())
            {
                //Debug.Log("Properly Grounded");
                airborne = false;
            }
        }

        private void OnCollisionStay(Collision other)
        {
            if (!IsCollisionLayer(other.gameObject.layer)) return;
            if (airborne & IsGrounded())
            {
                //Debug.Log("Properly Grounded");
                airborne = false;
            }
        }

        private void OnCollisionExit(Collision other)
        {
            if (!IsCollisionLayer(other.gameObject.layer)) return;
            if (!airborne && !IsGrounded())
            {
                //Debug.Log("Player Fell off...");
                airborne = true;
                jumpsLeft--;
            }

            // Set all velocity components not related to hump (x & z) to 0
            // See: http://answers.unity.com/answers/193406/view.html
            var localVelocity = transform.InverseTransformDirection(rb.velocity);
            localVelocity.x = 0;
            localVelocity.z = 0;
            rb.velocity = transform.TransformDirection(localVelocity);
            rb.angularVelocity *= 0.5f;
        }

        // ==========
        // Animations
        // ==========

        // Animation event.
        public void StepFoot()
        {
            if (!stepped)
            {
                if (stepAudioClips.Count > 0)
                {
                    var stepAudioClip = stepAudioClips[UnityEngine.Random.Range(0, stepAudioClips.Count)];
                    if (stepAudioClip)
                    {
                        audioSource.PlayOneShot(stepAudioClip);
                    }
                }
            }

            stepped = true;
        }

        // Animation event.
        public void LiftFoot()
        {
            stepped = false;
        }

        // public void PlaySpecialAnimation(MinifigController.SpecialAnimation animation,
        //     AudioClip specialAudioClip = null,
        //     Action<bool> onSpecialComplete = null)
        // {
        //     animator.SetBool(playSpecialHash, true);
        //     animator.SetInteger(specialIdHash, (int)animation);

        //     if (specialAudioClip)
        //     {
        //         audioSource.PlayOneShot(specialAudioClip);
        //     }

        //     this.onSpecialComplete = onSpecialComplete;
        // }

        // Particle System
        // ===============
        private void StartBoost()
        {
            particles.Play(true);
        }

        private void StopBoost()
        {
            particles.Stop(true);
        }

        // =====
        // Other
        // =====

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }

        private string GetTag(Transform current)
        {
            while (true)
            {
                if (!current.CompareTag(null) && !current.CompareTag("Untagged"))
                {
                    return transform.tag;
                }

                if (!transform.parent) return string.Empty;
                current = transform.parent;
                continue;

                return string.Empty;
                break;
            }
        }

        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
        {
            //Get a direction from the pivot to the point
            Vector3 dir = point - pivot;
            //Rotate vector around pivot
            dir = rotation * dir;
            //Calc the rotated vector
            point = dir + pivot;
            //Return calculated vector
            return point;
        }
    }
}