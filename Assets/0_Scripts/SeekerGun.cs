using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using StarterAssets;
using System.Collections;
using UnityEngine.Audio;
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
    //충전 시간
    public float chargeDelay = 2.0f;
    //충전 타이머
    private float chargeTimer;
    //UI가 반복적으로 변경되는 것을 막기 위한 플래그
    private bool UIChanged;



    //Seeker에 붙은 Controller를 저장하기
    private ThirdPersonController TPC;
    //본래 감도를 저장하는 변수
	private float originSensitivity;

    public Transform firePoint;
    //각 프리팹 접근 키 선언
    public string localMuzzleKey = "LocalMuzzle";
    public string localBulletKey = "LocalBullet";
	public string localBulletHitKey = "LocalHit";


	private void Start()
	{
        //다음과 같이 root parent를 검색한다.
		Transform current = transform;
		while (current.parent != null)
		{
			current = current.parent;
		}
        //해당 root parent에 붙은 controller를 가져와서 TPC에 저장한다.
		TPC = current.GetComponent<ThirdPersonController>();

        chargeTimer = 0f;
        UIChanged = false;

        //각 프리팹들을 pool에 삽입한다.
        PoolManager.instance.CreatePool(localMuzzleKey, Resources.Load<GameObject>("LocalPrefabs/SmallEnergyMuzzle"), 5);
		PoolManager.instance.CreatePool(localBulletKey, Resources.Load<GameObject>("LocalPrefabs/SmallEnergyBullet"), 5);
		PoolManager.instance.CreatePool(localBulletHitKey, Resources.Load<GameObject>("LocalPrefabs/SmallEnergyBulletHit"), 5);
	}

	private void Update()
	{
        if (!photonView.IsMine) return;
        if (RoleManager.instance.spawning) return;

        //만약 충전이 아직 안된경우
		if (chargeTimer < chargeDelay)
		{
            //충전한다.
			chargeTimer += Time.deltaTime;
            //이때, blur 오브젝트의 크기를 충전도에 맞춰서 감소시킨다.
            Game_UIManager.instance.blur.transform.localScale = new Vector3(1.0f, (chargeDelay - chargeTimer) / chargeDelay, 1.0f); 
		}
        else
        {
            //만약 충전이 완료되었는데 UI가 변경안된 경우
            if (!UIChanged)
            {
                //UI를 변경하고
                Game_UIManager.instance.GunUICharged();
                //반복 초기화를 막기위해 flag를 true로 설정한다.
                UIChanged = true;
            }
        }

		if (!photonView.IsMine) return;
        //좌클릭이 눌리면 호출되는 함수
        if (Input.GetButtonDown("Fire1"))
            Shoot();
        //우클릭이 눌리면 줌인
        if(Input.GetButtonDown("Fire2"))
            ZoomIn();
        //우클릭이 떼지면 줌아웃
        if (Input.GetButtonUp("Fire2"))
            ZoomOut();
	}

	//총알 발사 함수
	void Shoot()
    {
		//충전이 아직 안된경우 발사를 막는다.
		if (chargeTimer < chargeDelay) return;

        photonView.RPC("RPC_PlayGunShot", RpcTarget.All, transform.position);

        //로컬 환경에서(총 발사한 환경) 프리팹을 생성한다.
		GameObject localMuzzle = PoolManager.instance.GetFromPool(localMuzzleKey, firePoint.position, firePoint.rotation);
        GameObject localBullet = PoolManager.instance.GetFromPool(localBulletKey, firePoint.position, firePoint.rotation);
        //각 프리펩에 달린 코드를 통해 처리를 진행한다.
        localMuzzle.GetComponent<LocalMuzzle>().MuzzleInit();
        localBullet.GetComponent<LocalBullet>().Init(seekerCam.transform.forward, range, localBulletHitKey);

        //총 발사 효과
        ApplyRecoil();

        //그리고 총 발사 정보를 공유하기 위해 해당 RPC 함수를 호출한다.
        //이때 MasterClient만 수행한다.
        photonView.RPC("RPC_RequestShoot", RpcTarget.MasterClient, firePoint.position, seekerCam.transform.forward);

		//충전 타이머 초기화
		chargeTimer = 0;
		Game_UIManager.instance.GunUIInit();
		UIChanged = false;
	}

    //총 발사 효과 적용 함수
    //추후에 수정 할 예정(현재는 단순히 카메라를 뒤로 조금 민다.)
    void ApplyRecoil()
    {
        //shoot 플래그 활성화
        CameraCollision.instance.shoot = true;

        StartCoroutine(kickBack());
    }

    //일정 시간이 지난 후 shoot 플래그를 비활성화 하는 함수
    IEnumerator kickBack()
    {
        yield return new WaitForSeconds(0.2f);
        CameraCollision.instance.shoot = false;
	}


    //MasterClient가 처리하는 Shoot 함수
    [PunRPC]
	void RPC_RequestShoot(Vector3 firePosition, Vector3 fireDir)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        
        //Raycast 정의
        RaycastHit hit;
        Vector3 hitPoint = firePosition + fireDir * range;

		//거점 전용 collider가 Ray에 감지되지 않기 위해서 다음 처리 진행
		int excludeMask = (1 << LayerMask.NameToLayer("Site")) | (1 << LayerMask.NameToLayer("Item"));
		int layerMask = ~excludeMask;

		//seeker 카메라 기준으로, seeker가 바라보는 방향으로 Ray를 발사한다.
		if (Physics.Raycast(firePosition, fireDir, out hit, range, layerMask))
        {
            //맞은 포인트 저장
            hitPoint = hit.point;

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
            }

            //만약 맞은 오브젝트가 Dummy일 경우
            if (hit.transform.CompareTag("Dummy"))
            {
                //해당 Dummy 오브젝트에서 PhotonView를 가져온다.
                var Dummy = hit.transform.GetComponent<PhotonView>();
                //맞은 Dummy에 달린 DestroySelf 함수를 MasterClient가 실행하도록 한다.
				Dummy.RPC("DestroySelf", RpcTarget.MasterClient);
				//Seeker, 즉 자기 자신의 체력을 감소시키는 함수를 호출한다.
				this.photonView.RPC("GetDamagedOnDummy", this.photonView.Owner, damageToSeekerOnDummyHit);
                
				//SeekerManager.Instance.GetDamagedOnDummy(damageToSeekerOnDummyHit);
            }
		}

		//총알 발사 이펙트 및 총알 이펙트를 네트워크 상에서 생성
		GameObject muzzle = PhotonNetwork.Instantiate("SmallEnergyMuzzle",firePosition, Quaternion.LookRotation(fireDir));
		GameObject bullet = PhotonNetwork.Instantiate("SmallEnergyBullet", firePosition, Quaternion.LookRotation(fireDir));
		//각각 이펙트에 붙어있는 PhotonView를 가져온다.
		PhotonView muzzlePV = muzzle.GetComponent<PhotonView>();
		PhotonView bulletPV = bullet.GetComponent<PhotonView>();
		//각 총알에 붙어있는 PhotonView가 모든 클라이언트에게 해당 함수들을 실행하라고 요청한다.
		muzzlePV.RPC("RPC_Muzzle", RpcTarget.All);
		bulletPV.RPC("RPC_MoveBullet", RpcTarget.All, hit.point);
	}

	[PunRPC]
	void RPC_PlayGunShot(Vector3 pos)
	{
        //GunAudio라는 프리팹을 Resources 폴더에서 가져오고
		GameObject prefab = Resources.Load<GameObject>("GunAudio");
        //해당 오브젝트를 pos 위치에 생성한다.
		GameObject go = Instantiate(prefab, pos, Quaternion.identity); //인스턴스 생성 후 변수 저장

        //해당 오디오 소스를 재생하고
		AudioSource audio = go.GetComponent<AudioSource>();
		audio.Play();

        //클립 길이만틈 재생 후 파괴한다.
		Destroy(go, audio.clip.length); // 복제본 제거는 정상
	}


	//줌인 함수
	void ZoomIn()
    {
        //zoom flag를 true로 변경
        CameraCollision.instance.zoom = true;
        
        //본래 감도를 저장하고
        originSensitivity = TPC.Sensitivity;
        //감도를 0.5배 한다.
        TPC.Sensitivity *= 0.5f;
    }

    //줌 아웃 함수
    void ZoomOut()
    {
        //zoom flag를 false로 변경
		CameraCollision.instance.zoom = false;
        //감도를 원래대로 돌려놓는다.
        TPC.Sensitivity = originSensitivity;
	}
}
