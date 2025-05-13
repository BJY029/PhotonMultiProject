using Photon.Pun;
using UnityEngine;

public class PhotonHelper
{
	public static Player GetPlayer(int actorNum)
	{
		foreach(var player in PhotonNetwork.PlayerList)
		{
			if (player.ActorNumber == actorNum)
			{
				return player;
			}
		}
		return null;
	}
}
