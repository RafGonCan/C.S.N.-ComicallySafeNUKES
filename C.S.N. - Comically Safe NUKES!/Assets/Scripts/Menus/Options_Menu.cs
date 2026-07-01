using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Required for URP

public class Options_Menu : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider gammaSlider;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Volume postProcessVolume;

    private const string VolumeKey = "MasterVolume";
    private const string SensitivityKey = "MouseSensitivity";
    private const string GammaKey = "Gamma";

    private ColorAdjustments colorAdjustments;

    private void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (postProcessVolume.profile.TryGet<ColorAdjustments>(out var ca))
            {
                colorAdjustments = ca;
            }
        }
    }

    private void OnEnable()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 2f);
        float savedGamma = PlayerPrefs.GetFloat(GammaKey, 1f);

        volumeSlider.value = savedVolume;
        sensitivitySlider.value = savedSensitivity;
        if (gammaSlider != null) gammaSlider.value = savedGamma;

        AudioListener.volume = savedVolume;
        if (playerMovement != null)
            playerMovement.SetMouseSensitivity(savedSensitivity);
        if (gammaSlider != null) ApplyGamma(savedGamma);

        volumeSlider.onValueChanged.RemoveAllListeners();
        sensitivitySlider.onValueChanged.RemoveAllListeners();
        if (gammaSlider != null) gammaSlider.onValueChanged.RemoveAllListeners();

        volumeSlider.onValueChanged.AddListener(SetVolume);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        if (gammaSlider != null) gammaSlider.onValueChanged.AddListener(SetGamma);
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

    public void SetGamma(float gamma)
    {
        PlayerPrefs.SetFloat(GammaKey, gamma);
        PlayerPrefs.Save();
        ApplyGamma(gamma);
    }

    private void ApplyGamma(float gamma)
    {
        if (colorAdjustments == null)
            return;

        colorAdjustments.postExposure.value = gamma;
    }
}