using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Game_UIManager : MonoBehaviour
{
	//싱글턴으로 제작
	public static Game_UIManager instance;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//내 역할을 알려주는 pannel을 없애는 애니메이션 재생을 위한 선언
	public Animator PanelAnim;
	//역할을 알려주기 위한 UI들
	public GameObject Panel;
    public Text Role;
	public Text Desc;
	public Text Timer;

	//각종 flag들
    public bool isTyping;
	public bool isIniting;

	//각종 값들
    [SerializeField]
    private float typingSpeed;
	[SerializeField]
	private float waitingTime;
	[SerializeField]
	private float seekerWaitingTime;

	private void Start()
	{
		Panel.transform.localScale = Vector3.one;
	}

	//Runner UI 설정을 담당하는 코루틴
	public IEnumerator InitRunnerUI()
	{
		isIniting = true;

		Role.text = "";
		Desc.text = "";
		Timer.text = "";

		//Runner임을 알리는 텍스트를 타이핑 형식으로 작성해나가는 함수 호출
		//인자로 사용된 색상 값은 hex 값을 string 형식으로 넘겨주며, 자동 변환을 해주는 함수를 사용함
		StartCoroutine(TypeSentence("Runner", ColorToHex(Color.green)));
		//위 타이핑이 끝날때까지 대기 후
		yield return new WaitUntil(() => !isTyping);
		//해당 역할에 대한 간단한 설명을 해주는 문자열을 타이핑하도록 함수 호출
		//해당 함수는 오버로드로 구현 됨
		StartCoroutine(TypeSentence("Take all the strongholds and escape without the seeker's knowledge!"));

		//모두 작성이 끝나면
		yield return new WaitUntil(() => !isTyping);

		//패널을 닫는 애니메이션을 재생하고
		PanelAnim.Play("RoleClose");
		//코루틴이 종료되었음을 알린다.
		isIniting = false;
	}

	//Seeker UI 설정을 담당하는 코루틴
	public IEnumerator InitSeekerUI()
	{
		isIniting = true;

		Role.text = "";
		Desc.text = "";
		Timer.text = "";

		//Seeker임을 알리는 텍스트를 타이핑 형식으로 작성해나가는 함수 호출
		//인자로 사용된 색상 값은 hex 값을 string 형식으로 넘겨주며, 자동 변환을 해주는 함수를 사용함
		StartCoroutine(TypeSentence("Seeker", ColorToHex(Color.red)));
		//위 타이핑이 끝날 때까지 대기 한다.
		yield return new WaitUntil(() => !isTyping);
		//Seeker 역할에 대한 간단한 설명을 타이핑을 통해 보여주고
		StartCoroutine(TypeSentence("Catch all the runners pretending to be bots and keep them away from the base!"));
		//타이핑 작성이 끝나면
		yield return new WaitUntil(() => !isTyping);

		//Seeker는 별도의 대기 시간을 부여한다.(30초)
		float remaining = seekerWaitingTime;
		//아래 코드를 통해 타이머를 재생한다.
		while(remaining >= 0)
		{
			if(remaining <= 10) Timer.color = Color.red;
			Timer.text = $"{remaining:F0}";
			yield return new WaitForSeconds(1f);
			remaining -= 1f;
		}
		//타이머가 끝나면, panel을 닫는다.
		PanelAnim.Play("RoleClose");
		//코루틴이 끝났음을 알린다.
		isIniting = false;
	}

	//타이핑을 담당하는 함수
	//인자로 Seeker 혹은 Runner 문자열을 받고, 해당 문자열을 출력할 색상 값을 Hex 문자열로 받는다.
	private IEnumerator TypeSentence(string myRole, string colorHex)
    {
        isTyping = true;
		//기본 문자열
        string str = "You are ";
		//색상 값을 입히기 위한 문자열들
		string colorStartTag = $"<color={colorHex}>";
		string colorEndTag = "</color>";

		Role.text = "";

		//기본 문자열을 차례대로 타이핑한다.
		foreach (char c in str.ToCharArray())
		{
			Role.text += c;
			yield return new WaitForSeconds(typingSpeed);
		}

		//역할을 출력하기 위한 별도의 문자열 선언
		string roleTyped = "";

		foreach (char c in myRole.ToCharArray())
		{
			//우선 역할 글자의 한 글자를 해당 문자열에 저장 후
			roleTyped += c;
			//사용자 UI에 다음과 같은 연산으로 출력한다.
			//이렇게 하지 않을 시, 색상값을 입히기 위한 앞뒤 명령줄이 출력되버린다.
			Role.text = str + colorStartTag + roleTyped + colorEndTag;

			yield return new WaitForSeconds(typingSpeed);
		}

		//타이핑이 끝났음을 알린다.
		isTyping = false;
	}

	//overload 로 구현된 함수
	//해당 함수는 그냥 전달받은 문자열을 타이핑해서 출력한다.
	private IEnumerator TypeSentence(string text)
	{
		isTyping = true;
		Desc.text = "";

		foreach(char c in text.ToCharArray())
		{
			Desc.text += c;
			yield return new WaitForSeconds(typingSpeed);
		}

		isTyping = false;
	}

	//인자로 받은 색상을 hex 값으로 변환해서 문자열로 반환하는 함수
	private string ColorToHex(Color color)
	{
		Color32 color32 = color;
		return $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}";
	}
}
