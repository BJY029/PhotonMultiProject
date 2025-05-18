using UnityEngine;
using UnityEngine.UI;

public class WarningPopUp : MonoBehaviour
{
	//�̱������� ����
	public static WarningPopUp Instance;

	public Text WarningText;
	Animator PopUpAnimator;

	private void Awake()
	{
		if(Instance == null) Instance = this;

		//�̱����̱� ������ active �� �����̸�, ���� 0���� ������ ����
		//���� 1�� �ʱ�ȭ
		this.transform.localScale = Vector3.one;
		//��Ȱ��ȭ
		this.gameObject.SetActive(false);
		PopUpAnimator = GetComponent<Animator>();
	}


	//�ش� �Լ��� LobbyUIManager���� RoomCreate ���ǿ� ���� ������ ȣ��ȴ�.
	public void PopUpWarning(string temp)
	{
		//������Ʈ Ȱ��ȭ
		this.gameObject.SetActive(true);
		WarningText.text = temp;
		//�ִϸ��̼��� ����Ѵ�.
		PopUpAnimator.Play("PopUpAnim");
	}

	//�ִϸ��̼��� �ڵ����� ������Ʈ�� �ݴ� �ִϸ��̼����� ����ǰ�
	//�Ʒ� �̺�Ʈ�� �ݴ� �ִϸ��̼��� �������� ����Ǵ� ������Ʈ ��Ȱ��ȭ �Լ���.
	public void Deactive() => this.gameObject.SetActive(false);
}
