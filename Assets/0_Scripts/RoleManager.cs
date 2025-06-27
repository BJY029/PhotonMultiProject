using Photon.Pun;
using StarterAssets;
using System;
using System.Collections;
using UnityEngine;

//������ ��Ÿ���� Enum ����
public enum PlayerRoles
{
    Seeker,
    Runner,
    None
}

public class RoleManager : MonoBehaviour
{
	public GameObject camera;

    //�÷��̾��� ������ �����ϴ� ��ü
    private PlayerRoles myRole;
    //������ �������� �����Ǵ� �÷��̾� ��ü
    private GameObject playerObj;
    //�÷��̾� ��ü�κ��� ��� controller ��ũ��Ʈ�� ������ ��ü
    ThirdPersonController PC;

	private void Start()
	{
        //���� �� ������ �������� Ȯ���Ѵ�.
        myRole = GetMyRole();

        //���ҿ� ���� ����Ǵ� �ڵ尡 �����ȴ�.
        switch (myRole)
        {
            case PlayerRoles.Seeker:
                //Seeker�� ���, Seeker ĳ���͸� ���� ������ ��
                SpawnSeeker();
                //�ش� ĳ���ͷκ��� controller�� �޾ƿͼ�
				PC = playerObj.GetComponent<ThirdPersonController>();
                //��Ȱ��ȭ ��Ų��.
				PC.enabled = false;
                //�׸��� Seeker�� ���� �ڷ�ƾ�� �����Ѵ�.
				StartCoroutine(SeekerInit());
				break;
            case PlayerRoles.Runner:
                //Runner�� ���, Runner ĳ���͸� ������ ��
				SpawnRunner();
                //�ش� ĳ���ͷκ��� Controller�� �޾ƿͼ�
                PC = playerObj.GetComponent<ThirdPersonController>();
                //��Ȱ��ȭ ��Ų��.
                PC.enabled = false;
                //�׸��� Runner�� ���� �ڷ�ƾ�� �����Ѵ�.
				StartCoroutine(RunnerInit());
                break;
            default:
                //���׳� ���
                Debug.LogWarning("No Roles! ERROR!");
                break;
        }
	}

    //Runner�� ��� ����Ǵ� �ڷ�ƾ
    private IEnumerator RunnerInit()
    {
        //UI���� ���� ���� ��
		StartCoroutine(Game_UIManager.instance.InitRunnerUI());
        //�ش� �ڷ�ƾ�� ����Ǹ�
        yield return new WaitUntil(() => !Game_UIManager.instance.isIniting);
        //controller�� �ٽ� Ȱ��ȭ�ؼ� ������ �� �ֵ��� �Ѵ�.
        PC.enabled = true;
	}

    //Seeker�� ��� ����Ǵ� �ڷ�ƾ
    private IEnumerator SeekerInit()
    {
        //UI ���� ���� ���� ��
        StartCoroutine(Game_UIManager.instance.InitSeekerUI());
        //�ش� �ڷ�ƾ�� ����Ǹ�
		yield return new WaitUntil(() => !Game_UIManager.instance.isIniting);
        //Controller�� �ٽ� Ȱ��ȭ�ؼ� ������ �� �ֵ��� �Ѵ�.
        PC.enabled = true;
	}

    //�� ������ �������� ã�Ƽ� ��ȯ�ϴ� �Լ�
	public PlayerRoles GetMyRole()
    {
        //�ش� �÷��̾��� Ŀ���� �Ӽ��� ����ִ� hashTable���� Role�� Ű�� ������ �ִ� ���� ���� roldObj�� ��ȯ
        if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out object roleObj))
        {
            //���ڿ��� enum���� ��ȯ�ϴ� �Լ�(��ȯ���� bool ��)
            //roleObj�� ���ڿ��� ������ ��, �ش� ���ڿ��� PlayerRoles enum ������ ��ȯ�Ѵ�.
            //������ �ش� enum�� roleEnum�̸��� object ������ ��ȯ�Ѵ�.
            if(Enum.TryParse(typeof(PlayerRoles), roleObj.ToString(),  out object roleEnum))
            {
				//��ȯ�� roleEnum�� ���� object �� �̹Ƿ�, PlayerRoles ������ ��ȯ�ؼ� ��ȯ�Ѵ�.
				return (PlayerRoles)roleEnum;
            }
        }
        return PlayerRoles.None;
    }

	//�÷��̾ �����ϴ� �Լ�
	void SpawnRunner()
	{
		//������ ��ġ�� �÷��̾� ������ ����
		Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-85.0f, -90f), 2.0f, UnityEngine.Random.Range(10.0f, 30.0f));
		playerObj = PhotonNetwork.Instantiate("RobotKyle", spawnPosition, Quaternion.identity);
		//������ ī�޶� �ϳ��� �����ؾ� �ϸ�, Audio Listener�� �ϳ� �����ؾ� �ϱ� ������, �⺻ ī�޶� ������ ����
		//�÷��̾� �ϳ� �̻��� ���� ���, �⺻ ī�޶� ��Ȱ��ȭ ��Ų��.
		if (camera != null) camera.SetActive(false);
	}

	void SpawnSeeker()
	{
		//������ ��ġ�� �÷��̾� ������ ����
		Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(93.0f, 88.0f), 2.0f, UnityEngine.Random.Range(-6.0f, -7.0f));
		playerObj = PhotonNetwork.Instantiate("Seeker", spawnPosition, Quaternion.identity);
		//������ ī�޶� �ϳ��� �����ؾ� �ϸ�, Audio Listener�� �ϳ� �����ؾ� �ϱ� ������, �⺻ ī�޶� ������ ����
		//�÷��̾� �ϳ� �̻��� ���� ���, �⺻ ī�޶� ��Ȱ��ȭ ��Ų��.
		if (camera != null) camera.SetActive(false);
	}
}
