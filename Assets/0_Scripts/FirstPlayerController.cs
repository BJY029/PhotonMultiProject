using UnityEngine;
using Photon.Pun;
using StarterAssets;
using UnityEngine.InputSystem.XR;
public class FirstController : MonoBehaviourPun
{
    [Header("Movement")]//이동 관련 
    public float moveSpeed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 1.5f;
	public float SprintSpeed = 5.335f;

	[Header("Moese Look")]//마우스 관련
    public Transform cameraRoot; //수직 회전을 담당하는 카메라 기준 오브젝트
    public Camera playerCamera; //플레이어의 1인칭 카메라
    public float mouseSensitivity = 100f; //마우스 회전 감도
    public float minPitch = -90f; //아래로 볼 수 있는 최대 각도
    public float maxPitch = 90f; //위로 볼 수 있는 최대 각도

    //CharacterController 사용
    private CharacterController controller;
    private Vector3 velocity; //수직(점프) 속도
    private float pitch = 0f; //x축 회전 값
    //현재 캐릭터가 지면에 닿아있는지 확인하는 부울
    private bool isGrounded;


	public AudioClip LandingAudioClip;
	public AudioClip[] FootstepAudioClips;
	[Range(0, 1)] public float FootstepAudioVolume = 0.5f;

	//PhotonView 선언
	PhotonView photonView;

	private void Start()
	{
		//할당
		photonView = GetComponent<PhotonView>();
        //만약 내 캐릭터가 아니라면
		if(!photonView.IsMine)
        {
            //해당 카메라는 파괴하고 실행 안함
            Destroy(playerCamera.gameObject);
			return;
		}

		//CharacterController 할당
		controller = GetComponent<CharacterController>();
        //마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
        //만약 내 캐릭터가 아니면 조작 안함
        if (!photonView.IsMine) return;

        GroundCheck(); //지면 체크
        Move(); //키보드 이동 및 점프 처리
        Look(); //마우스로 시야 회전
	}

    void GroundCheck()
    {
        //Unity 내장 isGrounded 플래그를 사용
        isGrounded = controller.isGrounded;

        //착지 시 수직 속도 초기화(지면에 안착시키기)
        //착지 한 상태에서 아래로 향하는 속도가 있을 경우 초기화(튕김 방지)
        if(isGrounded && velocity.y < 0f)
        {
            //velocity는 중력 가속도로 인해 캐릭터가 계속 아래로 떨어지던 속도
            //여기서 velocity.y를 소량의 음수로 고정해서
            //지면에 안정적으로 밀착 된 상태를 유지하게 한다.
            //0이 아닌 -2f로 두는 이유는 CharacterController의 GroundCheck 로직에서 살짝 눌러줘야 하기 때문
            velocity.y = -2f;
        }
    }

    void Move()
    {
        //키보드 입력 수집
        float x = Input.GetAxis("Horizontal"); //A, D
        float z = Input.GetAxis("Vertical");  //W, S


        //현재 방향 기준으로 움직임 방향 벡터 계산
        Vector3 move = transform.right * x + transform.forward * z;
        //위에서 구한 방향값을 통해 수평 이동 적용
        controller.Move(move * moveSpeed * Time.deltaTime);

        //만약 지면에 붙어있는 상태에서 점프가 입력되면
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //중력을 거꾸로 풀어 점프 속도 계산(v = root(2gh))
            //중력이 음수 값이므로 -2f를 곱하는 것
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        //매 프레임마다 중력 가속도에 의해 값이 점점 작아진다.
        //따라서 점프가 발생된 후, 점점 낙하 속도가 빨라지도록 설정한다.
		velocity.y += gravity * Time.deltaTime;
        //이렇게 계산된 수직 속도 값을 실제 위치에 적용
		controller.Move(velocity * Time.deltaTime);
	}

    void Look()
    {
        //마우스 입력 수집
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //위아래 보기: pitch(x축)
        pitch -= mouseY;
        //시야 각도 제한(-90 ~ 90로 시야 고정)
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        //카메라 기준 오브젝트의 회전(상하 회전만 적용(x축 기준))
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        //플레이어 전체의 좌우 회전(y축 기준)
        transform.Rotate(Vector3.up * mouseX);
    }
}
