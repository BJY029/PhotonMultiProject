using Photon.Pun;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    void ExitButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
			//masterClient�� ���� ���� �Ѱ��ִ� �۾� ����
		}
		Application.Quit();
    }
}
