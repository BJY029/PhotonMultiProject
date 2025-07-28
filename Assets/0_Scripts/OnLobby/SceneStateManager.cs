using UnityEngine;

public class SceneStateManager : MonoBehaviour
{
	//싱글톤
    public static SceneStateManager instance;

	private void Awake()
	{
		if(instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject);
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	//강제로 방이 떠나졌는지 확인하는 플래그
	public bool ForcedToLeaveRoom = false;
}
