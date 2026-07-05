using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class Pause_Menu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject areYouSure;
    [SerializeField] private GameObject optionsPanel;

    [Header("Buttons (to hide/show)")]
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject optionsButton;
    [SerializeField] private GameObject exitButton;

    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Options_Menu optionsMenu;

    private CameraFocusController cameraFocusController => InteractionManager.instance.CameraFocusController;
    private bool _isPaused;
    private bool _isOptionsOpen = false;
    public bool Paused => _isPaused;

    private InputSystem_Actions _inputActions;
    private InputAction _pauseAction;
    private InputAction _cancelAction;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();

        _pauseAction = _inputActions.Player.Pause;
        _cancelAction = _inputActions.UI.Cancel;
        _cancelAction.performed += OnCancel;
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _cancelAction.performed -= OnCancel;
            _inputActions.Disable();
        }
    }

    private void Update()
    {
        if (_pauseAction.WasPressedThisFrame())
        {
            if (_isOptionsOpen)
            {
                CloseOptionsFromPause();
                return;
            }

            if (_isPaused)
                Resume();
            else if (!cameraFocusController.GetFocusing())
                Pause();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (_isOptionsOpen)
        {
            CloseOptionsFromPause();
            return;
        }

        if (_isPaused)
            Resume();
    }

    // ----- Pause / Resume -----

    public void Pause()
    {
        if (_isPaused) return;
        _isPaused = true;

        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        if (playerMovement != null)
            playerMovement.enabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        SelectUIElement(resumeButton);
    }

    public void Resume()
    {
        if (!_isPaused) return;
        _isPaused = false;

        pauseMenu.SetActive(false);
        Time.timeScale = 1f;

        if (playerMovement != null)
            playerMovement.enabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        EventSystem.current?.SetSelectedGameObject(null);
    }

    // ----- Options -----

    public void OpenOptionsFromPause()
    {
        _isOptionsOpen = true;

        // Hide pause menu buttons
        if (resumeButton != null) resumeButton.SetActive(false);
        if (optionsButton != null) optionsButton.SetActive(false);
        if (exitButton != null) exitButton.SetActive(false);

        // Open the options panel
        if (optionsMenu != null)
            optionsMenu.OpenOptions();
        else if (optionsPanel != null)
            optionsPanel.SetActive(true);

        // Wait one frame for the UI to activate, then select the first slider
        StartCoroutine(SelectFirstSlider());
    }

    private IEnumerator SelectFirstSlider()
    {
        yield return null; // Wait one frame

        // Try to find a slider in the options panel
        Slider slider = null;
        if (optionsMenu != null)
            slider = optionsMenu.GetComponentInChildren<Slider>();
        else if (optionsPanel != null)
            slider = optionsPanel.GetComponentInChildren<Slider>();

        if (slider != null && slider.gameObject.activeInHierarchy && slider.interactable)
        {
            SelectUIElement(slider.gameObject);
        }
        else
        {
            // Fallback: try to select any Selectable
            Selectable selectable = null;
            if (optionsMenu != null)
                selectable = optionsMenu.GetComponentInChildren<Selectable>();
            else if (optionsPanel != null)
                selectable = optionsPanel.GetComponentInChildren<Selectable>();

            if (selectable != null && selectable.gameObject.activeInHierarchy && selectable.interactable)
                SelectUIElement(selectable.gameObject);
        }
    }

    public void CloseOptionsFromPause()
    {
        _isOptionsOpen = false;

        // Close options
        if (optionsMenu != null)
            optionsMenu.CloseOptions();
        else if (optionsPanel != null)
            optionsPanel.SetActive(false);

        // Re-enable pause menu buttons
        if (resumeButton != null) resumeButton.SetActive(true);
        if (optionsButton != null) optionsButton.SetActive(true);
        if (exitButton != null) exitButton.SetActive(true);

        // Re-select the Resume button
        SelectUIElement(resumeButton);
    }

    // ----- Exit / Confirm -----

    public void YouSure()
    {
        areYouSure.SetActive(true);
        Button firstButton = areYouSure.GetComponentInChildren<Button>();
        if (firstButton != null && firstButton.gameObject.activeInHierarchy)
            SelectUIElement(firstButton.gameObject);
    }

    public void NotSure()
    {
        areYouSure.SetActive(false);
        SelectUIElement(exitButton);
    }

    public void Exit()
    {
        _isPaused = false;
        pauseMenu.SetActive(false);
        areYouSure.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // ----- Safe selection helper -----

    private void SelectUIElement(GameObject obj)
    {
        if (obj == null) return;
        if (!obj.activeInHierarchy)
        {
            Debug.LogWarning($"Cannot select {obj.name} – inactive.");
            return;
        }

        Selectable selectable = obj.GetComponent<Selectable>();
        if (selectable != null && !selectable.interactable)
        {
            Debug.LogWarning($"Cannot select {obj.name} – not interactable.");
            return;
        }

        if (EventSystem.current == null)
        {
            Debug.LogWarning("No EventSystem found.");
            return;
        }

        EventSystem.current.SetSelectedGameObject(obj);
        Debug.Log($"Selected: {obj.name}");
    }
}