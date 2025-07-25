using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTracker : MonoBehaviour
{
	//싱글턴
    public static PlayerTracker instance;
	//PhotonNetwork의 Player를 Key로 해서 해당 플레이어의 Object를 찾기 위한 딕셔너리
    private Dictionary<Player, GameObject> playerObjects = new Dictionary<Player, GameObject>();
	//각 플레이어가 등록되거나 삭제될 때 실행될 이벤트
	public event Action OnAlivePlayersChanged;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//플레이어가 등록될 때 호출되는 함수
	public void Register(Player player, GameObject obj)
	{
		//해당 플레이어가 딕셔너리에 없는 경우
		if(!playerObjects.ContainsKey(player))
		{
			//딕셔너리에 추가하고
			playerObjects.Add(player, obj);
			//이벤트가 등록되어 있을 경우, 해당 이벤트를 invoke 한다.
			OnAlivePlayersChanged?.Invoke();
		}
		//테스트 용
		Debug.Log(playerObjects.Count);
	}

	//플레이어가 해제될 때 호출되는 함수
	public void Unregister(Player player)
	{
		//해당 플레이어가 딕셔너리 내에 존재하는 경우
		if (playerObjects.ContainsKey(player))
		{
			//해당 플레이어를 딕셔너리에서 삭제하고
			playerObjects.Remove(player);
			//이벤트가 등록되어 있을 경우 해당 이벤트를 invoke 한다.
			OnAlivePlayersChanged?.Invoke();

			//MasterClient의 경우, 게임 종료 여부를 확인한다.
			if (PhotonNetwork.IsMasterClient)
			{
				CheckGameOver();
			}
		}
		Debug.Log(playerObjects.Count);
	}

	//플레이어의 Key를 기반으로 해당 플레이어 오브젝트를 반환해주는 함수
	public GameObject GetPlayerObject(Player player)
	{
		//해당 player가 딕셔너리에 존재하면 object를 반환
		if(playerObjects.TryGetValue(player, out GameObject obj)) return obj;
		Debug.LogWarning("GetPlayerObject ERROR : NULL Return");
		return null;
	}

	//딕셔너리의 key를 리스트로 반환하는 함수
	public List<Player> GetAlivePlayers()
	{
		return playerObjects.Keys.ToList();
	}

	//게임 종료 확인
	public void CheckGameOver()
	{
		//해당 딕셔너리에 한명의 플레이어만 저장되어 있고
		if(playerObjects.Count == 1)
		{
			//해당 딕셔너리의 Player 객체를 받아온 후
			Player lastPlayer = playerObjects.First().Key;
			//그 Player의 커스텀 프로퍼티에 저장된 역할이
			if(lastPlayer.CustomProperties.TryGetValue("Role", out object role)){
				//Seeker인 경우, Seeker가 최종 생존자
				if(role.ToString() == "Seeker")
				{
					//따라서, 게임을 종료하고 Seeker로 게임 우승을 처리한다.
					GameResultManager.instance.photonView.RPC("EndGame", RpcTarget.All, "Seeker");
				}
			}
		}
	}
}
