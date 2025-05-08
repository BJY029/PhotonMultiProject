using UnityEngine;
using Photon.Pun;
using StarterAssets;
using UnityEngine.InputSystem.XR;
public class FirstController : MonoBehaviourPun
{
    [Header("Movement")]//�̵� ���� 
    public float moveSpeed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 1.5f;
	public float SprintSpeed = 5.335f;

	[Header("Moese Look")]//���콺 ����
    public Transform cameraRoot; //���� ȸ���� ����ϴ� ī�޶� ���� ������Ʈ
    public Camera playerCamera; //�÷��̾��� 1��Ī ī�޶�
    public float mouseSensitivity = 100f; //���콺 ȸ�� ����
    public float minPitch = -90f; //�Ʒ��� �� �� �ִ� �ִ� ����
    public float maxPitch = 90f; //���� �� �� �ִ� �ִ� ����

    //CharacterController ���
    private CharacterController controller;
    private Vector3 velocity; //����(����) �ӵ�
    private float pitch = 0f; //x�� ȸ�� ��
    //���� ĳ���Ͱ� ���鿡 ����ִ��� Ȯ���ϴ� �ο�
    private bool isGrounded;


	public AudioClip LandingAudioClip;
	public AudioClip[] FootstepAudioClips;
	[Range(0, 1)] public float FootstepAudioVolume = 0.5f;

	//PhotonView ����
	PhotonView photonView;

	private void Start()
	{
		//�Ҵ�
		photonView = GetComponent<PhotonView>();
        //���� �� ĳ���Ͱ� �ƴ϶��
		if(!photonView.IsMine)
        {
            //�ش� ī�޶�� �ı��ϰ� ���� ����
            Destroy(playerCamera.gameObject);
			return;
		}

		//CharacterController �Ҵ�
		controller = GetComponent<CharacterController>();
        //���콺 Ŀ�� ���
        Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
        //���� �� ĳ���Ͱ� �ƴϸ� ���� ����
        if (!photonView.IsMine) return;

        GroundCheck(); //���� üũ
        Move(); //Ű���� �̵� �� ���� ó��
        Look(); //���콺�� �þ� ȸ��
	}

    void GroundCheck()
    {
        //Unity ���� isGrounded �÷��׸� ���
        isGrounded = controller.isGrounded;

        //���� �� ���� �ӵ� �ʱ�ȭ(���鿡 ������Ű��)
        //���� �� ���¿��� �Ʒ��� ���ϴ� �ӵ��� ���� ��� �ʱ�ȭ(ƨ�� ����)
        if(isGrounded && velocity.y < 0f)
        {
            //velocity�� �߷� ���ӵ��� ���� ĳ���Ͱ� ��� �Ʒ��� �������� �ӵ�
            //���⼭ velocity.y�� �ҷ��� ������ �����ؼ�
            //���鿡 ���������� ���� �� ���¸� �����ϰ� �Ѵ�.
            //0�� �ƴ� -2f�� �δ� ������ CharacterController�� GroundCheck �������� ��¦ ������� �ϱ� ����
            velocity.y = -2f;
        }
    }

    void Move()
    {
        //Ű���� �Է� ����
        float x = Input.GetAxis("Horizontal"); //A, D
        float z = Input.GetAxis("Vertical");  //W, S


        //���� ���� �������� ������ ���� ���� ���
        Vector3 move = transform.right * x + transform.forward * z;
        //������ ���� ���Ⱚ�� ���� ���� �̵� ����
        controller.Move(move * moveSpeed * Time.deltaTime);

        //���� ���鿡 �پ��ִ� ���¿��� ������ �ԷµǸ�
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //�߷��� �Ųٷ� Ǯ�� ���� �ӵ� ���(v = root(2gh))
            //�߷��� ���� ���̹Ƿ� -2f�� ���ϴ� ��
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        //�� �����Ӹ��� �߷� ���ӵ��� ���� ���� ���� �۾�����.
        //���� ������ �߻��� ��, ���� ���� �ӵ��� ���������� �����Ѵ�.
		velocity.y += gravity * Time.deltaTime;
        //�̷��� ���� ���� �ӵ� ���� ���� ��ġ�� ����
		controller.Move(velocity * Time.deltaTime);
	}

    void Look()
    {
        //���콺 �Է� ����
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //���Ʒ� ����: pitch(x��)
        pitch -= mouseY;
        //�þ� ���� ����(-90 ~ 90�� �þ� ����)
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        //ī�޶� ���� ������Ʈ�� ȸ��(���� ȸ���� ����(x�� ����))
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        //�÷��̾� ��ü�� �¿� ȸ��(y�� ����)
        transform.Rotate(Vector3.up * mouseX);
    }
}
