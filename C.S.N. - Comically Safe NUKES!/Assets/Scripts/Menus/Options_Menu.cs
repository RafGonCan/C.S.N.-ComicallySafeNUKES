using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

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

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private const string VolumeKey = "MasterVolume";
    private const string SensitivityKey = "MouseSensitivity";
    private const string GammaKey = "Gamma";

    private ColorAdjustments colorAdjustments;
    private InputSystem_Actions _inputActions;
    private bool _isOpen = false;

    private float _lastSavedVolume = -1f;
    private float _lastSavedSensitivity = -1f;
    private float _lastSavedGamma = -1f;

    private CanvasGroup _canvasGroup;
    private Coroutine _fadeCoroutine;

    // ----- Lifecycle -----

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();
        _inputActions.UI.Cancel.performed += OnCancel;

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (postProcessVolume.profile.TryGet<ColorAdjustments>(out var ca))
                colorAdjustments = ca;
        }

        ApplySavedSettings();

        SetSliderNavigation(volumeSlider);
        SetSliderNavigation(sensitivitySlider);
        SetSliderNavigation(gammaSlider);

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.UI.Cancel.performed -= OnCancel;
            _inputActions.Disable();
        }
    }

    // ----- Apply saved settings at start -----

    private void ApplySavedSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        float savedGamma = PlayerPrefs.GetFloat(GammaKey, 1f);

        AudioListener.volume = savedVolume;
        if (playerMovement != null)
            playerMovement.SetMouseSensitivity(savedSensitivity);
        if (colorAdjustments != null)
            colorAdjustments.postExposure.value = savedGamma;

        _lastSavedVolume = savedVolume;
        _lastSavedSensitivity = savedSensitivity;
        _lastSavedGamma = savedGamma;
    }

    // ----- UI Show/Hide -----

    public void OpenSettings()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.interactable = true;

        _isOpen = true;

        foreach (GameObject btn in mainMenuButtons)
            if (btn != null) btn.SetActive(false);

        if (playerMovement != null)
            playerMovement.CanMove = false;

        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
        float savedSensitivity = PlayerPrefs.GetFloat(SensitivityKey, 1f);
        float savedGamma = PlayerPrefs.GetFloat(GammaKey, 1f);

        volumeSlider.SetValueWithoutNotify(savedVolume);
        sensitivitySlider.SetValueWithoutNotify(savedSensitivity);
        if (gammaSlider != null)
            gammaSlider.SetValueWithoutNotify(savedGamma);

        AudioListener.volume = savedVolume;
        if (playerMovement != null)
            playerMovement.SetMouseSensitivity(savedSensitivity);
        if (gammaSlider != null) ApplyGamma(savedGamma);

        _lastSavedVolume = savedVolume;
        _lastSavedSensitivity = savedSensitivity;
        _lastSavedGamma = savedGamma;

        volumeSlider.onValueChanged.RemoveAllListeners();
        sensitivitySlider.onValueChanged.RemoveAllListeners();
        if (gammaSlider != null) gammaSlider.onValueChanged.RemoveAllListeners();

        volumeSlider.onValueChanged.AddListener(SetVolume);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        if (gammaSlider != null) gammaSlider.onValueChanged.AddListener(SetGamma);

        EventSystem.current.SetSelectedGameObject(volumeSlider.gameObject);
    }

    // Fade-out version (used from main menu)
    public void CloseSettings()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        _isOpen = false;
        _fadeCoroutine = StartCoroutine(FadeOutCoroutine());
    }

    // Immediate close (used from pause menu)
    public void CloseSettingsImmediate()
    {
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        _isOpen = false;

        foreach (GameObject btn in mainMenuButtons)
            if (btn != null) btn.SetActive(true);

        if (playerMovement != null)
            playerMovement.CanMove = true;

        if (mainMenuButtons.Length > 0 && mainMenuButtons[0] != null)
            EventSystem.current.SetSelectedGameObject(mainMenuButtons[0]);
        else
            EventSystem.current.SetSelectedGameObject(null);
    }

    private IEnumerator FadeOutCoroutine()
    {
        float elapsed = 0f;
        float startAlpha = _canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        foreach (GameObject btn in mainMenuButtons)
            if (btn != null) btn.SetActive(true);

        if (playerMovement != null)
            playerMovement.CanMove = true;

        if (mainMenuButtons.Length > 0 && mainMenuButtons[0] != null)
            EventSystem.current.SetSelectedGameObject(mainMenuButtons[0]);
        else
            EventSystem.current.SetSelectedGameObject(null);

        _fadeCoroutine = null;
    }

    // ----- Cancel input -----

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (_isOpen)
            CloseSettings();
    }

    // ----- Slider callbacks -----

    public void SetVolume(float volume)
    {
        if (Mathf.Approximately(volume, _lastSavedVolume)) return;
        _lastSavedVolume = volume;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat(VolumeKey, volume);
        PlayerPrefs.Save();
    }

    public void SetSensitivity(float sensitivity)
    {
        if (Mathf.Approximately(sensitivity, _lastSavedSensitivity)) return;
        _lastSavedSensitivity = sensitivity;
        PlayerPrefs.SetFloat(SensitivityKey, sensitivity);
        PlayerPrefs.Save();
        if (playerMovement != null)
            playerMovement.SetMouseSensitivity(sensitivity);
    }

    public void SetGamma(float gamma)
    {
        if (Mathf.Approximately(gamma, _lastSavedGamma)) return;
        _lastSavedGamma = gamma;
        PlayerPrefs.SetFloat(GammaKey, gamma);
        PlayerPrefs.Save();
        ApplyGamma(gamma);
    }

    private void ApplyGamma(float gamma)
    {
        if (colorAdjustments == null) return;
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