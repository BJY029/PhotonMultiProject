using UnityEngine;

public class RoomManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.instance.PlayReadyBgm(AudioManager.instance.GetVolume(AudioMixerType.BGM));
    }
}
