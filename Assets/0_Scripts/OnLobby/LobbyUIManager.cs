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
	public InputField RoomName;
	public InputField Password;


	private void Start()
	{
		//�̱����� ���������� ����Ǳ� ���ؼ� active �� ���·� ������ ���۵ǹǷ�
		//���ص��� �ʰ� ���� localScale�� 0���� ������ ����
		//���� localScale�� �ٽ� 1�� �������ش�.
		CreateRoomFrame.transform.localScale = Vector3.one;
		//�׸��� ��Ȱ��ȭ�Ѵ�.
		CreateRoomFrame.gameObject.SetActive(false);
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

	//�� ���� â���� ������ �Է��ϰ�, Create ��ư�� ������ ȣ��Ǵ� �Լ�
	public void OnClickedCreateBtn()
	{
		//��� ��Ұ� �Էµ��� ���� ���¶��, Warning �޽����� �����Ѵ�.
		if (RoomName.text.IsNullOrEmpty() || Password.text.IsNullOrEmpty())
		{
			WarningPopUp.Instance.PopUpWarning("��� ��Ҹ� �Է��ϼ���!");
			return;
		}

		LobbyManager.CreateRoomWithPassword(RoomName.text, Password.text);
	}
}
