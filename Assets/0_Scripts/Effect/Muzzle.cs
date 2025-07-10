using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Muzzle : MonoBehaviourPun
{
    //파티클 파괴 시간
    public float DistoryTime = 1.0f;

    //RPC, ALL
    [PunRPC]
    void RPC_Muzzle()
    {
        StartCoroutine(DestoryMuzzle());
    }

    IEnumerator DestoryMuzzle()
    {
        //일정 시간 대기 후
        yield return new WaitForSeconds(DistoryTime);
        //해당 파티클을 네트워크 상에서 파괴한다.
        PhotonNetwork.Destroy(gameObject);
    }
}
