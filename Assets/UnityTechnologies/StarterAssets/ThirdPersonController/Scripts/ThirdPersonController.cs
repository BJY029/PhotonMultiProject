using UnityEngine;
using Photon.Pun;   //Photon Networking용 네임 스페이스


#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem; //Input System을 사용하는 경우 사용
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    //CharacterController 필수 컴포넌트로 자동 추가
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM  //InputSystem 사용 시 필수 컴포넌트로 자동 추가
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviourPun
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f; //걷기 속도

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f; //달리기 속도

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f; //회전 부드러움

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f; //가속, 감속 속도

        //사운드 관련
        public AudioClip LandingAudioClip; //착지 시 사운드
        public AudioClip[] FootstepAudioClips; //발소리 사운드
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f; //사운드 소리 크기

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f; //점프 높이

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f; //중력 값

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f; //점프 후 재점프까지 대기 시간

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f; //낙하 판정까지 지연 시간(계단 내려갈 때 유용한 설정)

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true; //지면에 있는지 체크하는 플래그

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f; //지면 체크 오프셋(거친 표면에서 유용한 설정)

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f; //지면 체크 반지름, CharacterController 반지름과 동일해야 함

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers; //어떤 레이어를 지면으로 판단할 것인지

        //카메라 pivot(회전의 기준)
        public GameObject CameraRoot;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f; //위쪽 카메라 회전 제한

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;//아래쪽 카메라 회전 제한

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;//카메라 각도 보정

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false; //카메라 위치 고정 여부


        //=====================내부 상태 변수======================
        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs(애니메이션 해시 값)
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;

        public GameObject _mainCamera; //해당 플레이어가 사용할 카메라

        private const float _threshold = 0.01f; //미세 입력 무시 기준값

        private bool _hasAnimator;

        private PhotonView photonView; //멀티 플레이를 위해 PhotonView 선언

        //마우스 입력 확인
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            //PhotonView 선언
			photonView = GetComponent<PhotonView>();
		}

        private void Start()
        {
            //애니메이터 컴포넌트가 있으면 반환
            _hasAnimator = TryGetComponent(out _animator);
            //CharacterController 객체 생성
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif
            
            //각 애니메이션 해시 값 할당
            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

			//만약 내 view가 아닌 경우 해당 카메라는 꺼둔다.
			//즉 다른사람의 카메라를 꺼둔다.
			if (!photonView.IsMine)
			{
				_mainCamera.SetActive(false);
				return;
			}
            _mainCamera.SetActive(true);
		}

        private void Update()
        {
            //현재 이렇게 처리하니 상대방 애니메이션이 적용되지 않음
            //RPC를 활용해야 할 것 같음
            if (!photonView.IsMine) return;
            _hasAnimator = TryGetComponent(out _animator);


            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        //각 Animator 파라미터의 문자열 이름을 정수 해시값으로 변환하는 함수
        //Update() 같은 반복 실행에서 효율적인 비교를 하기 위해 정수해시값으로 변환해서 사용
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        //캐릭터가 지면에 닿았는지 체크해서 Grounded 변수를 업데이트하고, 애니메이터 파라미터도 갱신하는 함수
        private void GroundedCheck()
        {
            //지면 체크용 구체의 중심 좌표를 설정(현재 플레이어 기준, 지정한 오프셋만큼 아래에 위치)
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            
            //Physics.CheckSphere : 구체 충돌 체크 함수
                //spherePosition : 검사 중심 좌표 , GroundedRadius : 검사 범위(캐릭터 크기에 맞춰 설정)
                //GroundLayers : 어떤 레이어를 '지면'으로 판단할 지 필터링, QueryTriggerInteraction.Ignore : 트리커 콜라이더는 무시
            //즉, 발밑의 일정 범위 내에 GroundLayers에 해당하는 오브젝트가 존재하면 Grounded = true로 설정
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            //Animator가 존재하는 경우
            if (_hasAnimator)
            {
                //애니메이터에 Grounded 상태 전달(false면 공중 애니메이션 재생)
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        //카메라 회전을 담당하는 함수
        //마우스 혹은 조이스틱의 입력을 받아 카메라의 pitch와 캐릭터의 Yaw 방향 조정
        //CameraRoot를 로컬 회전 시키고, 플레이어의 전체 회전은 Y축 기준으로 조정
        private void CameraRotation()
        {
            //만약 입력벡터의 크기가 threshold 보다 작으면(즉 카메라를 거의 움직이지 않았을 경우)
            //혹은 LockCameraPosition이 true인 경우
            //카메라 회전을 하지 않는다.
            if (_input.look.sqrMagnitude < _threshold || LockCameraPosition)
                return;

            //마우스 입력인 경우, 절대 위치 입력이므로 보정이 필요없음 -> 1.0f
            //패드 등의 입력일 경우 프레임 보정이 필요함. -> Time.deltaTime
            float deltaTimeMultipler = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            //좌우 회전 값, 캐릭터 기준 Y축 회전으로 마우스 X축 또는 패드 좌우로 조정
            _cinemachineTargetYaw += _input.look.x * deltaTimeMultipler;
            //상하 회전 값, 카메라 기준 X축 회전, 마우스 Y축 혹은 패드 상하로 조정(마우스 상하 반전 효과로 음수)
            _cinemachineTargetPitch -= _input.look.y * deltaTimeMultipler;

            //상하 회전 각도를 위아래로 제한
            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

            //플레이어 오브젝트 자체를 Y축 회전시켜서 방향을 전환
            transform.rotation = Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f);
            //카메라 pivot(CameraRoot)의 X축 회전을 적용하여 상하 시야 회전
            CameraRoot.transform.localRotation = Quaternion.Euler(-_cinemachineTargetPitch, 0.0f, 0.0f);

            //즉 좌우 회전은 캐릭터 자체를 회전, 상하 회전은 카메라의 상하 회전을 통해 구현
        }


        //이동 관련 함수
        private void Move()
        {
            //달리기(_input.sprint)가 true이면 달리기 속도, 아니면 걷기 속도를 적용
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            //Vector2의 == 연산자 사용은 비교 근사 연산을 사용하기 때문에 소수점 오차 없이 빠른 계산이 가능하다. magnitude 보다 성능 좋음
            // if there is no input, set the target speed to 0
            // 입력이 없는 경우, 속도를 0으로 설정한다.
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            //플레이어 컨트롤러의 x,z 축의 벡터의 크기, 즉 플레이어의 수평 속도를 구한다.
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            //아날로그 입력일 경우, 입력 세기를 사용, 아니면 1로 고정
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            //가속 및 감속의 부드러운 처리
            //현재 수평 속도가 목표 속도와 차이가 많이 나는 경우, 
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                //Lerp을 사용하여 점진적으로 속도 변경
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round()로 속도를 0.001 단위로 정리해 불필요한 미세 진동 제거
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else //속도 차이가 많아 나지 않으면
            {
                //그냥 목표 속도록 속도 설정
                _speed = targetSpeed;
            }

            //_speed = targetSpeed;

            //애니메이션 블렌드 값 계산, 애니메이션 속도를 부드럽게 전환하기 위한 보간 값
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            //거의 멈춘 경우 0으로 정리하여 Idle 애니메이션 정상 전환 유도
            if (_animationBlend < 0.01f) _animationBlend = 0f;

			// 이동 방향: 카메라 기준 전/후/좌/우
			Vector3 camForward = _mainCamera.transform.forward;
			Vector3 camRight = _mainCamera.transform.right;

			// Y축 (수직)방향 제거 (수평 이동만 고려)
			camForward.y = 0f;
			camRight.y = 0f;

            // 각 이동 방향 정규화
			camForward.Normalize();
			camRight.Normalize();

			// 입력 방향 벡터 계산 (WASD)
			Vector3 moveDirection = camForward * _input.move.y + camRight * _input.move.x;
            // 입력 방향 정규화
			moveDirection.Normalize();

			// 실제 이동 적용(수평 및 점프(JumpAndGravity()) 구현)
			_controller.Move(moveDirection * (_speed * Time.deltaTime) +
							 new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);


            // 애니메이션 파라미터 업데이트
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }


        private void JumpAndGravity()
        {
            //플레이어가 지면에 존재하는 경우
            if (Grounded)
            {
                //낙하 판멸 타이머를 초기화한다.
                _fallTimeoutDelta = FallTimeout;

                // 공중에 있을 때 재생되는 애니메이션 비활성화
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // 지면에 붙어있는 경우, 계속 내려가게 두지 않고, 살짝 아래로 눌러주는 정도로만 고정
                //이는 지면에 자연스럽게 붙어 있게 한다.
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // 점프 키가 눌렸고, 점프 쿨타임이 끝난 경우
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    //점프 속도를 수직 속도 공식(root(2 * gravity * height))로 계산
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    //점프 애니메이션 활성화
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // 점프 쿨타임 계산을 위해 지속적으로 감소
                //해당 변수 값은 아래 else 문에서 초기화 되어서 지속적으로 계산 됨
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else //지면에 붙어있지 않은 경우, 즉 점프 중이면
            {
                // 점프 쿨타임을 초기화
                _jumpTimeoutDelta = JumpTimeout;

                // 낙하 판정 딜레이를 위한 처리
                // 약간의 점프 이후만 진짜 낙하로 처리하기 위해 해당 쿨타임 만큼은 애니메이션 처리 하지 않음
                // 이는 계단 내려올 때 자연스럽게 보이게 한다.
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                // 낙하 판정 딜레이가 모두 소요된 경우
                else
                {
                    // 낙하 애니메이션 재생
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // 점프 중이면 점프 입력 무시
                _input.jump = false;
            }

			// 수직 속도가 최대 낙하 속도(terminalVelocity)보다 느리면 중력을 적용해서 수직 속도 가속
			if (_verticalVelocity < _terminalVelocity)
            {
                //각 프레임에 독립적인 중력 가속을 구현
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        //private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        //{
        //    if (lfAngle < -360f) lfAngle += 360f;
        //    if (lfAngle > 360f) lfAngle -= 360f;
        //    return Mathf.Clamp(lfAngle, lfMin, lfMax);
        //}

        //디버깅용 함수로 Unity 에디터에서 객체가 선택되었을 때 디버깅용 시각적 힌트(Gizmo)를 표시하는데 사용
        //씬 뷰에서 객체를 선택하면 호출되는 함수
        private void OnDrawGizmosSelected()
        {
            //투명한 초록색, 빨간색 색상 지정
            //초록색 -> 지면에 닿은 경우
            //빨간색 -> 공중에 떠있음
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            //지면에 닿은 경우, Gizmos의 색상을 초록새으로 설정
            if (Grounded) Gizmos.color = transparentGreen;
            //지면에 닿지 않은 경우, 즉 공중에 있는 경우, Gizmos의 색상을 빨간색으로 설정
            else Gizmos.color = transparentRed;

            // Physics.CheckSphere()에서 사용한 지면 감지 위치와 동일한 위치, 크기로 구체를 그린다.
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }


        //Animation Event를 통해 자동 호출되는 함수
        //이벤트가 발생할 때 AnimationEvent 객체가 전달되어, 현재 애니메이션 상태의 가중치(weight) 등을 확인
        private void OnFootstep(AnimationEvent animationEvent)
        {
            //현재 애니메이션 클립이 Blend 상태에서 충분히 우선순위가 높을 때만 실행
            //weight가 1.0에 가까울수록 해당 클립이 확실히 재생되고 있다는 의미다.
            //따라서 0.5 이상이면 발소리를 재생한다.
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                //발소리 클립이 등록되어 있을 때만 실행된다.
                if (FootstepAudioClips.Length > 0)
                {
                    //발소리 클립이 랜덤 재생되도록 Random 함수 사용 해서 다음에 재생될 클립 설정
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    //현재 캐릭터 위치를 기준으로 사운드 재생
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

		//Animation Event를 통해 자동 호출되는 함수
		//이벤트가 발생할 때 AnimationEvent 객체가 전달되어, 현재 애니메이션 상태의 가중치(weight) 등을 확인
		private void OnLand(AnimationEvent animationEvent)
        {
			//현재 애니메이션 클립이 Blend 상태에서 충분히 우선순위가 높을 때만 실행
			//weight가 1.0에 가까울수록 해당 클립이 확실히 재생되고 있다는 의미다.
			//따라서 0.5 이상이면 발소리를 재생한다.
			if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                //착지음을 캐릭터 위치에서 재생
                AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }
}