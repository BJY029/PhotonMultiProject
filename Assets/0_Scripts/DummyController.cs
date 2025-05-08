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
	//���µ��� ����Ʈ�� ����
	DummyState[] states = { DummyState.Walk, DummyState.Turn, DummyState.Idle, DummyState.Jump };
	//���� ���� ����
	DummyState MyState;

	//���� �÷��̾ � �ൿ�� �ϴµ��� ���� ���� �ֱ�
    private float RandomTimer;

	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 2.0f; //�ȱ� �ӵ�
	private float targetSpeed;

	[Tooltip("How fast the character turns to face movement direction")]
	[Range(0.0f, 0.3f)]
	public float RotationSmoothTime = 0.12f; //ȸ�� �ε巯��

	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f; //����, ���� �ӵ�

	//���� ����
	public AudioClip LandingAudioClip; //���� �� ����
	public AudioClip[] FootstepAudioClips; //�߼Ҹ� ����
	[Range(0, 1)] public float FootstepAudioVolume = 0.5f; //���� �Ҹ� ũ��

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f; //���� ����

	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = -15.0f; //�߷� ��

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.50f; //���� �� ���������� ��� �ð�

	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f; //���� �������� ���� �ð�(��� ������ �� ������ ����)

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool Grounded = true; //���鿡 �ִ��� üũ�ϴ� �÷���

	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f; //���� üũ ������(��ģ ǥ�鿡�� ������ ����)

	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float GroundedRadius = 0.28f; //���� üũ ������, CharacterController �������� �����ؾ� ��

	[Tooltip("What layers the character uses as ground")]
	public LayerMask GroundLayers; //� ���̾ �������� �Ǵ��� ������

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

	// animation IDs(�ִϸ��̼� �ؽ� ��)
	private int _animIDSpeed;
	private int _animIDGrounded;
	private int _animIDJump;
	private int _animIDFreeFall;
	private int _animIDMotionSpeed;

	private Animator _animator;

	private const float _threshold = 0.01f; //�̼� �Է� ���� ���ذ�

	private bool _hasAnimator;

	private CharacterController _controller;

	private void Start()
	{
		//�ִϸ����� ������Ʈ�� ������ ��ȯ
		_hasAnimator = TryGetComponent(out _animator);

		_controller = GetComponent<CharacterController>();

		//�� �ִϸ��̼� �ؽ� �� �Ҵ�
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

	//�� Animator �Ķ������ ���ڿ� �̸��� ���� �ؽð����� ��ȯ�ϴ� �Լ�
	//Update() ���� �ݺ� ���࿡�� ȿ������ �񱳸� �ϱ� ���� �����ؽð����� ��ȯ�ؼ� ���
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
		//���� üũ�� ��ü�� �߽� ��ǥ�� ����(���� �÷��̾� ����, ������ �����¸�ŭ �Ʒ��� ��ġ)
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
			transform.position.z);

		//Physics.CheckSphere : ��ü �浹 üũ �Լ�
		//spherePosition : �˻� �߽� ��ǥ , GroundedRadius : �˻� ����(ĳ���� ũ�⿡ ���� ����)
		//GroundLayers : � ���̾ '����'���� �Ǵ��� �� ���͸�, QueryTriggerInteraction.Ignore : Ʈ��Ŀ �ݶ��̴��� ����
		//��, �߹��� ���� ���� ���� GroundLayers�� �ش��ϴ� ������Ʈ�� �����ϸ� Grounded = true�� ����
		Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
			QueryTriggerInteraction.Ignore);

		//Animator�� �����ϴ� ���
		if (_hasAnimator)
		{
			//�ִϸ����Ϳ� Grounded ���� ����(false�� ���� �ִϸ��̼� ���)
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

			// �ִϸ��̼� �Ķ���� ������Ʈ
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

			// �ִϸ��̼� �Ķ���� ������Ʈ
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
			// �߷� ���뵵 ���� (����)
			if (!Grounded && _verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}

			// ���� �� ĳ���͸� ���� �̵�
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

			// ���߿� ���� �� ����Ǵ� �ִϸ��̼� ��Ȱ��ȭ
			if (_hasAnimator)
			{
				_animator.SetBool(_animIDJump, false);
				_animator.SetBool(_animIDFreeFall, false);
			}

			// ���鿡 �پ��ִ� ���, ��� �������� ���� �ʰ�, ��¦ �Ʒ��� �����ִ� �����θ� ����
			//�̴� ���鿡 �ڿ������� �پ� �ְ� �Ѵ�.
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

			// ���� ���� �ִϸ��̼�
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


	//Animation Event�� ���� �ڵ� ȣ��Ǵ� �Լ�
	//�̺�Ʈ�� �߻��� �� AnimationEvent ��ü�� ���޵Ǿ�, ���� �ִϸ��̼� ������ ����ġ(weight) ���� Ȯ��
	private void OnFootstep(AnimationEvent animationEvent)
	{
		//���� �ִϸ��̼� Ŭ���� Blend ���¿��� ����� �켱������ ���� ���� ����
		//weight�� 1.0�� �������� �ش� Ŭ���� Ȯ���� ����ǰ� �ִٴ� �ǹ̴�.
		//���� 0.5 �̻��̸� �߼Ҹ��� ����Ѵ�.
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			//�߼Ҹ� Ŭ���� ��ϵǾ� ���� ���� ����ȴ�.
			if (FootstepAudioClips.Length > 0)
			{
				//�߼Ҹ� Ŭ���� ���� ����ǵ��� Random �Լ� ��� �ؼ� ������ ����� Ŭ�� ����
				var index = Random.Range(0, FootstepAudioClips.Length);
				//���� ĳ���� ��ġ�� �������� ���� ���
				AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
			}
		}
	}

	//Animation Event�� ���� �ڵ� ȣ��Ǵ� �Լ�
	//�̺�Ʈ�� �߻��� �� AnimationEvent ��ü�� ���޵Ǿ�, ���� �ִϸ��̼� ������ ����ġ(weight) ���� Ȯ��
	private void OnLand(AnimationEvent animationEvent)
	{
		//���� �ִϸ��̼� Ŭ���� Blend ���¿��� ����� �켱������ ���� ���� ����
		//weight�� 1.0�� �������� �ش� Ŭ���� Ȯ���� ����ǰ� �ִٴ� �ǹ̴�.
		//���� 0.5 �̻��̸� �߼Ҹ��� ����Ѵ�.
		if (animationEvent.animatorClipInfo.weight > 0.5f)
		{
			//�������� ĳ���� ��ġ���� ���
			AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);

		}
	}
}
