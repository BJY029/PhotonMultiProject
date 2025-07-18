using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class KeyPadManager : MonoBehaviourPun
{
	//싱글턴화
	public static KeyPadManager Instance;

	private void Awake()
	{
		if(Instance == null) Instance = this;
	}

	//거점 점령이 완료되면 횔성화 할 KeyPad 코드를 관리하기 위해 리스트 선언
	public List<GameObject> KeyPads = new List<GameObject>();
	//로컬 플레이어
	private GameObject player;
	//해당 로컬 플레이어의 역할 정보
	private PlayerRoles myRole;
	//해당 로컬 플레이어의 KeyPad 상호작용 스크립트
	private RunnerObjectSensor ros;

	//키패드 번호
	[SerializeField]
	private int keyCodeValue;

	private void Start()
	{
		//MasterClient만 실행한다.
		if (!PhotonNetwork.IsMasterClient) return;
		//랜덤 키패드 번호를 생성하고
		int value = getRandomKeyCode();
		//RPC로 모든 클라이언트들의 KeyPad 관련 스크립트를 비활성화 하도록 한다.
		photonView.RPC(nameof(InitKeyPads), RpcTarget.All, value);
	}

	//랜덤 키패드 번호를 생성해서 반환하는 함수
	private int getRandomKeyCode()
	{
		int value = 0;
		for (int i = 0; i < 4; i++)
		{
			if (i == 0)
				value = value * 10 + UnityEngine.Random.Range(1, 10);
			else value = value * 10 + UnityEngine.Random.Range(0, 10);
		}
		return value;
	}

	//키패드 코드 프로퍼티
	public int getKeyCodeValue()
	{
		return keyCodeValue;
	}

	//키패드 관련 설정 초기화
	[PunRPC]
	void InitKeyPads(int keycodValue)
	{
		//각 클라이언트들은 키패드 오브젝트의 KeyCode를 초기화한다.
		foreach (GameObject keypad in KeyPads)
		{
			NavKeypad.Keypad kp = keypad.GetComponent<NavKeypad.Keypad>();
			kp.setKeypadCombo(keycodValue);
		}
		this.keyCodeValue = keycodValue;

		//만약 해당 플레이어의 역할이 Runner인 경우
		myRole = RoleManager.instance.GetMyRole();
		if (myRole == PlayerRoles.Runner)
		{
			//로컬 플레이어 오브젝트를 가져오고
			player = RoleManager.instance.getPlayerObj();
			//해당 로컬 플레이어에 장착되어 있는 RunnerObjectSensor 스크립트를 비활성화 해서,
			//캐패드와의 상호작용을 막는다.
			ros = player.GetComponent<RunnerObjectSensor>();
			ros.enabled = false;
		}
		//Seeker는 그냥 null로 초기화
		else ros = null;
	}

	//각 KeyPad 상호작용을 활성화해주는 함수
	[PunRPC]
	public void ActiveKeyPads()
	{
		//Runner이고, 플레이어 오브젝트가 존재하면
		if(myRole == PlayerRoles.Runner &&  ros != null)
			//해당 스크립트를 활성화해서, 상호작용이 가능하게 해준다.
			ros.enabled = true;
	}
}
