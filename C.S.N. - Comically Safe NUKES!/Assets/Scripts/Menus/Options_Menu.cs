using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Options_Menu : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider gammaSlider;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private GameObject[] mainMenuButtons;

    [Header("UI Navigation")]
    [SerializeField] private Button backButton;

    private const string VolumeKey = "MasterVolume";
    private const string SensitivityKey = "MouseSensitivity";
    private const string GammaKey = "Gamma";

    private ColorAdjustments colorAdjustments;
    private InputSystem_Actions _inputActions;
    private bool _isOpen = false;

    // Prevent saving when value hasn't changed
    private float _lastSavedVolume = -1f;
    private float _lastSavedSensitivity = -1f;
    private float _lastSavedGamma = -1f;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();
        _inputActions.UI.Cancel.performed += OnCancel;
    }

    private void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (postProcessVolume.profile.TryGet<ColorAdjustments>(out var ca))
                colorAdjustments = ca;
        }

        SetSliderNavigation(volumeSlider);
        SetSliderNavigation(sensitivitySlider);
        SetSliderNavigation(gammaSlider);

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.UI.Cancel.performed -= OnCancel;
            _inputActions.Disable();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (_isOpen)
            CloseOptions();
    }

    public void OpenOptions()
    {
        gameObject.SetActive(true);
        _isOpen = true;

        foreach (GameObject btn in mainMenuButtons)
            if (btn != null) btn.SetActive(false);

        if (playerMovement != null)
            playerMovement.CanMove = false;

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        float savedGamma = PlayerPrefs.GetFloat(GammaKey, 1f);

        // Set slider values WITHOUT triggering listeners
        volumeSlider.SetValueWithoutNotify(savedVolume);
        sensitivitySlider.SetValueWithoutNotify(savedSensitivity);
        if (gammaSlider != null)
            gammaSlider.SetValueWithoutNotify(savedGamma);

        // Apply values directly (so they take effect immediately)
        AudioListener.volume = savedVolume;
        if (playerMovement != null)
            playerMovement.SetMouseSensitivity(savedSensitivity);
        if (gammaSlider != null) ApplyGamma(savedGamma);

        // Store initial values to avoid saving on open
        _lastSavedVolume = savedVolume;
        _lastSavedSensitivity = savedSensitivity;
        _lastSavedGamma = savedGamma;

        // Remove old listeners to avoid duplicates, then add new ones
        volumeSlider.onValueChanged.RemoveAllListeners();
        sensitivitySlider.onValueChanged.RemoveAllListeners();
        if (gammaSlider != null) gammaSlider.onValueChanged.RemoveAllListeners();

        volumeSlider.onValueChanged.AddListener(SetVolume);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        if (gammaSlider != null) gammaSlider.onValueChanged.AddListener(SetGamma);

        EventSystem.current.SetSelectedGameObject(volumeSlider.gameObject);
    }

    public void CloseOptions()
    {
        _isOpen = false;
        gameObject.SetActive(false);

        foreach (GameObject btn in mainMenuButtons)
            if (btn != null) btn.SetActive(true);

        if (playerMovement != null)
            playerMovement.CanMove = true;

        if (mainMenuButtons.Length > 0 && mainMenuButtons[0] != null)
            EventSystem.current.SetSelectedGameObject(mainMenuButtons[0]);
    }

    // --- Slider callbacks with guards ---

    public void SetVolume(float volume)
    {
        // Only save if the value actually changed
        if (Mathf.Approximately(volume, _lastSavedVolume))
            return;

        _lastSavedVolume = volume;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
        Debug.Log($"Volume saved: {volume}");
    }

    public void SetSensitivity(float sensitivity)
    {
        if (Mathf.Approximately(sensitivity, _lastSavedSensitivity))
            return;

        _lastSavedSensitivity = sensitivity;
        PlayerPrefs.SetFloat(SensitivityKey, sensitivity);
        PlayerPrefs.Save();
        if (playerMovement != null)
            playerMovement.SetMouseSensitivity(sensitivity);
        Debug.Log($"Sensitivity saved: {sensitivity}");
    }

    public void SetGamma(float gamma)
    {
        if (Mathf.Approximately(gamma, _lastSavedGamma))
            return;

        _lastSavedGamma = gamma;
        PlayerPrefs.SetFloat(GammaKey, gamma);
        PlayerPrefs.Save();
        ApplyGamma(gamma);
        Debug.Log($"Gamma saved: {gamma}");
    }

    private void ApplyGamma(float gamma)
    {
        if (colorAdjustments == null)
            return;
        colorAdjustments.postExposure.value = gamma;
    }

    private void SetSliderNavigation(Slider slider)
    {
        if (slider != null)
        {
            var nav = slider.navigation;
            nav.mode = Navigation.Mode.Automatic;
            slider.navigation = nav;
        }
    }
}