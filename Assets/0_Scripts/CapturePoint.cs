using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class CapturePoint : MonoBehaviourPun
{
	//거점 이름
	public string pointName;
	//거점 점령 시간
	public float captureTime = 30f;
	//현재 거점 점령 정보
	private float captureProgress = 0f;
	//거점 내 플레이어 수
	private int runnerCount = 0;
	//UI 관련 코루틴을 저장하기 위한 변수
	private Coroutine closeUICoroutine;
	//RPC 호출 빈도 조절을 위한 변수 설졍
	private float rpcCoolDown = 0.1f;
	private float rpcTimer = 0f;

	[Header("UI")]
	//거점 점령 정보 슬라이더
	public GameObject captureUI;
	private Animator UIanimator;
	private Text SiteText;
	private Slider SiteSlider;

	private void Awake()
	{
		//UI 컴포넌트를 받아온다.
		UIanimator = captureUI.GetComponent<Animator>();
		SiteText = captureUI.GetComponentInChildren<Text>();
		SiteSlider = captureUI.GetComponentInChildren<Slider>();
	}

	private void Start()
	{
		//UI 정보를 초기화한다.
		if(SiteSlider != null)
		{
			SiteSlider.maxValue = captureTime;
			SiteSlider.value = captureProgress;
		}

		if(SiteText != null)
		{
			SiteText.text = "Site : " + pointName;
		}

		UIanimator.transform.localScale = Vector3.one;
		UIanimator.gameObject.SetActive(false);
		//거점 리스트에 해당 거점을 등록한다.
		CapturePointManager.Instance.RegisterPoint(this);
	}

	private void Update()
	{
		//오직 MasterClient만 거점 정보를 업데이트 한다.(무결성)
		if (!PhotonNetwork.IsMasterClient) return;

		//RPC Timer 측정
		rpcTimer += Time.deltaTime;
		//만약 거점 내에 Runner 플레이어가 1명 이상 있고, 거점이 아직 점령되지 않았으면
		if(runnerCount > 0 && captureProgress < captureTime)
		{
			//거점 점령 정보를 업데이트 한다.
			//이때 거점 내에 플레이어 수에 따라 점령 속도를 빠르게 하기위해 로그 함수를 활용하여 곱한다.
			captureProgress += Time.deltaTime * Mathf.Log(1 + runnerCount, 2);

			//만약 Rpc가 호출되고 0.1초가 지나면, Rpc를 호출한다.(즉 동기화 주기 0.1초)
			if (rpcTimer >= rpcCoolDown)
			{
				//RPC 동기화
				//해당 점령 정보를 모든 클라이언트에게 RPC를 통해 전달 한다.
				photonView.RPC("UpdateCaptureProgress", RpcTarget.All, captureProgress);
				rpcTimer = 0;
			}

			//만약 점령이 완료되면
			if(captureProgress >= captureTime)
			{
				//점령 정보를 맞추고
				captureProgress = captureTime;
				//점령 완료 함수 호출
				OnCaptured();
			}
		}
	}

	//점령이 완료된 경우
	private void OnCaptured()
	{
		//모든 클라이언트에게 점령이 완료되었음을 RPC를 통해 알린다.
		photonView.RPC("CaptureComplete", RpcTarget.All);
	}

	//MasterClient 전용 거점 내에 플레이어 감지 함수
	//자식 collier에서 감지시 해당 함수가 호출된다.
	public void OnChildTriggerEnter(Collider other)
	{
		//MasterClient 만 사용 가능
		if(!PhotonNetwork.IsMasterClient) return;

		//MasterClient 만 사용 가능
		if (other.CompareTag("Runner"))
		{
			//runner 카운터를 올린다.
			runnerCount++;
		}
	}

	//MasterClient 전용 거점 내에 플레이어 감지 함수
	//자식 collier에서 감지시 해당 함수가 호출된다.
	public void OnChildTriggerExit(Collider other)
	{
		//MasterClient 만 사용 가능
		if (!PhotonNetwork.IsMasterClient) return;

		//MasterClient 만 사용 가능
		if (other.CompareTag("Runner"))
		{
			//runner 카운터를 내린다.'
			//이때 카운터 값이 음수가 되는 것을 막기위해 다음 연산을 활용한다.
			runnerCount = Mathf.Max(0, runnerCount - 1);
		}
	}

	//UI를 관리하는 함수
	public void ShowLocalUI(bool show)
	{
		//만약 show 값이 true이면
		if (show)
		{
			//거점 이름 초기화
			SiteText.text = "Site : " + pointName;
			//UI를 닫는 코루틴이 재생중이라면
			if (closeUICoroutine != null)
			{
				//해당 코루틴을 정지하고
				StopCoroutine(closeUICoroutine);
				//null로 바꾼다.
				closeUICoroutine = null;
			}
			//UI 오브젝트를 활성화 하고
			UIanimator.gameObject.SetActive(true);
			//활성화 애니메이션을 재생한다.
			UIanimator.Play("SliderPopUp");
		}
		else //UI를 닫는 경우
		{
			//UI를 닫는 코루틴이 재생중인 경우
			if (closeUICoroutine != null)
			{
				//해당 코루틴을 중지한다.
				StopCoroutine(closeUICoroutine);
			}
			//UI를 닫는 코루틴을 재생하면서 해당 변수에 저장한다.
			closeUICoroutine = StartCoroutine(CloseUI(captureProgress));
		}
	}

	//UI를 닫는 코루틴
	//원리는 다음과 같다.
		//해당 코루틴은 기본적으로 플레이어가 collier를 나가는 순간 호출된다.
		//그런데, 일부 collier는 한 거점으로 취급되며, 그 거점의 연결 부위에서는 in-out-in 이 순식간에 발생해서 
		//버그가 발생하게 된다. 따라서 다음 코루틴을 활용한다.
		//우선 호출된 시점의 점령도를 저장한다.
		//그리고 3초 대기후, 3초 후의 점령도를 측정해서, 호출된 시점의 점령도와 비교한다.
		//만약 차이가 1초 이상 나지 않는 경우(서버 딜레이 감안) 거점을 벗어낫다고 판단하고, UI를 닫는다.
	IEnumerator CloseUI(float progress)
	{
		//호출된 시점의 점령도를 저장
		float currentProgress = progress;
		Debug.Log(currentProgress);

		float waitTime = 3.0f;
		float time = 0f;
		//3초 대기한다.
		while(time < waitTime)
		{
			time += Time.deltaTime;
			yield return null;
		}

		Debug.Log(captureProgress);
		//3초 전의 점령도와 현재 점령도를 비교해서 1초 이상 차이나지 않는 경우
		if (Mathf.Abs(captureProgress - currentProgress) < 1.0f)
			//거점을 벗어나서 점령이 되지 않았다고 판단. UI를 닫는다.
			UIanimator.Play("SliderClose");
		//그리고 저장한 코루틴을 null로 초기화한다.(종료했음을 의미)
		closeUICoroutine = null;
	}

	//점령도를 모든 client에게 전달하는 RPC 함수
	[PunRPC]
	void UpdateCaptureProgress(float progress)
	{
		//현재 점령도를 매개변수 값으로 초기화하고
		captureProgress = progress;
		if(SiteSlider != null)
		{
			//슬라이더 값도 해당 값으로 반영한다.
			SiteSlider.value = (float)captureProgress;
		}
	}

	//MasterClient가 아닌 Runner Client가 거점에 들어간 경우
	//MasterClient에게 실행하라고 호출되는 함수
	[PunRPC]
	void RPC_OnRunnerEnter()
	{
		//runner Count를 1 증가시킨다.
		runnerCount++;
		
	}

	//MasterClient가 아닌 Runner Client가 거점에서 나간 경우
	//MasterClient에게 실행하라고 호출되는 함수
	[PunRPC]
	void RPC_OnRunnerExit()
	{
		//runner count를 1 감소시킨다.
		runnerCount = Mathf.Max(0, runnerCount - 1);
	}

	//점령이 완료되었을 때 모든 클라이언트에게 호출되는 함수
	[PunRPC]
	void CaptureComplete()
	{
		//점령 정보를 맞추고
		captureProgress = captureTime;
		if(SiteSlider != null) SiteSlider.value = captureProgress;

		//임시로 점령 완료했음을 디버그로 알림
		Debug.Log($"{pointName} 점령 완료!");
		//거점 점령 완료를 UI에 표시
		Game_UIManager.instance.SetSiteComplete(pointName);
		//UI를 닫고
		UIanimator.Play("SliderClose");
		//해당 거점 자체를 비활성화한다.
		gameObject.SetActive(false);
	}
}
