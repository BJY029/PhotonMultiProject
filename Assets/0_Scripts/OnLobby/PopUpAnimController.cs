using UnityEngine;
using UnityEngine.UI;

public class PopUpAnimController : MonoBehaviour
{
    //�̱��� ����
    public static PopUpAnimController Instance;

    //�� PopUp�� ������Ʈ��
    public GameObject Warning_Create;
    public GameObject Warning_MaxPlayer;
    public GameObject Warning_NullPassword;
    public GameObject Warning_Password;

    //�ִϸ�����
    Animator PopUpAnimator;

	private void Awake()
	{
        //�̱��� �۾�
		if(Instance == null) Instance = this;
        
        //������ ������Ʈ���� �ʱ�ȭ ���ش�.
        Warning_Create.transform.localScale = Vector3.one;
        Warning_MaxPlayer.transform.localScale = Vector3.one;
        Warning_NullPassword.transform.localScale = Vector3.one;
        Warning_Password.transform.localScale=Vector3.one;

        Warning_Create.SetActive(false);
        Warning_MaxPlayer.SetActive(false);
        Warning_NullPassword.SetActive(false);
        Warning_Password.SetActive(false);
	}

    //PopUp �ִϸ��̼��� ����ϴ� �Լ�
    //PopUp�� ������Ʈ�� �ۼ��� �ؽ�Ʈ�� ���ڷ� �޴´�.
    public void PopUpWarning(GameObject popUpObj, string temp)
    {
        //�Ѿ�� ���ڰ� ���� ���� ��� ������Ʈ�ΰ��
        if(popUpObj == Warning_Create)
        {
            //�ִϸ����߸� �ش� ������Ʈ�� �ִϸ����ͷ� �ʱ�ȭ �� ��
			PopUpAnimator = Warning_Create.GetComponent<Animator>();

            //�ش� ������Ʈ�� Ȱ��ȭ �Ѵ�.
			Warning_Create.SetActive(true);
            //GetComponentInChildren<>�� ���� �ؽ�Ʈ ������Ʈ�� �޾ƿͼ� �����Ѵ�.
			Warning_Create.GetComponentInChildren<Text>().text = temp;
            //�ִϸ��̼��� ����Ѵ�.
			PopUpAnimator.Play("PopUpAnim");
		}
        else if(popUpObj == Warning_MaxPlayer)
        {
            PopUpAnimator=Warning_MaxPlayer.GetComponent<Animator>();

            Warning_MaxPlayer.SetActive(true);
            Warning_MaxPlayer.GetComponentInChildren<Text>().text=temp;
            PopUpAnimator.Play("PopUpMaxPlayer");
        }
        else if(popUpObj == Warning_NullPassword)
        {
			PopUpAnimator = Warning_NullPassword.GetComponent<Animator>();

			Warning_NullPassword.SetActive(true);
			Warning_NullPassword.GetComponentInChildren<Text>().text = temp;
			PopUpAnimator.Play("PopUpNullPass");
		}
        else if(popUpObj == Warning_Password)
        {
            PopUpAnimator = Warning_Password.GetComponent<Animator>();

            Warning_Password.SetActive(true);
            Warning_Password.GetComponentInChildren<Text>().text = temp;
            PopUpAnimator.Play("PopUpErrorPass");
        }
    }
}
