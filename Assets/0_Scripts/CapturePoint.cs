using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class CapturePoint : MonoBehaviourPun
{
	//���� �̸�
	public string pointName;
	//���� ���� �ð�
	public float captureTime = 30f;
	//���� ���� ���� ����
	private float captureProgress = 0f;
	//���� �� �÷��̾� ��
	private int runnerCount = 0;
	//UI ���� �ڷ�ƾ�� �����ϱ� ���� ����
	private Coroutine closeUICoroutine;
	//RPC ȣ�� �� ������ ���� ���� ����
	private float rpcCoolDown = 0.1f;
	private float rpcTimer = 0f;

	[Header("UI")]
	//���� ���� ���� �����̴�
	public GameObject captureUI;
	private Animator UIanimator;
	private Text SiteText;
	private Slider SiteSlider;

	private void Awake()
	{
		//UI ������Ʈ�� �޾ƿ´�.
		UIanimator = captureUI.GetComponent<Animator>();
		SiteText = captureUI.GetComponentInChildren<Text>();
		SiteSlider = captureUI.GetComponentInChildren<Slider>();
	}

	private void Start()
	{
		//UI ������ �ʱ�ȭ�Ѵ�.
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
		//���� ����Ʈ�� �ش� ������ ����Ѵ�.
		CapturePointManager.Instance.RegisterPoint(this);
	}

	private void Update()
	{
		//���� MasterClient�� ���� ������ ������Ʈ �Ѵ�.(���Ἲ)
		if (!PhotonNetwork.IsMasterClient) return;

		//RPC Timer ����
		rpcTimer += Time.deltaTime;
		//���� ���� ���� Runner �÷��̾ 1�� �̻� �ְ�, ������ ���� ���ɵ��� �ʾ�����
		if(runnerCount > 0 && captureProgress < captureTime)
		{
			//���� ���� ������ ������Ʈ �Ѵ�.
			//�̶� ���� ���� �÷��̾� ���� ���� ���� �ӵ��� ������ �ϱ����� �α� �Լ��� Ȱ���Ͽ� ���Ѵ�.
			captureProgress += Time.deltaTime * Mathf.Log(1 + runnerCount, 2);

			//���� Rpc�� ȣ��ǰ� 0.1�ʰ� ������, Rpc�� ȣ���Ѵ�.(�� ����ȭ �ֱ� 0.1��)
			if (rpcTimer >= rpcCoolDown)
			{
				//RPC ����ȭ
				//�ش� ���� ������ ��� Ŭ���̾�Ʈ���� RPC�� ���� ���� �Ѵ�.
				photonView.RPC("UpdateCaptureProgress", RpcTarget.All, captureProgress);
				rpcTimer = 0;
			}

			//���� ������ �Ϸ�Ǹ�
			if(captureProgress >= captureTime)
			{
				//���� ������ ���߰�
				captureProgress = captureTime;
				//���� �Ϸ� �Լ� ȣ��
				OnCaptured();
			}
		}
	}

	//������ �Ϸ�� ���
	private void OnCaptured()
	{
		//��� Ŭ���̾�Ʈ���� ������ �Ϸ�Ǿ����� RPC�� ���� �˸���.
		photonView.RPC("CaptureComplete", RpcTarget.All);
	}

	//MasterClient ���� ���� ���� �÷��̾� ���� �Լ�
	//�ڽ� collier���� ������ �ش� �Լ��� ȣ��ȴ�.
	public void OnChildTriggerEnter(Collider other)
	{
		//MasterClient �� ��� ����
		if(!PhotonNetwork.IsMasterClient) return;

		//MasterClient �� ��� ����
		if (other.CompareTag("Runner"))
		{
			//runner ī���͸� �ø���.
			runnerCount++;
		}
	}

	//MasterClient ���� ���� ���� �÷��̾� ���� �Լ�
	//�ڽ� collier���� ������ �ش� �Լ��� ȣ��ȴ�.
	public void OnChildTriggerExit(Collider other)
	{
		//MasterClient �� ��� ����
		if (!PhotonNetwork.IsMasterClient) return;

		//MasterClient �� ��� ����
		if (other.CompareTag("Runner"))
		{
			//runner ī���͸� ������.'
			//�̶� ī���� ���� ������ �Ǵ� ���� �������� ���� ������ Ȱ���Ѵ�.
			runnerCount = Mathf.Max(0, runnerCount - 1);
		}
	}

	//UI�� �����ϴ� �Լ�
	public void ShowLocalUI(bool show)
	{
		//���� show ���� true�̸�
		if (show)
		{
			//���� �̸� �ʱ�ȭ
			SiteText.text = "Site : " + pointName;
			//UI�� �ݴ� �ڷ�ƾ�� ������̶��
			if (closeUICoroutine != null)
			{
				//�ش� �ڷ�ƾ�� �����ϰ�
				StopCoroutine(closeUICoroutine);
				//null�� �ٲ۴�.
				closeUICoroutine = null;
			}
			//UI ������Ʈ�� Ȱ��ȭ �ϰ�
			UIanimator.gameObject.SetActive(true);
			//Ȱ��ȭ �ִϸ��̼��� ����Ѵ�.
			UIanimator.Play("SliderPopUp");
		}
		else //UI�� �ݴ� ���
		{
			//UI�� �ݴ� �ڷ�ƾ�� ������� ���
			if (closeUICoroutine != null)
			{
				//�ش� �ڷ�ƾ�� �����Ѵ�.
				StopCoroutine(closeUICoroutine);
			}
			//UI�� �ݴ� �ڷ�ƾ�� ����ϸ鼭 �ش� ������ �����Ѵ�.
			closeUICoroutine = StartCoroutine(CloseUI(captureProgress));
		}
	}

	//UI�� �ݴ� �ڷ�ƾ
	//������ ������ ����.
		//�ش� �ڷ�ƾ�� �⺻������ �÷��̾ collier�� ������ ���� ȣ��ȴ�.
		//�׷���, �Ϻ� collier�� �� �������� ��޵Ǹ�, �� ������ ���� ���������� in-out-in �� ���İ��� �߻��ؼ� 
		//���װ� �߻��ϰ� �ȴ�. ���� ���� �ڷ�ƾ�� Ȱ���Ѵ�.
		//�켱 ȣ��� ������ ���ɵ��� �����Ѵ�.
		//�׸��� 3�� �����, 3�� ���� ���ɵ��� �����ؼ�, ȣ��� ������ ���ɵ��� ���Ѵ�.
		//���� ���̰� 1�� �̻� ���� �ʴ� ���(���� ������ ����) ������ ����ٰ� �Ǵ��ϰ�, UI�� �ݴ´�.
	IEnumerator CloseUI(float progress)
	{
		//ȣ��� ������ ���ɵ��� ����
		float currentProgress = progress;
		Debug.Log(currentProgress);

		float waitTime = 3.0f;
		float time = 0f;
		//3�� ����Ѵ�.
		while(time < waitTime)
		{
			time += Time.deltaTime;
			yield return null;
		}

		Debug.Log(captureProgress);
		//3�� ���� ���ɵ��� ���� ���ɵ��� ���ؼ� 1�� �̻� ���̳��� �ʴ� ���
		if (Mathf.Abs(captureProgress - currentProgress) < 1.0f)
			//������ ����� ������ ���� �ʾҴٰ� �Ǵ�. UI�� �ݴ´�.
			UIanimator.Play("SliderClose");
		//�׸��� ������ �ڷ�ƾ�� null�� �ʱ�ȭ�Ѵ�.(���������� �ǹ�)
		closeUICoroutine = null;
	}

	//���ɵ��� ��� client���� �����ϴ� RPC �Լ�
	[PunRPC]
	void UpdateCaptureProgress(float progress)
	{
		//���� ���ɵ��� �Ű����� ������ �ʱ�ȭ�ϰ�
		captureProgress = progress;
		if(SiteSlider != null)
		{
			//�����̴� ���� �ش� ������ �ݿ��Ѵ�.
			SiteSlider.value = (float)captureProgress;
		}
	}

	//MasterClient�� �ƴ� Runner Client�� ������ �� ���
	//MasterClient���� �����϶�� ȣ��Ǵ� �Լ�
	[PunRPC]
	void RPC_OnRunnerEnter()
	{
		//runner Count�� 1 ������Ų��.
		runnerCount++;
		
	}

	//MasterClient�� �ƴ� Runner Client�� �������� ���� ���
	//MasterClient���� �����϶�� ȣ��Ǵ� �Լ�
	[PunRPC]
	void RPC_OnRunnerExit()
	{
		//runner count�� 1 ���ҽ�Ų��.
		runnerCount = Mathf.Max(0, runnerCount - 1);
	}

	//������ �Ϸ�Ǿ��� �� ��� Ŭ���̾�Ʈ���� ȣ��Ǵ� �Լ�
	[PunRPC]
	void CaptureComplete()
	{
		//���� ������ ���߰�
		captureProgress = captureTime;
		if(SiteSlider != null) SiteSlider.value = captureProgress;

		//�ӽ÷� ���� �Ϸ������� ����׷� �˸�
		Debug.Log($"{pointName} ���� �Ϸ�!");
		//���� ���� �ϷḦ UI�� ǥ��
		Game_UIManager.instance.SetSiteComplete(pointName);
		//UI�� �ݰ�
		UIanimator.Play("SliderClose");
		//�ش� ���� ��ü�� ��Ȱ��ȭ�Ѵ�.
		gameObject.SetActive(false);
	}
}
