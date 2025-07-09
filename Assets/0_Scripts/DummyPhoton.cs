using UnityEngine;
using Photon.Pun;

public class DummyPhoton : MonoBehaviourPun
{
    //자기 자신을 파괴하는 함수
    [PunRPC]
    public void DestroySelf()
    {
        //MasterClient만 해당 함수를 수행한다.
        if (PhotonNetwork.IsMasterClient)
            //해당 오브젝트가 모든 클라이언트에서 자동 파괴된다.
            PhotonNetwork.Destroy(gameObject);
    }
}
