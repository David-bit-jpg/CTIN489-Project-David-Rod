using System.Collections;
using Unity.Collections;
using UnityEngine;

namespace FPSController
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        #region Variables
        #region Private Serialized     
        #region Data
        [Space, Header("Data")]
        [SerializeField] public MovementInputData movementInputData = null;
        [SerializeField] public HeadBobData headBobData = null;

        #endregion

        #region Locomotion
        [Space, Header("Locomotion Settings")]
        [SerializeField] private float crouchSpeed = 1f;
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 3f;
        [SerializeField] public float jumpSpeed = 8f;
        [Range(0f, 1f)] [SerializeField] private float moveBackwardsSpeedPercent = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float moveSideSpeedPercent = 0.75f;
        #endregion

        #region Run Settings
        [Space, Header("Run Settings")]
        [Range(-1f, 1f)] [SerializeField] private float canRunThreshold = 0.8f;
        [SerializeField] private AnimationCurve runTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        #endregion

        #region Crouch Settings
        [Space, Header("Crouch Settings")]
        [Range(0.2f, 0.9f)] [SerializeField] private float crouchPercent = 0.6f;
        [SerializeField] private float crouchTransitionDuration = 1f;
        [SerializeField] private AnimationCurve crouchTransitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        #endregion

        #region Landing Settings
        [Space, Header("Landing Settings")]
        [Range(0.05f, 0.5f)] [SerializeField] private float lowLandAmount = 0.1f;
        [Range(0.2f, .9f)] [SerializeField] private float highLandAmount = 0.6f;
        [SerializeField] private float landTimer = 0.5f;
        [SerializeField] private float landDuration = 1f;
        [SerializeField] private AnimationCurve landCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        #endregion

        #region Extra Jump Settings
        [Space, Header("Extra Jump Settings")]
        [Range(0.1f, 1f)] [SerializeField] private float extraJumpStrength = .8f;
        [SerializeField] private int numberOfJumps = 1;
        private float defaultJumpSpeed;
        private int jumpCount;
        #endregion

        #region Gravity
        [Space, Header("Gravity Settings")]
        [SerializeField] public float gravityMultiplier = 2.5f;
        [SerializeField] public float stickToGroundForce = 5f;

        [SerializeField] private LayerMask groundLayer = ~0;
        [Range(0f, 1f)] [SerializeField] private float rayLength = 0.1f;
        [Range(0.01f, 1f)] [SerializeField] private float raySphereRadius = 0.1f;
        #endregion

        #region Wall Settings
        [Space, Header("Check Wall Settings")]
        [SerializeField] private LayerMask obstacleLayers = ~0;
        [Range(0f, 1f)] [SerializeField] private float rayObstacleLength = 0.1f;
        [Range(0.01f, 1f)] [SerializeField] private float rayObstacleSphereRadius = 0.1f;

        #endregion

        #region Smooth Settings
        [Space, Header("Smooth Settings")]
        [Range(1f, 100f)] [SerializeField] private float smoothRotateSpeed = 25f;
        [Range(1f, 100f)] [SerializeField] private float smoothInputSpeed = 10f;
        [Range(1f, 100f)] [SerializeField] private float smoothVelocitySpeed = 15f;
        [Range(1f, 100f)] [SerializeField] private float smoothFinalDirectionSpeed = 10f;
        [Range(1f, 100f)] [SerializeField] private float smoothHeadBobSpeed = 5f;
        private float defaultSmoothFinalDirectionSpeed;
        private float defaultSmoothRotateSpeed;
        [Space]

        #endregion
        #endregion
        #region Private Non-Serialized
        #region Components / Custom Classes / Caches
        private CharacterController m_characterController;
        private Transform m_yawTransform;
        private Transform m_camTransform;
        private HeadBob m_headBob;
        private CameraController m_cameraController;
        private WallClimb wallClimb;
        private Slide slide;
        private WallRun wallRun;

        private RaycastHit m_hitInfo;
        private IEnumerator m_CrouchRoutine;
        private IEnumerator m_LandRoutine;
        #endregion

        #region Grounded
        private RaycastHit groundDistanceHit;
        public GameObject groundCheck;
        public LayerMask ignoreLayer;
        [SerializeField] [ReadOnly] public float distanceToGround;
        [SerializeField] [ReadOnly] public bool m_isGrounded;
        [SerializeField] [ReadOnly] private bool m_previouslyGrounded;

        private float bounceFactor = 1;
        #endregion

        #region Debug
        [Space]
        [SerializeField] [ReadOnly] public Vector2 m_inputVector;
        [SerializeField] [ReadOnly] private Vector2 m_smoothInputVector;

        [Space]
        [SerializeField] [ReadOnly] public Vector3 m_finalMoveDir;
        [SerializeField] [ReadOnly] private Vector3 m_smoothFinalMoveDir;
        [Space]
        [SerializeField] [ReadOnly] public Vector3 m_finalMoveVector;

        [Space]
        [SerializeField] [ReadOnly] public float m_currentSpeed;
        [SerializeField] [ReadOnly] private float m_smoothCurrentSpeed;
        [SerializeField] [ReadOnly] private float m_finalSmoothCurrentSpeed;
        [SerializeField] [ReadOnly] private float m_walkRunSpeedDifference;

        [Space]
        [SerializeField] [ReadOnly] private float m_finalRayLength;
        [SerializeField] [ReadOnly] public bool m_hitWall;

        [Space]
        [SerializeField] [ReadOnly] private float m_InitialHeight;
        [SerializeField] [ReadOnly] private float m_crouchHeight;
        [SerializeField] [ReadOnly] private Vector3 m_InitialCenter;
        [SerializeField] [ReadOnly] private Vector3 m_crouchCenter;
        [Space]
        [SerializeField] [ReadOnly] private float m_InitialCamHeight;
        [SerializeField] [ReadOnly] private float m_crouchCamHeight;
        [SerializeField] [ReadOnly] private float m_crouchStandHeightDifference;
        [SerializeField] [ReadOnly] private bool m_duringCrouchAnimation;
        [SerializeField] [ReadOnly] private bool m_duringRunAnimation;
        [Space]
        [SerializeField] [ReadOnly] public float m_inAirTimer;
        #endregion
        #endregion

        #endregion

        #region BuiltIn Methods     
        protected virtual void Start()
        {
            wallClimb = GetComponent<WallClimb>();
            slide = GetComponent<Slide>();
            wallRun = GetComponent<WallRun>();

            defaultSmoothFinalDirectionSpeed = smoothFinalDirectionSpeed;
            defaultSmoothRotateSpeed = smoothRotateSpeed;
            defaultJumpSpeed = jumpSpeed;

            GetComponents();
            InitialVariables();

            StartCoroutine(TemporaryDisableCamera());
        }

        protected virtual void Update()
        {
            if (m_characterController)
            {
                CheckIfWall();
                CheckGroundDistance();

                if (!slide.jumpedFromSlide)
                {
                    SmoothSpeed();
                    SmoothDirection();
                }
                SmoothInput();

                if (slide.jumpedFromSlide)
                    smoothFinalDirectionSpeed = 3f;
                else
                    smoothFinalDirectionSpeed = defaultSmoothFinalDirectionSpeed;

                if (slide.isSliding)
                    smoothRotateSpeed = 1f;
                else
                    smoothRotateSpeed = defaultSmoothRotateSpeed;

                CalculateSpeed();
                CalculateFinalMovement();
                
                HandleHeadBob();
                HandleRunFOV();
                HandleCameraSway();

                if (!wallClimb.isMantling)
                {                    
                    CheckIfGrounded();
                    HandleLanding();
                    ApplyGravity();
                }

                if (!wallClimb.isClimbing && !m_cameraController.isLookingAround)
                {
                    RotateTowardsCamera();
                    if (!slide.isSliding)
                        CalculateMovementDirection();
                }
                
                ApplyMovement();

                if (slide.jumpedFromSlide)
                    bounceFactor = 1.8f;
                else
                    bounceFactor = 1;

                m_previouslyGrounded = m_isGrounded;
            }
        }

        //Crouch is in LateUpdate because of some weird behavior on certain builds. This makes it consistent.
        void LateUpdate()
        {
            HandleCrouch();
        }
        #endregion

        #region Custom Methods
        //For getting the initial values and components.  
        #region Initialize Methods  
        protected virtual void GetComponents()
        {
            m_characterController = GetComponent<CharacterController>();
            m_cameraController = GetComponentInChildren<CameraController>();
            m_yawTransform = m_cameraController.transform;
            m_camTransform = GetComponentInChildren<Camera>().transform;
            m_headBob = new HeadBob(headBobData, moveBackwardsSpeedPercent, moveSideSpeedPercent);
        }

        protected virtual void InitialVariables()
        {
            m_characterController.center = new Vector3(0f, m_characterController.height / 2f + m_characterController.skinWidth, 0f);

            m_InitialCenter = m_characterController.center;
            m_InitialHeight = m_characterController.height;

            m_crouchHeight = m_InitialHeight * crouchPercent;
            m_crouchCenter = (m_crouchHeight / 2f + m_characterController.skinWidth) * Vector3.up;

            m_crouchStandHeightDifference = m_InitialHeight - m_crouchHeight;

            m_InitialCamHeight = m_yawTransform.localPosition.y;
            m_crouchCamHeight = m_InitialCamHeight - m_crouchStandHeightDifference;

            m_finalRayLength = rayLength + m_characterController.center.y;

            m_isGrounded = true;
            m_previouslyGrounded = true;

            m_inAirTimer = 0f;
            m_headBob.CurrentStateHeight = m_InitialCamHeight;

            m_walkRunSpeedDifference = runSpeed - walkSpeed;
        }
        #endregion

        //Smoothing added so movement is not jagged and instantaneous. Higher smoothing values mean slower build to new values.
        #region Smoothing Methods
        protected virtual void SmoothInput()
        {
            m_inputVector = movementInputData.InputVector.normalized;
            m_smoothInputVector = Vector2.Lerp(m_smoothInputVector, m_inputVector, Time.deltaTime * smoothInputSpeed);
        }

        protected virtual void SmoothSpeed()
        {
            m_smoothCurrentSpeed = Mathf.Lerp(m_smoothCurrentSpeed, m_currentSpeed, Time.deltaTime * smoothVelocitySpeed);

            if (movementInputData.IsRunning && CanRun())
            {
                float _walkRunPercent = Mathf.InverseLerp(walkSpeed, runSpeed, m_smoothCurrentSpeed);
                m_finalSmoothCurrentSpeed = runTransitionCurve.Evaluate(_walkRunPercent) * m_walkRunSpeedDifference + walkSpeed;
            }
            else
                m_finalSmoothCurrentSpeed = m_smoothCurrentSpeed;
        }

        protected virtual void SmoothDirection()
        {
            m_smoothFinalMoveDir = Vector3.Lerp(m_smoothFinalMoveDir, m_finalMoveDir, Time.deltaTime * smoothFinalDirectionSpeed);
            Debug.DrawRay(transform.position, m_smoothFinalMoveDir, Color.yellow);
        }
        #endregion

        #region Locomotion Calculation Methods
        //Is the player grounded? Sends a spherecast downwards and checks hits on the 
        //ground layer to determine if the player is touching the ground.
        protected virtual void CheckIfGrounded()
        {
            Vector3 _origin = transform.position + m_characterController.center;

            bool _hitGround = Physics.SphereCast(_origin, raySphereRadius, Vector3.down, out m_hitInfo, m_finalRayLength, groundLayer);
            Debug.DrawRay(_origin, Vector3.down * (m_finalRayLength), Color.red);

            m_isGrounded = _hitGround ? true : false;
        }

        //Sends spherecast in the player move direction. Detects if there is a wall to stop the player run state and forward movement.
        protected virtual void CheckIfWall()
        {
            Vector3 _origin = transform.position + m_characterController.center;
            RaycastHit _wallInfo;

            bool _hitWall = false;

            if (movementInputData.HasInput && m_finalMoveDir.sqrMagnitude > 0)
                _hitWall = Physics.SphereCast(_origin, rayObstacleSphereRadius, m_finalMoveDir, out _wallInfo, rayObstacleLength, obstacleLayers);
            Debug.DrawRay(_origin, m_finalMoveDir * rayObstacleLength, Color.blue);

            m_hitWall = _hitWall ? true : false;
        }

        //Checks if there is anything above the player's head by sending a spherecast from the player upwards.
        protected virtual bool CheckIfRoof()
        {
            Vector3 _origin = transform.position;
            RaycastHit _roofInfo;

            bool _hitRoof = Physics.SphereCast(_origin, raySphereRadius, Vector3.up, out _roofInfo, m_InitialHeight);

            return _hitRoof;
        }

        //Check distance to ground by sending a raycast from the player feet downwards.
        void CheckGroundDistance()
        {
            if ((Physics.Raycast(groundCheck.transform.position, Vector3.down, out groundDistanceHit, 100f, ~ignoreLayer)))
            {
                distanceToGround = groundDistanceHit.distance;
            }
            else
                distanceToGround = 0;
        }

        //Checks if the player momentum is fast enough to activate run.
        //Takes into account the direction the player is moving as well as their current move transform.
        protected virtual bool CanRun()
        {
            Vector3 _normalizedDir = Vector3.zero;

            if (m_smoothFinalMoveDir != Vector3.zero)
                _normalizedDir = m_smoothFinalMoveDir.normalized;

            float _dot = Vector3.Dot(transform.forward, _normalizedDir);
            return _dot >= canRunThreshold && !movementInputData.IsCrouching ? true : false;
        }

        //Checks the player's move direction by taking into account the inputVectors and the move transform.
        protected virtual void CalculateMovementDirection()
        {
            Vector3 _vDir = transform.forward * m_smoothInputVector.y;
            Vector3 _hDir = transform.right * m_smoothInputVector.x;

            Vector3 _desiredDir = _vDir + _hDir;
            Vector3 _flattenDir = FlattenVectorOnSlopes(_desiredDir);

            m_finalMoveDir = _flattenDir;
        }

        protected virtual Vector3 FlattenVectorOnSlopes(Vector3 _vectorToFlat)
        {
            if (m_isGrounded)
                _vectorToFlat = Vector3.ProjectOnPlane(_vectorToFlat, m_hitInfo.normal);

            return _vectorToFlat;
        }

        //Determines player movespeed based on movement state and input.
        //Current speed is ultimately the player's desired speed so changing states need to change the currentSpeed value.
        protected virtual void CalculateSpeed()
        {
            m_currentSpeed = movementInputData.IsRunning && CanRun() ? runSpeed : walkSpeed;
            m_currentSpeed = movementInputData.IsCrouching ? crouchSpeed : m_currentSpeed;
            m_currentSpeed = !movementInputData.HasInput ? 0f : m_currentSpeed;
            m_currentSpeed = movementInputData.InputVector.y == -1 ? m_currentSpeed * moveBackwardsSpeedPercent : m_currentSpeed;
            m_currentSpeed = movementInputData.InputVector.x != 0 && movementInputData.InputVector.y == 0 ? m_currentSpeed * moveSideSpeedPercent : m_currentSpeed;
            m_currentSpeed = slide.isSliding ? crouchSpeed : m_currentSpeed;
        }

        //By getting the smooth values for speed, input, and direction, you can multiply them together to get the final vector for movement.
        protected virtual void CalculateFinalMovement()
        {
            float _smoothInputVectorMagnitude = 1f;
            Vector3 _finalVector = m_smoothFinalMoveDir * m_finalSmoothCurrentSpeed * _smoothInputVectorMagnitude;

            m_finalMoveVector.x = _finalVector.x;
            m_finalMoveVector.z = _finalVector.z;

            if (m_characterController.isGrounded)
                m_finalMoveVector.y += _finalVector.y;
        }
        #endregion

        #region Crouching Methods
        //Checks if player pressed crouch button and starts crouch routine.
        protected virtual void HandleCrouch()
        {
            if (movementInputData.CrouchClicked)
                InvokeCrouchRoutine();
        }

        public void InvokeCrouchRoutine()
        {
            if (movementInputData.IsCrouching)
                if (CheckIfRoof())
                    return;

            if (m_LandRoutine != null)
                StopCoroutine(m_LandRoutine);

            if (m_CrouchRoutine != null)
                StopCoroutine(m_CrouchRoutine);

            m_CrouchRoutine = CrouchRoutine();
            StartCoroutine(m_CrouchRoutine);
        }

        //Changes the camera position and speed when the crouch state changes. Starts coroutine based on current 
        //state and moves camera and speed value to new values based on crouch curves.
        public IEnumerator CrouchRoutine()
        {
            m_duringCrouchAnimation = true;
            movementInputData.IsRunning = false;

            float _percent = 0f;
            float _smoothPercent = 0f;
            float _speed = 1f / crouchTransitionDuration;

            float _currentHeight = m_characterController.height;
            Vector3 _currentCenter = m_characterController.center;

            float _desiredHeight = movementInputData.IsCrouching ? m_InitialHeight : m_crouchHeight;
            Vector3 _desiredCenter = movementInputData.IsCrouching ? m_InitialCenter : m_crouchCenter;

            Vector3 _camPosition = m_yawTransform.localPosition;
            float _camCurrentHeight = _camPosition.y;
            float _camDesiredHeight = movementInputData.IsCrouching ? m_InitialCamHeight : m_crouchCamHeight;
            
            movementInputData.IsCrouching = !movementInputData.IsCrouching;
            m_headBob.CurrentStateHeight = movementInputData.IsCrouching ? m_crouchCamHeight : m_InitialCamHeight;

            while (_percent < 1f)
            {
                _percent += Time.deltaTime * _speed;
                _smoothPercent = crouchTransitionCurve.Evaluate(_percent);

                m_characterController.height = Mathf.Lerp(_currentHeight, _desiredHeight, _smoothPercent);
                m_characterController.center = Vector3.Lerp(_currentCenter, _desiredCenter, _smoothPercent);

                _camPosition.y = Mathf.Lerp(_camCurrentHeight, _camDesiredHeight, _smoothPercent);
                m_yawTransform.localPosition = _camPosition;

                yield return null;
            }

            m_duringCrouchAnimation = false;
        }
        #endregion        

        #region Landing Methods
        //These methods determine player land speed and the impact to be placed on the camera.
        //Takes into account falling speed and inAirTimer to determine the impact on the camera when hitting the ground.
        protected virtual void HandleLanding()
        {
            if (!m_previouslyGrounded && m_isGrounded)
                InvokeLandingRoutine();
        }

        protected virtual void InvokeLandingRoutine()
        {
            if (m_LandRoutine != null)
                StopCoroutine(m_LandRoutine);

            m_LandRoutine = LandingRoutine();
            StartCoroutine(m_LandRoutine);
        }

        protected virtual IEnumerator LandingRoutine()
        {
            float _percent = 0f;
            float _landAmount = 0f;

            float _speed = 1f / landDuration;

            Vector3 _localPosition = m_yawTransform.localPosition;
            float _InitialLandHeight = _localPosition.y;

            _landAmount = m_inAirTimer > landTimer ? highLandAmount : lowLandAmount;

            while (_percent < 1f)
            {
                _percent += Time.deltaTime * _speed;
                float _desiredY = landCurve.Evaluate(_percent) * _landAmount;

                _localPosition.y = _InitialLandHeight + _desiredY;
                m_yawTransform.localPosition = _localPosition;

                yield return null;
            }
        }
        #endregion

        #region Locomotion Apply Methods
        protected virtual void HandleHeadBob()
        {
            //If moving in some way
            if (movementInputData.HasInput && m_isGrounded && !m_hitWall || wallClimb.isClimbing || wallClimb.isMantling
                || wallRun.isWallRunning)
            {
                if (!m_duringCrouchAnimation) //Head bob if moving and not during crouch routine.
                {
                    m_headBob.ScrollHeadBob(movementInputData.IsRunning && CanRun(), movementInputData.IsCrouching, movementInputData.InputVector);
                    m_yawTransform.localPosition = Vector3.Lerp(m_yawTransform.localPosition, (Vector3.up * m_headBob.CurrentStateHeight) + m_headBob.FinalOffset, Time.deltaTime * smoothHeadBobSpeed);
                }
            }
            else //If we are not moving.
            {
                if (!m_headBob.Resetted)
                    m_headBob.ResetHeadBob();

                if (!m_duringCrouchAnimation) //Reset head bob only if standing still and not during crouch routine.
                    m_yawTransform.localPosition = Vector3.Lerp(m_yawTransform.localPosition, new Vector3(0f, m_headBob.CurrentStateHeight, 0f), Time.deltaTime * smoothHeadBobSpeed);
            }
        }

        protected virtual void HandleCameraSway()
        {
            m_cameraController.HandleSway(m_smoothInputVector, movementInputData.InputVector.x);
        }

        protected virtual void HandleRunFOV()
        {
            if (movementInputData.HasInput && m_isGrounded && !m_hitWall)
            {
                if (movementInputData.RunClicked && CanRun())
                {
                    m_duringRunAnimation = true;
                    m_cameraController.ChangeRunFOV(false);
                }

                if (movementInputData.IsRunning && CanRun() && !m_duringRunAnimation)
                {
                    m_duringRunAnimation = true;
                    m_cameraController.ChangeRunFOV(false);
                }
            }

            if (movementInputData.RunReleased || !movementInputData.HasInput || m_hitWall)
            {
                if (m_duringRunAnimation)
                {
                    m_duringRunAnimation = false;
                    m_cameraController.ChangeRunFOV(true);
                }
            }
        }

        
        protected virtual void HandleJump()
        {
            //If jump was pressed, removes grounded restrictions and automatically sends player move vector upwards.
            if (movementInputData.JumpClicked)
            {
                jumpCount++;
                m_finalMoveVector.y = jumpSpeed;
                m_previouslyGrounded = true;
                m_isGrounded = false;
            }
        }
        protected virtual void ApplyGravity()
        {
            //CharacterController grounded condition works better than this grounded condition.
            if (m_characterController.isGrounded) 
            {
                jumpCount = 0;
                m_inAirTimer = 0f;
                m_finalMoveVector.y = -stickToGroundForce;

                HandleJump();
            }
            else //If the player is airborne, inAirTimer counts for landing purposes and gravity is applied.
            {
                if (m_inAirTimer <= .1f)
                {
                    HandleJump();
                }
                m_inAirTimer += Time.deltaTime;
                m_finalMoveVector += (Physics.gravity * gravityMultiplier * Time.deltaTime) / bounceFactor;
            }

            if (jumpCount < numberOfJumps && !m_characterController.isGrounded)
                HandleJump();
                
            //Changes jump power for additional jumps.
            if (jumpCount > 0)
                jumpSpeed = defaultJumpSpeed * extraJumpStrength;
            else
                jumpSpeed = defaultJumpSpeed;
        }

        protected virtual void ApplyMovement()
        {
            //Moves the player based on the move vector established above.
            m_characterController.Move(m_finalMoveVector * Time.deltaTime);
        }

        protected virtual void RotateTowardsCamera()
        {
            Quaternion _currentRot = transform.rotation;
            Quaternion _desiredRot = m_yawTransform.rotation;

            transform.rotation = Quaternion.Slerp(_currentRot, _desiredRot, Time.deltaTime * smoothRotateSpeed);
        }

        //Runs on Start only.
        public IEnumerator TemporaryDisableCamera()
        {
            m_cameraController.enabled = false;
            yield return new WaitForSeconds(.5f);
            m_cameraController.enabled = true;
        }
        #endregion
        #endregion
    }
}
