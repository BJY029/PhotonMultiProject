using UnityEngine;

public class CloseUI : MonoBehaviour
{
	public void Deactive() => this.gameObject.SetActive(false);

	public void SetActiveFalse() => this.gameObject.SetActive(false);

	public void SetActiveAndFlat()
	{
		Game_UIManager.instance.AlertUIFlag = false;
		this.gameObject.SetActive(false);
	}
}
