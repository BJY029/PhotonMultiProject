using UnityEngine;

public class MapController : MonoBehaviour
{
    public GameObject Map;

	private void Start()
	{
		Map.SetActive(false);
	}

	// Update is called once per frame
	void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
            ToggleMap();
    }

    void ToggleMap()
    {
        if (Map.activeSelf)
        {
            Map.SetActive(false);
        }
        else
        {
            Map.SetActive(true);
        }
    }
}
