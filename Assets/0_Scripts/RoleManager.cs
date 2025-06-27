using Photon.Pun;
using StarterAssets;
using System;
using System.Collections;
using UnityEngine;

//역할을 나타내는 Enum 선언
public enum PlayerRoles
{
    Seeker,
    Runner,
    None
}

public class RoleManager : MonoBehaviour
{
	public GameObject camera;

    //플레이어의 역할을 저장하는 객체
    private PlayerRoles myRole;
    //프리팹 생성으로 생성되는 플레이어 객체
    private GameObject playerObj;
    //플레이어 객체로부터 얻는 controller 스크립트를 저장할 객체
    ThirdPersonController PC;

	private void Start()
	{
        //먼저 내 역할이 무엇인지 확인한다.
        myRole = GetMyRole();

        //역할에 따라 수행되는 코드가 결정된다.
        switch (myRole)
        {
            case PlayerRoles.Seeker:
                //Seeker인 경우, Seeker 캐릭터를 먼저 스폰한 후
                SpawnSeeker();
                //해당 캐릭터로부터 controller를 받아와서
				PC = playerObj.GetComponent<ThirdPersonController>();
                //비활성화 시킨다.
				PC.enabled = false;
                //그리고 Seeker를 위한 코루틴을 실행한다.
				StartCoroutine(SeekerInit());
				break;
            case PlayerRoles.Runner:
                //Runner인 경우, Runner 캐릭터를 스폰한 후
				SpawnRunner();
                //해당 캐릭터로부터 Controller를 받아와서
                PC = playerObj.GetComponent<ThirdPersonController>();
                //비활성화 시킨다.
                PC.enabled = false;
                //그리고 Runner를 위한 코루틴을 실행한다.
				StartCoroutine(RunnerInit());
                break;
            default:
                //버그난 경우
                Debug.LogWarning("No Roles! ERROR!");
                break;
        }
	}

    //Runner인 경우 수행되는 코루틴
    private IEnumerator RunnerInit()
    {
        //UI관련 설정 진행 후
		StartCoroutine(Game_UIManager.instance.InitRunnerUI());
        //해당 코루틴이 종료되면
        yield return new WaitUntil(() => !Game_UIManager.instance.isIniting);
        //controller를 다시 활성화해서 움직일 수 있도록 한다.
        PC.enabled = true;
	}

    //Seeker인 경우 수행되는 코루틴
    private IEnumerator SeekerInit()
    {
        //UI 관련 설정 진행 후
        StartCoroutine(Game_UIManager.instance.InitSeekerUI());
        //해당 코루틴이 종료되면
		yield return new WaitUntil(() => !Game_UIManager.instance.isIniting);
        //Controller를 다시 활성화해서 움직일 수 있도록 한다.
        PC.enabled = true;
	}

    //내 역할이 무엇인지 찾아서 반환하는 함수
	public PlayerRoles GetMyRole()
    {
        //해당 플레이어의 커스텀 속성을 담고있는 hashTable에서 Role을 키로 가지고 있는 것의 값을 roldObj로 반환
        if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out object roleObj))
        {
            //문자열을 enum으로 변환하는 함수(반환형은 bool 형)
            //roleObj를 문자열로 변경한 후, 해당 문자열을 PlayerRoles enum 형으로 변환한다.
            //성공시 해당 enum을 roleEnum이름의 object 형으로 반환한다.
            if(Enum.TryParse(typeof(PlayerRoles), roleObj.ToString(),  out object roleEnum))
            {
				//반환된 roleEnum은 현재 object 형 이므로, PlayerRoles 형으로 변환해서 반환한다.
				return (PlayerRoles)roleEnum;
            }
        }
        return PlayerRoles.None;
    }

	//플레이어를 생성하는 함수
	void SpawnRunner()
	{
		//랜덤한 위치에 플레이어 프리팹 생성
		Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-85.0f, -90f), 2.0f, UnityEngine.Random.Range(10.0f, 30.0f));
		playerObj = PhotonNetwork.Instantiate("RobotKyle", spawnPosition, Quaternion.identity);
		//씬에는 카메라가 하나는 존재해야 하며, Audio Listener가 하나 존재해야 하기 때문에, 기본 카메라를 설정해 놓고
		//플레이어 하나 이상이 들어온 경우, 기본 카메라를 비활성화 시킨다.
		if (camera != null) camera.SetActive(false);
	}

	void SpawnSeeker()
	{
		//랜덤한 위치에 플레이어 프리팹 생성
		Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(93.0f, 88.0f), 2.0f, UnityEngine.Random.Range(-6.0f, -7.0f));
		playerObj = PhotonNetwork.Instantiate("Seeker", spawnPosition, Quaternion.identity);
		//씬에는 카메라가 하나는 존재해야 하며, Audio Listener가 하나 존재해야 하기 때문에, 기본 카메라를 설정해 놓고
		//플레이어 하나 이상이 들어온 경우, 기본 카메라를 비활성화 시킨다.
		if (camera != null) camera.SetActive(false);
	}
}
