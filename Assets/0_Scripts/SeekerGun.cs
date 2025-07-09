using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class SeekerGun : MonoBehaviourPun
{
    //Seeker에 부착된 카메라로, 해당 카메라를 기준으로 Ray를 쏜다.
    public Camera seekerCam;
    //Ray 범위
    public float range = 100f;
    //Runner에게 주는 데미지
    public float damageToRunnner = 40f;
    //잘못 쏠 경우 나에게 주는 데미지
    public float damageToSeekerOnDummyHit = 10f;

	private void Update()
	{
        if (!photonView.IsMine) return;
        //좌클릭이 눌리면 호출되는 함수
        if (Input.GetButtonDown("Fire1"))
            Shoot();
	}

    //총알 발사 함수
	void Shoot()
    {
        //Raycast 정의
        RaycastHit hit;
        //seeker 카메라 기준으로, seeker가 바라보는 방향으로 Ray를 발사한다.
        if(Physics.Raycast(seekerCam.transform.position, seekerCam.transform.forward, out hit, range))
        {
            //만약 맞은 오브젝트의 태그가 Runner인 경우
            if (hit.transform.CompareTag("Runner"))
            {
                //해당 Runner 오브젝트에서 RunnerManager 스크립트를 찾아온다.
                var Runner = hit.transform.GetComponentInChildren<RunnerManager>(); 
                if(Runner != null )
                {
                    //해당 스크립트의 PhotonView를 PMPV에 저장한다.
                    PhotonView PMPV = Runner.GetComponent<PhotonView>();
                    //PMPV, 즉 총을 맞은 Runner의 PhotonView에 달린 GetDamageBySeeker 함수를
                        //호출 한다. 이때, 실행자는 맞은 Runner이다.
                    PMPV.RPC("GetDamagedBySeeker", PMPV.Owner, damageToRunnner);
                }
                return;
            }

            //만약 맞은 오브젝트가 Dummy일 경우
            if (hit.transform.CompareTag("Dummy"))
            {
                //해당 Dummy 오브젝트에서 PhotonView를 가져온다.
                var Dummy = hit.transform.GetComponent<PhotonView>();
                //맞은 Dummy에 달린 DestroySelf 함수를 MasterClient가 실행하도록 한다.
				Dummy.RPC("DestroySelf", RpcTarget.MasterClient);
                //Seeker, 즉 자기 자신의 체력을 감소시키는 함수를 호출한다.
				SeekerManager.Instance.GetDamagedOnDummy(damageToSeekerOnDummyHit);
                return;
            }
        }
    }
}
