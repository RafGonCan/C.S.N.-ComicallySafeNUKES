using UnityEngine;
using UnityEngine.UI;

public class Options_Menu : MonoBehaviour
{
    [SerializeField] private Slider volumeslider;
    [SerializeField] private Slider sensitivityslider;

    private const string VolumeKey = "MasterVolume";
    private const string SensitivityKey = "MouseSensitivity";

    void Start()
    {

        if (!PlayerPrefs.HasKey(VolumeKey))
            PlayerPrefs.SetFloat(VolumeKey, 1f); 
        if (!PlayerPrefs.HasKey(SensitivityKey))
            PlayerPrefs.SetFloat(SensitivityKey, 2f); 
        PlayerPrefs.Save();

        volumeslider.onValueChanged.RemoveAllListeners();
        sensitivityslider.onValueChanged.RemoveAllListeners();

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        volumeslider.value = savedVolume;
        AudioListener.volume = savedVolume;

        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 2f);
        sensitivityslider.value = savedSensitivity;

        volumeslider.onValueChanged.AddListener(SetVolume);
        sensitivityslider.onValueChanged.AddListener(SetSensitivity);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }
    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(SensitivityKey, sensitivity);
        PlayerPrefs.Save();
    }

    void LoadVolume()
    {
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        AudioListener.volume = volume;
        volumeslider.value = volume;

        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 2f);
        sensitivityslider.value = savedSensitivity;
    }
}
