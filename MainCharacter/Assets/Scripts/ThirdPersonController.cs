using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class ThirdPersonController : MonoBehaviour
	{
        public SFSceneChanger sFScene;
        public GameObject wheelUi;
        public GameObject parent;
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 2.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 5.335f;
		[Tooltip("How fast the character turns to face movement direction")]
		[Range(0.0f, 0.3f)]
		public float RotationSmoothTime = 0.12f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.50f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.28f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 70.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -30.0f;
		[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
		public float CameraAngleOverride = 0.0f;
		[Tooltip("For locking the camera position on all axis")]
		public bool LockCameraPosition = false;


        public bool isMove = false;

        // cinemachine
        private float _cinemachineTargetYaw;
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _animationBlend;
		private float _targetRotation = 0.0f;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta, desiredY, timeToSendPListRequest=0;
		private float _fallTimeoutDelta;

		// animation IDs
		private int _animIDSpeed;
		private int _animIDGrounded;
		private int _animIDJump;
		private int _animIDFreeFall;
		private int _animIDMotionSpeed;

		private Animator _animator ;
        public Animator animc;
        private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

        private bool _hasAnimator,inAreaToSit=false, wantToSit=false, setDesiredY = false, canLoadEventScene=false;
        GameObject chair;
        GameState gameState = new GameState();

        private void Awake()
		{
            Cursor.lockState = CursorLockMode.Locked;
            sFScene = GameObject.FindGameObjectWithTag("networkManger").GetComponent<SFSceneChanger>();
            // get a reference to our main camera
            if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
            _animator = GetComponent<Animator>();
			_hasAnimator = TryGetComponent(out _animator);
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();

			AssignAnimationIDs();
			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}
        void setFalseToAllDanceStates()
        {
            _animator.SetBool("TwistDance", false);
            _animator.SetBool("RumbaDance", false);
            _animator.SetBool("HipHopDance", false);
        }

        void setFalseToAllDanceStatesExcept(string stateName)
        {
            List<string> allSatets = new List<string>() { "TwistDance", "RumbaDance", "HipHopDance" };
            for(int i=0;i<allSatets.Count;i++)
            {
                if (stateName == allSatets[i])
                    continue;
                _animator.SetBool(allSatets[i], false);
            }
        }
        void sit()
        {
            if (chair == null || setDesiredY)
                return;
            isOnRightlookToSit(chair.transform);
            
                if(isOnRightPositionToSit(chair.transform))
                {
                    if(!setDesiredY)
                    {
                        setDesiredY = true;
                        transform.Rotate(Vector3.up, 180);
                        _animator.SetBool("Sit", true);
                }
                }
            
        }

        List<string> boolStates = new List<string>() { "Jump", "TwistDance", "RumbaDance", "HipHopDance", "Sit" };
        List<string> floatStates = new List<string>() { "Speed", "MotionSpeed" };
        float timeToSendAnimRequest = 0;
        private void Update()
		{
			_hasAnimator = TryGetComponent(out _animator);
            
            if (canLoadEventScene&&Input.GetKeyDown(KeyCode.L))
            {
                sFScene.JoinLiveEventRoom();
                return;
            }
            if(Input.GetKeyDown(KeyCode.Tab))
            {
                wheelUi.active = true;
                gameState.PauseGame();
            }

            if (Input.GetKeyUp(KeyCode.Tab))
            {
                wheelUi.active = false;
                gameState.ResumeGame();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                wantToSit = false;
                setFalseToAllDanceStates();
                animc.SetBool("TwistDance", true);
                _animator.SetBool("TwistDance", true);
            }
            if (Input.GetKeyDown(KeyCode.X))
            {
                wantToSit = false;
                setFalseToAllDanceStates();
                _animator.SetBool("RumbaDance", true);
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                wantToSit = false;
                setFalseToAllDanceStates();
                _animator.SetBool("HipHopDance", true);
            }
            if (Input.GetKeyDown(KeyCode.V) && inAreaToSit && !wantToSit)
            {
                setFalseToAllDanceStates();
                wantToSit=true;
                setDesiredY = false;
                //_animator.SetBool("Sit", true);
            }
            JumpAndGravity();
			GroundedCheck();
            float lastX = (int)(transform.position.x * 10), lastY = (int)(transform.position.y * 10), lastZ = (int)(transform.position.z * 10);
            Move();
            if (wantToSit)
            {
                sit();
            }
            if (lastX / 10 != ((int)transform.position.x * 10) / 10 || lastY / 10 != ((int)transform.position.y * 10) / 10 || lastZ / 10 != ((int)transform.position.z * 10) / 10)
            {
                isMove = true;
            }
            List<KeyValuePair<string, bool>> b=new List<KeyValuePair<string, bool>>();
            List<KeyValuePair<string, float>> f = new List<KeyValuePair<string, float>>();
            for (int i = 0; i < boolStates.Count; i++)
            {
                b.Add(new KeyValuePair<string, bool>(boolStates[i],_animator.GetBool(boolStates[i])));
            }
            for (int i = 0; i < floatStates.Count; i++)
            {
                f.Add(new KeyValuePair<string, float>(floatStates[i], _animator.GetFloat(floatStates[i])));
            }

            if(timeToSendAnimRequest<=Time.time)
            {
                timeToSendAnimRequest = Time.time + 0.2f;
                NetworkManager.Instance.SendAnimationStates(b, f);
            }
        }

		private void LateUpdate()
		{
			CameraRotation();
		}

		private void AssignAnimationIDs()
		{
			_animIDSpeed = Animator.StringToHash("Speed");
			_animIDGrounded = Animator.StringToHash("Grounded");
			_animIDJump = Animator.StringToHash("Jump");
			_animIDFreeFall = Animator.StringToHash("FreeFall");
			_animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
		}

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
		}

		private void CameraRotation()
		{
			// if there is an input and camera position is not fixed
			if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
			{
				_cinemachineTargetYaw += _input.look.x * Time.deltaTime;
				_cinemachineTargetPitch += _input.look.y * Time.deltaTime;
			}

			// clamp our rotations so our values are limited 360 degrees
			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

			// Cinemachine will follow this target
			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
		}

		private void Move()
		{


                // set target speed based on move speed, sprint speed and if sprint is pressed
                float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}
			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
                try
                {
                    _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                }
                catch
                {
                    if (_mainCamera == null)
                        print("_mainCamera");
                    if (_targetRotation == null)
                        print("_targetRotation");

                }
				_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
				float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

				// rotate to face input direction relative to camera position
				transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
			}


			Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

			// move the player
			_controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            if (_animationBlend>=.6f)
            {
                wantToSit = false;
                _animator.SetBool("Sit", false);
                setFalseToAllDanceStates();
            }

            // update animator if using character
            if (_hasAnimator)
			{

				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
			}
		}


        bool isOnRightPositionToSit(Transform tr)
        {
            float dis = .5f;
            Vector3 offset = tr.position - transform.position;
            if (offset.sqrMagnitude <= dis*dis)
            {
                return true;

            }

            _animator.SetFloat(_animIDSpeed, 2f);
            _animator.SetFloat(_animIDMotionSpeed, 1f);
            offset = new Vector3(offset.x, transform.position.y, offset.z);
            offset = offset.normalized;
            _controller.Move(offset*Time.deltaTime);

            return false;
        }


        void isOnRightlookToSit(Transform tr)
        {
            int damping = 10;
            Vector3 lookPos = tr.position -transform.position;
            // lookPos.y = 0;
            //lookPos.x = 0;
            //lookPos.z= 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.tag=="chair")
            {
                inAreaToSit = true;
                chair = other.gameObject;
            }

            if (other.gameObject.tag == "goToEvent")
            {
                canLoadEventScene = true;
            }
            
        }

        void loadLiveEventScene()
        {
            SceneManager.LoadScene(SceneNames.LiveEventsScene);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "chair")
            {
                inAreaToSit = false;
                chair = null;
            }

            if (other.gameObject.tag == "goToEvent")
            {
                canLoadEventScene = false;
            }
        }

        private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// update animator if using character
				if (_hasAnimator)
				{
					_animator.SetBool(_animIDJump, false);
				}

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

					// update animator if using character
					if (_hasAnimator)
					{
						_animator.SetBool(_animIDJump, true);
					}
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}
        
		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;
			
			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}