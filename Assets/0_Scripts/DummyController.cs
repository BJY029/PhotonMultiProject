using Photon.Pun;
using System.Collections;
using UnityEngine;

enum DummyState
{
    Walk,
    Turn,
    Idle,
    Jump
};

public class DummyController : MonoBehaviourPun
{
	public int DummyID => photonView.ViewID;
	public static DummyController instance;

	private void Awake()
	{
		if (instance == null)
			instance = this;
	}

	//상태들을 리스트로 저장
	DummyState[] states = { DummyState.Walk, DummyState.Turn, DummyState.Idle, DummyState.Jump };
	//현재 상태 정의
	DummyState MyState;

	//더미 플레이어가 어떤 행동을 하는데에 대한 랜덤 주기
    private float RandomTimer;

	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 2.0f; //걷기 속도
	private float targetSpeed;

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

	// player
	private float _speed;
	private float _animationBlend;
	private float _rotationVelocity;
	private float _verticalVelocity;
	private float _terminalVelocity = 53.0f;
	private float _turnDuration = 1.0f;

	// timeout deltatime
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;

	// animation IDs(애니메이션 해시 값)
	private int _animIDSpeed;
	private int _animIDGrounded;
	private int _animIDJump;
	private int _animIDFreeFall;
	private int _animIDMotionSpeed;

	private Animator _animator;

	private const float _threshold = 0.01f; //미세 입력 무시 기준값

	private bool _hasAnimator;

	private CharacterController _controller;

	public float maxDistance = 10f;

	private void Start()
	{
		//애니메이터 컴포넌트가 있으면 반환
		_hasAnimator = TryGetComponent(out _animator);

		_controller = GetComponent<CharacterController>();

		//각 애니메이션 해시 값 할당
		AssignAnimationIDs();

		// reset our timeouts on start
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;

		MyState = DummyState.Idle;

		
		//일정 시간 후 행동들을 실행할 수 있도록 코루틴 사용
		StartCoroutine(WaitAndChangeStates());
		
	}


	IEnumerator WaitAndChangeStates()
	{
		//전반적인 모션 스피드 설정으로 1로 해놔서 애니메이션 모션 속도를 고정시킨다.
		_animator.SetFloat(_animIDMotionSpeed, 1);
		float timer = 2.0f ;
		while (timer >= 0)
		{
			yield return null;
			timer -= Time.deltaTime;
		}

		//MasterClient만 코루틴 실행
		if (PhotonNetwork.IsMasterClient)
		{
			//Debug.Log("나는 MasterClient 입니다.");
			ChangeState();
		}		
	}

	private void Update()
	{
		//지속적으로 플레이어가 땅에 붙어있는지 확인
		GroundCheck();
		//지속적으로 중력 적용
		PlayerGravity();

		//Debug.DrawRay(transform.position, transform.forward * 10f, Color.red);

	}

