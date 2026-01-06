using UnityEngine;
using UnityEngine.UI;

public class Options_Menu : MonoBehaviour
{
    [SerializeField] private Slider volumeslider;

    private const string VolumeKey = "MasterVolume";

    void Start()
    {
        LoadVolume();
        volumeslider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    void LoadVolume()
    {
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = volume;
        volumeslider.value = volume;
    }
}
