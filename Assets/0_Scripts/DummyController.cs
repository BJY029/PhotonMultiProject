using System.Collections;
using UnityEngine;

enum DummyState
{
    Walk,
    Turn,
    Idle,
    Jump
};

public class DummyController : MonoBehaviour
{
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
	private float _targetRotation = 0.0f;
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

		ChangeState();
	}

	private void Update()
	{
		GroundCheck();
		PlayerGravity();
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


	private void ChangeState()
	{
		StopAllCoroutines();

		RandomTimer = Random.Range(3.0f, 10.0f);
		StartCoroutine(WaitAndChangeState());
	}

	IEnumerator WaitAndChangeState()
	{
		MyState = states[Random.Range(0, states.Length)];
		//MyState = DummyState.Walk;
		Debug.Log("Dummy State : " + MyState.ToString());

		switch(MyState)
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
				StartCoroutine(Turn());
				break;
			default:
				break;
		}

		while (RandomTimer > 0)
		{
			yield return new WaitForEndOfFrame();
			RandomTimer -= Time.deltaTime;
		}
	}


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

	IEnumerator JustIdle()
	{
		while(MyState == DummyState.Idle && RandomTimer > 0)
		{
			yield return new WaitForEndOfFrame();
		}
		ChangeState();
	}

	IEnumerator Move()
	{
		targetSpeed = MoveSpeed;
		while (MyState == DummyState.Walk && RandomTimer > 0)
		{
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;

			if(currentHorizontalSpeed < targetSpeed - speedOffset)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);

				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else 
			{
				_speed = targetSpeed;
			}


			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

			Vector3 moveDirection = transform.forward;

			_controller.Move(moveDirection * (_speed * Time.deltaTime)+ new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

			// 애니메이션 파라미터 업데이트
			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, 1);
			}

			yield return null;
		}
		targetSpeed = 0;
		StartCoroutine(StopMove());
	}

	IEnumerator StopMove()
	{
		float targetSpeed = 0;
		while(_speed > 0)
		{
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
			float speedOffset = 0.1f;

			if(currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);

				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			_animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
			if (_animationBlend < 0.01f) _animationBlend = 0f;

			Vector3 moveDirection = transform.forward;

			_controller.Move(moveDirection * (_speed * Time.deltaTime));

			// 애니메이션 파라미터 업데이트
			if (_hasAnimator)
			{
				_animator.SetFloat(_animIDSpeed, _animationBlend);
				_animator.SetFloat(_animIDMotionSpeed, 1);
			}

			yield return null;
		}

		ChangeState();
	}

	IEnumerator Jump()
	{
		_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
		if(_hasAnimator)
		{
			_animator.SetBool(_animIDJump, true);
		}

		while (RandomTimer > 0f)
		{
			// 중력 적용도 포함 (낙하)
			if (!Grounded && _verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}

			// 점프 중 캐릭터를 위로 이동
			Vector3 jumpMove = new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime;
			_controller.Move(jumpMove);

			yield return null;
		}

		ChangeState();
	}

	private void PlayerGravity()
	{
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
		else
		{
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

	IEnumerator Turn()
	{
		float turnTimer = _turnDuration;

		float targetYaw = Random.Range(0f, 360f);
		Quaternion startRotation = transform.rotation;
		Quaternion targetRotation = Quaternion.Euler(0f, targetYaw, 0f);

		float timer = 0f;
		while (timer < turnTimer)
		{
			timer += Time.deltaTime / turnTimer;
			transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timer);
			yield return null;
		}
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
