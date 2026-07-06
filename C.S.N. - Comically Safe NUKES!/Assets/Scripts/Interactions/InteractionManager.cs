using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.Controls;

public class InteractionManager : MonoBehaviour
{
    private static InteractionManager _instance;

    public static InteractionManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<InteractionManager>();
                if (_instance != null)
                    _instance.Init();
            }
            return _instance;
        }
    }

    private string _interactPrefix = "[F]";
    private string _pickPrefix = "Pick";
    private string _awakeAnimationName = "Awake";
    private string _interactAnimationName = "Interact";
    private string _fallbackAnimationName = "InteractWrong";

    public bool showMouse = false;
    private bool _cursorAllowed = false;
    private bool _forceMouseState = false;

    private CameraFocusController _cameraFocusController;
    private PlayerInput _playerInput;
    public InputActionAsset inputActions => _playerInput?.actions;
    private PlayerInventory _playerInventory;
    private PlayerMovement _playerMovement;
    private Pause_Menu _pauseMenu;
    private List<Interactive> _interactives;
    public CameraFocusController CameraFocusController => _cameraFocusController;
    public PlayerInventory playerInventory => _playerInventory;
    public string awakeAnimationName => _awakeAnimationName;
    public string interactAnimationName => _interactAnimationName;
    public string fallbackAnimationName => _fallbackAnimationName;
    private bool _dependenciesProcessed = false;

    // ----- Public Cursor Control -----

    public void SetCursorAllowed(bool allowed)
    {
        _cursorAllowed = allowed;
        UpdateCursorState();
    }

    public void UpdateCursorState()
    {
        // Gameplay: force cursor hidden
        if (!_cursorAllowed)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }

        // Menus: respect showMouse (auto‑detected or user toggle)
        if (showMouse)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ShowMouse(bool var)
    {
        showMouse = var;
        _forceMouseState = true;
        UpdateCursorState();
    }

    // ----- Input Detection (auto‑toggle) -----

    private void Update()
    {
        // Only auto‑detect if menus are allowed (not during gameplay)
        if (!_cursorAllowed) return;

        bool controllerUsed = IsControllerUsed();
        bool mouseOrKeyboardUsed = IsMouseOrKeyboardUsed();

        if (controllerUsed)
        {
            if (!_forceMouseState)
            {
                showMouse = false;
                UpdateCursorState();
            }
        }
        else if (mouseOrKeyboardUsed)
        {
            if (!_forceMouseState)
            {
                showMouse = true;
                UpdateCursorState();
            }
        }
    }

    private bool IsControllerUsed()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) return false;

        if (gamepad.leftStick.ReadValue().sqrMagnitude > 0.1f) return true;
        if (gamepad.rightStick.ReadValue().sqrMagnitude > 0.1f) return true;

        return gamepad.allControls.Any(c => c is ButtonControl && ((ButtonControl)c).wasPressedThisFrame);
    }

    private bool IsMouseOrKeyboardUsed()
    {
        var mouse = Mouse.current;
        if (mouse != null)
        {
            if (mouse.delta.ReadValue().sqrMagnitude > 0.01f) return true;
            if (mouse.leftButton.wasPressedThisFrame ||
                mouse.rightButton.wasPressedThisFrame ||
                mouse.middleButton.wasPressedThisFrame)
                return true;
        }

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            return keyboard.allControls.Any(c => c is KeyControl && ((KeyControl)c).wasPressedThisFrame);
        }

        return false;
    }

    // ----- Lifecycle -----

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Init()
    {
        _interactives = new List<Interactive>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (_playerInput == null)
            _playerInput = FindFirstObjectByType<PlayerInput>();
        if (_playerInput != null)
            _playerInput.actions?.Enable();
        UpdateCursorState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"OnSceneLoaded: {scene.name}");

        if (_interactives != null)
        {
            Debug.Log($"Clearing {_interactives.Count} old interactives from previous scene");
            _interactives.Clear();
        }

        FindSceneReferences();
        _dependenciesProcessed = false;

        // Apply cursor state for the new scene
        UpdateCursorState();

        Debug.Log($"Interactives count after clearing: {_interactives.Count}");
    }

    public void RegisterInteractive(Interactive interactive)
    {
        _interactives.Add(interactive);
    }

    private void FindSceneReferences()
    {
        if (_playerMovement == null)
            _playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (_playerInventory == null)
            _playerInventory = _playerMovement?.GetComponent<PlayerInventory>();
        if (_cameraFocusController == null)
            _cameraFocusController = _playerMovement?.GetComponentInChildren<CameraFocusController>();
        if (_pauseMenu == null)
            _pauseMenu = FindFirstObjectByType<Pause_Menu>();
    }

    void Start()
    {
        Debug.Log("InteractionManager Start called.");
        FindSceneReferences();
    }

    void LateUpdate()
    {
        if (!_dependenciesProcessed)
        {
            Debug.Log("Processing dependencies in LateUpdate.");
            ProcessDependencies();
            _dependenciesProcessed = true;
        }
    }

    private void ProcessDependencies()
    {
        if (_interactives == null)
        {
            Debug.LogError("_interactives is NULL at start of ProcessDependencies!");
            return;
        }

        Debug.Log($"Processing dependencies. _interactives count: {_interactives.Count}");

        for (int i = 0; i < _interactives.Count; i++)
        {
            Interactive interactive = _interactives[i];

            if (interactive == null) continue;
            if (interactive.interactiveData == null) continue;
            if (interactive.interactiveData.requirements == null) continue;

            foreach (InteractiveData requirementData in interactive.interactiveData.requirements)
            {
                if (requirementData == null) continue;
                Interactive requirement = FindInteractive(requirementData);
                if (requirement != null)
                {
                    interactive.AddRequirement(requirement);
                    requirement.AddDependent(interactive);
                }
            }
        }
    }

    public Interactive FindInteractive(InteractiveData interactiveData)
    {
        foreach (Interactive interactive in _interactives)
            if (interactive.interactiveData == interactiveData)
                return interactive;
        return null;
    }

    public string GetPickMessage(string objectName)
    {
        return _interactPrefix + " " + _pickPrefix + " " + objectName;
    }

    public string GetInteractionMessage(string message)
    {
        return _interactPrefix + " " + message;
    }

    void OnDestroy()
    {
        if (_instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}