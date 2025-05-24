using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using WebSocketSharp;

public class LobbyUIManager : MonoBehaviour
{
	//�̱���ȭ
	public static LobbyUIManager Instance;
	//LobbyManager�� �̱������� ������ �ʴ´�.
	public LobbyManager LobbyManager;
	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	public GameObject CreateRoomFrame;
	public GameObject JoinRoomFrame;
	public GameObject PassWordFrame;

	public GameObject Warning_Create;
	public GameObject Warning_MaxPlayer;
	public GameObject Warning_NullPass;

	public InputField RoomName;
	public InputField Password;

	private RoomInfo CurrentRoomInfo;

	private void Start()
	{
		//�̱����� ���������� ����Ǳ� ���ؼ� active �� ���·� ������ ���۵ǹǷ�
		//���ص��� �ʰ� ���� localScale�� 0���� ������ ����
		//���� localScale�� �ٽ� 1�� �������ش�.
		CreateRoomFrame.transform.localScale = Vector3.one;
		JoinRoomFrame.transform.localScale = Vector3.one;
		PassWordFrame.transform.localScale= Vector3.one;
		//�׸��� ��Ȱ��ȭ�Ѵ�.
		CreateRoomFrame.gameObject.SetActive(false);
		JoinRoomFrame.gameObject.SetActive(false);
		PassWordFrame.gameObject.SetActive(false);
	}

	//Create ��ư�� �������, ���� �Լ��� ����ȴ�.
	public void OnClickedCreateRoom()
	{
		if (!CreateRoomFrame.activeSelf)
		{
			CreateRoomFrame.SetActive(true);
		}
	}

	//x ��ư�� ���� ��� ���� �Լ��� ����ȴ�.
	public void OnExitCreateRoom()
	{
		if (CreateRoomFrame.activeSelf)
		{
			CreateRoomFrame.SetActive(false);
		}
	}

	public void OnClickedJoinRoom()
	{
		if (!JoinRoomFrame.activeSelf)
		{
			JoinRoomFrame.SetActive(true);
		}
	}

	public void OnExitJoinRoom()
	{
        if (JoinRoomFrame.activeSelf)
        {
			JoinRoomFrame.SetActive(false);
        }
		if (PassWordFrame.activeSelf)
		{
			PassWordFrame.SetActive(false);
		}
    }

	public void OnRefreshRooms()
	{
		PhotonNetwork.JoinLobby();
	}

	//�� ���� ��ư�� ������ ȣ��Ǵ� �Լ�
	public void	OnPassWord(RoomInfo roomInfo)
	{
		//�����ϰ��� �ϴ� �� ������ ������ ���´�.
		CurrentRoomInfo = roomInfo;
		//�н����� �Է��ϴ� UI�� �� ��
		if (!PassWordFrame.activeSelf)
		{
			PassWordFrame.SetActive(true);
			//�ش� inputfield�� ���� �ʱ�ȭ�����ش�.
			InputField field = PassWordFrame.GetComponentInChildren<InputField>();
			field.text = "";
			field.ActivateInputField();  // Ŀ�� �ڵ� ��ġ
		}
	}

	//password�� �Է� �� ��, enter key Ȥ�� Ŀ���� �������� ȣ��ȴ�.
	//Input Field�� On End Edit ���� ȣ��ȴ�.
	public void EnterPassWord()
	{
		//�켱 PasswordFrame���� InputField�� ã�´�.
		InputField field = PassWordFrame.GetComponentInChildren<InputField>();
		//���� �ش� InputField�� ���� ���� ���� ���
		if(field.text.IsNullOrEmpty())
		{
			//����� ����.
			PopUpAnimController.Instance.PopUpWarning(Warning_NullPass, "��й�ȣ�� �Է��ϼ���!");
			return;
		}

		//�н����带 �����ϰ�
		string password = field.text.Trim();
		//�� ������ ���� ���ڷ� �Ѱ��ش�.
		LobbyManager.instance.TryJoinRoom(CurrentRoomInfo, password);
	}

	//�н����� �Է� â �ݱ⸦ ���� ��� ȣ��Ǵ� �Լ�
	public void OnExitPassword()
	{
		if (PassWordFrame.activeSelf)
		{
			PassWordFrame.SetActive(false);
		}
	}

	//�� ���� â���� ������ �Է��ϰ�, Create ��ư�� ������ ȣ��Ǵ� �Լ�
	public void OnClickedCreateBtn()
	{
		//��� ��Ұ� �Էµ��� ���� ���¶��, Warning �޽����� �����Ѵ�.
		if (RoomName.text.IsNullOrEmpty() || Password.text.IsNullOrEmpty())
		{
			PopUpAnimController.Instance.PopUpWarning(Warning_Create, "��� ��Ҹ� �Է��ϼ���!");
			return;
		}

		LobbyManager.CreateRoomWithPassword(RoomName.text, Password.text.Trim());
	}
}
