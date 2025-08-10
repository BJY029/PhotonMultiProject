using UnityEngine;
using Photon.Pun;
using System.Collections;

public class MinimapManager : MonoBehaviourPun
{
	//특정 플레이어 기준 자기 팀에 적용할 아이콘
    [SerializeField] private GameObject allyIconPrefab;
	//특정 플레이어 기준 적 팀에 적용할 아이콘
	[SerializeField] private GameObject enemyIconPrefab;

	//자신의 아이콘
    private GameObject icon;
	//해당 플레이어 피아 식별
	private bool isEnemy;

	private void Start()
	{
		//해당 스크립트는 특정 플레이어를 제외하고 다른 플레이어들이 실행한다!
		if (!photonView.IsMine)
		{
			//내 팀의 역할을 받아온다.
			string myTeam = PhotonNetwork.LocalPlayer.CustomProperties["Role"].ToString();
			//해당 코드를 실행하는 플레이어의 역할을 받아온다.
			string otherTeam = photonView.Owner.CustomProperties["Role"].ToString();

			//속한 팀이 같은 경우
			if (myTeam == otherTeam)
			{
				//해당 코드를 실행하는 오브젝트의 자식에 팀원 전용 아이콘을 로컬로 생성시킨다.
				icon = Instantiate(allyIconPrefab, transform);
				isEnemy = false;
			}
			else //속한 팀이 다른 경우
			{
				//해당 코드를 실행하는 오브젝트의 자식에 적 전용 아이콘을 로컬로 생성하고
				icon = Instantiate(enemyIconPrefab, transform);
				//비활성화 한다.
				icon.SetActive(false);
				isEnemy = true;
			}

			//생성된 아이콘의 위치 설정
			icon.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
			icon.transform.localPosition = new Vector3(0f, 10f, 0f);

			//각 플레이어들은 해당 이벤트를 구독
			SeekerManager.OnRevealChange += HandleRevealChanged;
		}	
	}

	//bool 여부에 따라 관련 이벤트 실행 함수
	private void HandleRevealChanged(bool show)
	{
		//적이 아닌 경우 실행 안함
		if (!isEnemy || icon == null) return;

		//false인 경우, 비활성화하고 리턴
		if (!show)
		{
			icon.SetActive(false);
			return;
		}

		//적 아이콘 활성화
		icon.SetActive(true);
	}

	//해당 플레이어가 파괴되면, 아이콘도 삭제한다.
	private void OnDestroy()
	{
		SeekerManager.OnRevealChange -= HandleRevealChanged;
		if (icon != null)
		{
			Destroy(icon);
		}
	}
}
