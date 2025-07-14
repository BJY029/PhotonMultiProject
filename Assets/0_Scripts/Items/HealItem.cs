using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class HealItem : MonoBehaviour
{
    //아이템 회전 값
    [SerializeField]
    private float rotationSpeedX, rotationSpeedY, rotationSpeedZ;
    //아이템 위아래 진폭 값
    [SerializeField]
    private float amplitude = 0.05f;
    //아이템 위아래 이동 속도 값
    [SerializeField]
    private float frequency = 1f;
    //아이템 기본 위치
    [SerializeField]
    private Vector3 startPos;
    //힐 계수
    [SerializeField]
    private float healingAmount = 35f;
    //아이템을 먹은 플레이어
    private GameObject playerObj;

	private void Start()
	{
		startPos = transform.position + new Vector3(0f, 0.5f, 0f);
	}

    //해당 아이템을 회전 및 위아래로 움직이는 효과를 적용시킨다.
	private void Update()
	{
		transform.Rotate(rotationSpeedX, rotationSpeedY, rotationSpeedZ);
        floatingEffect();
	}
    //sin 함수의 특성을 이용하여 위아래 움직임을 구현한다.
    private void floatingEffect()
    {
        float yOffset = Mathf.Sin(Time.deltaTime * frequency) * amplitude;
        transform.position = startPos + new Vector3(0, yOffset, 0);
    }

    //아이템 콜라이더에 플레이어가 감지될 경우
	private void OnTriggerEnter(Collider other)
	{
        //감지된 플레이어의 PhotonView를 가져온다.
		playerObj = other.gameObject;
		PhotonView view = playerObj.GetComponent<PhotonView>();
		if (playerObj.CompareTag("Runner"))
        {
            Debug.LogWarning("Runner 감지");
            //해당 Runner에게 힐 할 수 있는 함수를 RPC로 호출한다.
            view.RPC("HealHearts", view.Owner, healingAmount);
        }
        else if (playerObj.CompareTag("Seeker"))
        {
			Debug.LogWarning("Seeker 감지");
			//해당 Seeker에게 힐 할 수 있는 함수를 RPC로 호출한다.
			view.RPC("HealHearts", view.Owner, healingAmount);
		}
        //아이템을 없애고 재배치하기 위해 해당 함수를 호출시킨다.
        ItemManager.Instance.photonView.RPC("RPC_PickUpItem", RpcTarget.MasterClient, playerObj.transform.position);
    }
}
