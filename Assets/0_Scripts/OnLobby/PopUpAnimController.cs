using UnityEngine;
using UnityEngine.UI;

public class PopUpAnimController : MonoBehaviour
{
    //싱글턴 선언
    public static PopUpAnimController Instance;

    //각 PopUp될 오브젝트들
    public GameObject Warning_Create;
    public GameObject Warning_MaxPlayer;
    public GameObject Warning_NullPassword;
    public GameObject Warning_Password;

    //애니메이터
    Animator PopUpAnimator;

	private void Awake()
	{
        //싱글턴 작업
		if(Instance == null) Instance = this;
        
        //각각의 오브젝트들을 초기화 해준다.
        Warning_Create.transform.localScale = Vector3.one;
        Warning_MaxPlayer.transform.localScale = Vector3.one;
        Warning_NullPassword.transform.localScale = Vector3.one;
        Warning_Password.transform.localScale=Vector3.one;

        Warning_Create.SetActive(false);
        Warning_MaxPlayer.SetActive(false);
        Warning_NullPassword.SetActive(false);
        Warning_Password.SetActive(false);
	}

    //PopUp 애니메이션을 재생하는 함수
    //PopUp될 오브젝트와 작성될 텍스트를 인자로 받는다.
    public void PopUpWarning(GameObject popUpObj, string temp)
    {
        //넘어온 인자가 생성 관련 경고 오브젝트인경우
        if(popUpObj == Warning_Create)
        {
            //애니메이텨를 해당 오브젝트의 애니메이터로 초기화 한 후
			PopUpAnimator = Warning_Create.GetComponent<Animator>();

            //해당 오브젝트를 활성화 한다.
			Warning_Create.SetActive(true);
            //GetComponentInChildren<>를 통해 텍스트 오브젝트를 받아와서 수정한다.
			Warning_Create.GetComponentInChildren<Text>().text = temp;
            //애니메이션을 재생한다.
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
