using Photon.Pun;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Game_UIManager : MonoBehaviour
{
	//�̱������� ����
	public static Game_UIManager instance;

	private void Awake()
	{
		if(instance == null) instance = this;
	}

	//�� ������ �˷��ִ� pannel�� ���ִ� �ִϸ��̼� ����� ���� ����
	public Animator PanelAnim;
	//������ �˷��ֱ� ���� UI��
	public GameObject Panel;
    public Text Role;
	public Text Desc;
	public Text Timer;

	//���� flag��
    public bool isTyping;
	public bool isIniting;

	//���� ����
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

	//Runner UI ������ ����ϴ� �ڷ�ƾ
	public IEnumerator InitRunnerUI()
	{
		isIniting = true;

		Role.text = "";
		Desc.text = "";
		Timer.text = "";

		//Runner���� �˸��� �ؽ�Ʈ�� Ÿ���� �������� �ۼ��س����� �Լ� ȣ��
		//���ڷ� ���� ���� ���� hex ���� string �������� �Ѱ��ָ�, �ڵ� ��ȯ�� ���ִ� �Լ��� �����
		StartCoroutine(TypeSentence("Runner", ColorToHex(Color.green)));
		//�� Ÿ������ ���������� ��� ��
		yield return new WaitUntil(() => !isTyping);
		//�ش� ���ҿ� ���� ������ ������ ���ִ� ���ڿ��� Ÿ�����ϵ��� �Լ� ȣ��
		//�ش� �Լ��� �����ε�� ���� ��
		StartCoroutine(TypeSentence("Take all the strongholds and escape without the seeker's knowledge!"));

		//��� �ۼ��� ������
		yield return new WaitUntil(() => !isTyping);

		//�г��� �ݴ� �ִϸ��̼��� ����ϰ�
		PanelAnim.Play("RoleClose");
		//�ڷ�ƾ�� ����Ǿ����� �˸���.
		isIniting = false;
	}

	//Seeker UI ������ ����ϴ� �ڷ�ƾ
	public IEnumerator InitSeekerUI()
	{
		isIniting = true;

		Role.text = "";
		Desc.text = "";
		Timer.text = "";

		//Seeker���� �˸��� �ؽ�Ʈ�� Ÿ���� �������� �ۼ��س����� �Լ� ȣ��
		//���ڷ� ���� ���� ���� hex ���� string �������� �Ѱ��ָ�, �ڵ� ��ȯ�� ���ִ� �Լ��� �����
		StartCoroutine(TypeSentence("Seeker", ColorToHex(Color.red)));
		//�� Ÿ������ ���� ������ ��� �Ѵ�.
		yield return new WaitUntil(() => !isTyping);
		//Seeker ���ҿ� ���� ������ ������ Ÿ������ ���� �����ְ�
		StartCoroutine(TypeSentence("Catch all the runners pretending to be bots and keep them away from the base!"));
		//Ÿ���� �ۼ��� ������
		yield return new WaitUntil(() => !isTyping);

		//Seeker�� ������ ��� �ð��� �ο��Ѵ�.(30��)
		float remaining = seekerWaitingTime;
		//�Ʒ� �ڵ带 ���� Ÿ�̸Ӹ� ����Ѵ�.
		while(remaining >= 0)
		{
			if(remaining <= 10) Timer.color = Color.red;
			Timer.text = $"{remaining:F0}";
			yield return new WaitForSeconds(1f);
			remaining -= 1f;
		}
		//Ÿ�̸Ӱ� ������, panel�� �ݴ´�.
		PanelAnim.Play("RoleClose");
		//�ڷ�ƾ�� �������� �˸���.
		isIniting = false;
	}

	//Ÿ������ ����ϴ� �Լ�
	//���ڷ� Seeker Ȥ�� Runner ���ڿ��� �ް�, �ش� ���ڿ��� ����� ���� ���� Hex ���ڿ��� �޴´�.
	private IEnumerator TypeSentence(string myRole, string colorHex)
    {
        isTyping = true;
		//�⺻ ���ڿ�
        string str = "You are ";
		//���� ���� ������ ���� ���ڿ���
		string colorStartTag = $"<color={colorHex}>";
		string colorEndTag = "</color>";

		Role.text = "";

		//�⺻ ���ڿ��� ���ʴ�� Ÿ�����Ѵ�.
		foreach (char c in str.ToCharArray())
		{
			Role.text += c;
			yield return new WaitForSeconds(typingSpeed);
		}

		//������ ����ϱ� ���� ������ ���ڿ� ����
		string roleTyped = "";

		foreach (char c in myRole.ToCharArray())
		{
			//�켱 ���� ������ �� ���ڸ� �ش� ���ڿ��� ���� ��
			roleTyped += c;
			//����� UI�� ������ ���� �������� ����Ѵ�.
			//�̷��� ���� ���� ��, ������ ������ ���� �յ� ������� ��µǹ�����.
			Role.text = str + colorStartTag + roleTyped + colorEndTag;

			yield return new WaitForSeconds(typingSpeed);
		}

		//Ÿ������ �������� �˸���.
		isTyping = false;
	}

	//overload �� ������ �Լ�
	//�ش� �Լ��� �׳� ���޹��� ���ڿ��� Ÿ�����ؼ� ����Ѵ�.
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

	//���ڷ� ���� ������ hex ������ ��ȯ�ؼ� ���ڿ��� ��ȯ�ϴ� �Լ�
	private string ColorToHex(Color color)
	{
		Color32 color32 = color;
		return $"#{color32.r:X2}{color32.g:X2}{color32.b:X2}";
	}
}
