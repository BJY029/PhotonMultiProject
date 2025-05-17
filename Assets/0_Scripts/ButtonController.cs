using Photon.Pun;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
	//Menu â
	[SerializeField] private GameObject Menu;

	private void Start()
	{
		//Menu â�� ó���� ũ�Ⱑ 0���� �ʱ�ȭ�Ǿ� �ֱ� ������, ũ�⸦ 1�� �����.
		Menu.transform.localScale = Vector3.one;
		//�׸��� Menu�� ��Ȱ��ȭ �Ѵ�.
		Menu.SetActive(false);
	}

	private void Update()
	{
		//ESCŰ�� ������
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			//Menu â�� ����Ѵ�.
			ToggleMenu();
		}
	}

	//Menu â�� ����ϴ� �Լ�
	public void ToggleMenu()
    {
		//Menu â�� ����������
		if (Menu.activeSelf)
		{
			//Menu�� ��Ȱ��ȭ �ϰ�
			Menu.SetActive(false);
			//���콺 ��� �� Ŀ���� �Ⱥ��̰� ó���Ѵ�.
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		else//Menu â�� ����������
		{
			//Menu�� Ȱ��ȭ�ϰ�
			Menu.SetActive(true);
			//���콺 ��� ���� �� Ŀ���� ���̰� ó���Ѵ�.
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
    }

	//EXIT ��ư�� OnButtonClicke�� ����� �̺�Ʈ
    public void ExitButtonClicked()
    {
		//���� ������.
		//���Ŀ��� �κ�� ������� �����Ѵ�.
		Application.Quit();
    }
}
