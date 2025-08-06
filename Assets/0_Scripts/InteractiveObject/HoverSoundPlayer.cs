using UnityEngine;
using UnityEngine.EventSystems;

public class HoverSoundPlayer : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.instance.PlayHoverClip();
    }
}
