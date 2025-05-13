using Photon.Pun;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    void ExitButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
			//masterClient가 지닌 정보 넘겨주는 작업 진행
		}
		Application.Quit();
    }
}
