using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using StarterAssets;
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
	}

	private void Update()
	{
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
				//충전 타이머 초기화
				chargeTimer = 0;
				Game_UIManager.instance.GunUIInit();
				UIChanged = false;
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
            }
			//충전 타이머 초기화
			chargeTimer = 0;
			Game_UIManager.instance.GunUIInit();
			UIChanged = false;
            return;
		}
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