	[PunRPC]
	public void RPC_SyncState(int viewID, int CurrentState, float CurrentTimer,
		float currentSpeed, float currentAnimBlend)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			GameObject dummyObj = PhotonView.Find(viewID)?.gameObject;
			MyState = (DummyState)CurrentState;
			RandomTimer = CurrentTimer;
			_speed = currentSpeed;
			_animationBlend = currentAnimBlend;
		}
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


	//Dummy의 상태를 변경시키는 함수
	public void ChangeState()
	{
		//상태 변경까지의 주기를 랜덤하게 설정
		RandomTimer = Random.Range(3.0f, 8.0f);
		//실행하던 모든 코루틴 중지
		StopAllCoroutines();
		//Dummy 상태를 변경하는 코루틴 호출
		StartCoroutine(WaitAndChangeState());
	}

	IEnumerator WaitAndChangeState()
	{
		bool blocked = false;
		Vector3 origin = transform.position;
		Vector3 direction = transform.forward;

		if (Physics.SphereCast(origin, 0.2f,direction, out RaycastHit hit, maxDistance))
		{
			//Debug.Log("Distance from wall : " + hit.distance);
			if (hit.distance < 1f)
			{
				blocked = true;
				//Debug.Log("Blocked!");
				MyState = DummyState.Turn;
			}
			else
			{
				//Debug.Log("Can go front");
				MyState = ReturnRandomState();
			}
		}
		else
		{
			//Debug.Log("No collider");
			MyState = ReturnRandomState();
		}
		//현재 내 상태를 디버깅 창에 표시
		//Debug.Log("Dummy State : " + MyState.ToString());
		//상태에 따라서 각 case문 실행
		switch (MyState)
		{
			case DummyState.Idle:
				StartCoroutine(JustIdle());
				break;
			case DummyState.Walk:
				StartCoroutine(Move());
				break;
			case DummyState.Jump:
				StartCoroutine(Jump());
				break;
			case DummyState.Turn:
				if (blocked)
				{
					StartCoroutine(Turn(90f, 270f));
					blocked = false;
				}
				else
					StartCoroutine(Turn(-360f, 360f));
				break;
			default:
				break;
		}

		//상태 변경 후, Timer 시작
		//해당 타이머 값은 각 행동 코루틴에서 while문의 조건으로 사용 됨
		while (RandomTimer > 0)
		{
			yield return null;
			RandomTimer -= Time.deltaTime;
		}
	}

	private DummyState ReturnRandomState()
	{
		//내 상태들을 랜덤값을 부여해서 정의
		MyState = states[Random.Range(0, states.Length)];

		return MyState;
	}

	//바닥에 붙어있는지 확인
	private void GroundCheck()
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

	//그냥 IDEL 애니메이션 재생하는 코루틴
	IEnumerator JustIdle()
	{
		//설정된 타이머동안
		while(MyState == DummyState.Idle && RandomTimer > 0)
		{
			//대기(Idle 애니메이션 재생)
			yield return new WaitForEndOfFrame();
		}
		//상태 변경
		ChangeState();
	}

	//이동 코루틴
	IEnumerator Move()
	{
		//목표 속도를 설정
		targetSpeed = MoveSpeed;
		//설정된 타이머동안 반복 실행
		while (MyState == DummyState.Walk && RandomTimer > 0)
		{
			//수평 속도 정의
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
			//일정 이하 속도가 줄면 그냥 멈추기 위한 오프셋 설정
			float speedOffset = 0.1f;
			//현재 수평 속도가 목표 속도와 차이가 많이 나면
			if(currentHorizontalSpeed < targetSpeed - speedOffset)
			{
				//Lerp을 사용해서 점진적으로 속도 변경
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
				//round()로 속도를 0.001 단위로 정리해서 불필요한 미세 진동 제거
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else //속도 차이가 많이 안나면
			{
				//그냥 목표 속도로 속도 설정
				_speed = targetSpeed;
			}

			//애니메이션 블렌드 값 계산, 애니메이션 속도를 부드럽게 전환하기 위한 보간 값
			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

			//전진 방향으로 설정
			Vector3 moveDirection = transform.forward;

			//controller를 사용해서 Dummy 이동시킴, 수직 운동도 수행함(어디 걸어서 내려갈 때 필요)
			_controller.Move(moveDirection * (_speed * Time.deltaTime)+ new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// 애니메이션 파라미터 업데이트
			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, 1);
			}

			yield return null;
		}
		//타이머가 다 된 경우
		targetSpeed = 0;
		//자연스럽게 멈추기 위한 코루틴 재생
		StartCoroutine(StopMove());
	}

	IEnumerator StopMove()
	{
		//목표 속도 0으로 설정
		float targetSpeed = 0;
		//속도가 0이 될 때 까지
		while(_speed > 0.01f)
		{
			//현재 수평 속도 계산
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
			float speedOffset = 0.1f;
			//현재 속도와 목표 속도가 많이 차이 나면
			if(currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				//Lerp을 사용하여 점진적으로 속도 변경
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
				// round()로 속도를 0.001 단위로 정리해 불필요한 미세 진동 제거
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				//속도 차이가 많이 안나면 그냥 0으로 설정
				_speed = targetSpeed;
			}
			//애니메이션 블렌드 값 계산, 애니메이션 속도를 부드럽게 전환하기 위한 보간 값
			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
			if (_animationBlend < 0.01f) _animationBlend = 0f;

			//동일하게 전진 방향으로 설정
			Vector3 moveDirection = transform.forward;
			//전진 이동
			_controller.Move(moveDirection * (_speed * Time.deltaTime));

			// 애니메이션 파라미터 업데이트
			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, 1);
			}

			yield return null;
		}
		
		//완전히 멈추면 속도를 0으로 초기화 하고
		_speed = 0;
		//애니메이션 파라미터도 0으로 넘겨서 오류 방지
		if (_hasAnimator)
		{
			_animator.SetFloat(_animIDSpeed, 0);
		}

		//상태 변경
		ChangeState();
	}

	//점프 코루틴
	IEnumerator Jump()
	{
		//점프 상태가 되면 수직 속도 계산
		_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

		//점프 애니메이션 활성화
		if(_hasAnimator)
		{
			_animator.SetBool(_animIDJump, true);
		}

		//타이머가 다 될때까지
		while (RandomTimer > 0f)
		{
			// 점프 중 캐릭터를 위로 이동
			//_verticalVelocity는 PlayerGravity() 함수에서 자동적으로 중력 적용으로 인해 감소하게 된다.
			//즉, 자연스러운 점프 연출 가능
			Vector3 jumpMove = new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime;
			_controller.Move(jumpMove);

			yield return null;
		}

		//상태 변경
		ChangeState();
	}

	//플레이어 중력 적용 함수
	private void PlayerGravity()
	{
		//플레이어가 땅바닥에 붙어있는 경우
		if (Grounded)
		{
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
		}
		else //점프 중이면
		{
			//중력 속도 적용
			if(_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}

			// 공중 상태 애니메이션
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDFreeFall, true);
			}
		}
	}

	//회전 코루틴
	IEnumerator Turn(float minRange, float maxRange)
	{
		//회전 타이머 초기화
		float turnTimer = _turnDuration;

		//0~360도 사이 중 랜덤 회전값 부여(수정해야 할 듯)
		float targetYaw = Random.Range(minRange, maxRange);
		//시작 회전 값(현재 내 상태)
		Quaternion startRotation = transform.rotation;
		//목표 회전 값
		Quaternion targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + targetYaw, 0f);

		//회전 타이머만큼 Slerp으로 회전 처리
		float timer = 0f;
		while (timer < turnTimer)
		{
			timer += Time.deltaTime / turnTimer;
			transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timer);
			yield return null;
		}

		while (RandomTimer > 0f) yield return null;	
		ChangeState();
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
			//해당 Dummy가 플레이어 근처이 있는 경우(약 20) 소리 재생
			if (PlayerSensor.Instance.IsDummyInRange(gameObject))
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
			//해당 Dummy가 플레이어 근처이 있는 경우(약 20) 소리 재생
			if (PlayerSensor.Instance.IsDummyInRange(gameObject))
			{
				//착지음을 캐릭터 위치에서 재생
				AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
			}
		}
	}
}
