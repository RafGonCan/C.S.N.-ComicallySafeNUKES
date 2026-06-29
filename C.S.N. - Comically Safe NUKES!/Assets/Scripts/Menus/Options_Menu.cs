using UnityEngine;
using UnityEngine.UI;

public class Options_Menu : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private PlayerMovement playerMovement;

    private const string VolumeKey = "MasterVolume";
    private const string SensitivityKey = "MouseSensitivity";

    private void OnEnable()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 2f);

        volumeSlider.value = savedVolume;
        sensitivitySlider.value = savedSensitivity;
        AudioListener.volume = savedVolume;

        if (playerMovement != null)
            playerMovement.SetMouseSensitivity(savedSensitivity);

        volumeSlider.onValueChanged.RemoveAllListeners();
        sensitivitySlider.onValueChanged.RemoveAllListeners();

        volumeSlider.onValueChanged.AddListener(SetVolume);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
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

        if (playerMovement != null)
            playerMovement.SetMouseSensitivity(sensitivity);
    }
}