using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InspectionTool : MonoBehaviour
{
    [SerializeField] private float _rotationSpeedMouse = 100f;
    [SerializeField] private float _rotationSpeedGamepad = 200f;
    [SerializeField] private PlayerInventory _playerInventory;
    [SerializeField] private UIManager _uiManager;
    public static event System.Action<bool, Interactive> OnInspectionStateChanged;

    private PlayerMovement _playerMovement;
    private Camera _mainCamera;
    private InspectionRoomData _inspectionRoom;
    private Coroutine _scaleCoroutine;
    private float _scaleDuration = 0.5f;
    private Interactive _currentInspect;
    private GameObject _spawnedInspectObject;
    private GameObject _currentInspectionModel;
    private StatefulInteractive _currentStatefulInspection;

    private bool _stillInAnimation;
    public bool isInspecting = false;
    public bool IsInspecting => isInspecting;

    private InputSystem_Actions _inputActions;
    private InputAction _lookAction;
    private InputAction _interactAction;
    private InputAction _cancelAction;
    private InputAction _rightMouseAction;
    private Vector2 _rotationInput;
    private bool _isUsingGamepad = false;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();

        _lookAction = _inputActions.Player.Look;
        _interactAction = _inputActions.Player.Interact;
        _cancelAction = _inputActions.UI.Cancel;
        _rightMouseAction = _inputActions.Player.RightMouse;

        _lookAction.performed += OnLook;
        _lookAction.canceled += OnLook;
        _interactAction.performed += OnInteract;
        _cancelAction.performed += OnCancel;
    }

    private void Start()
    {
        _playerMovement = _playerInventory.GetComponent<PlayerMovement>();
        _mainCamera = _playerInventory.GetComponentInChildren<Camera>();
        _inspectionRoom = GetComponent<InspectionRoomData>();
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _lookAction.performed -= OnLook;
            _lookAction.canceled -= OnLook;
            _interactAction.performed -= OnInteract;
            _cancelAction.performed -= OnCancel;
            _inputActions.Disable();
        }
    }

    private void Update()
    {
        if (isInspecting)
        {
            HandleInspectionInput();

            if (Input.GetMouseButtonDown(0) && _currentStatefulInspection != null)
                CheckForPartInteraction();
        }
    }

    // ----- Input Callbacks -----

    private void OnLook(InputAction.CallbackContext context)
    {
        _rotationInput = context.ReadValue<Vector2>();
        if (context.control != null)
            _isUsingGamepad = context.control.device is Gamepad;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (isInspecting && !_stillInAnimation && _currentStatefulInspection != null)
            CheckForPartInteraction();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        if (isInspecting && !_stillInAnimation)
            EndInspection();
    }

    // ----- Part Interaction -----

    private void CheckForPartInteraction()
    {
        if (_currentInspectionModel == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.transform.IsChildOf(_currentInspectionModel.transform))
            {
                GameObject clickedObject = hit.transform.gameObject;

                if (_currentStatefulInspection != null)
                {
                    int partIndex;
                    if (_currentStatefulInspection.IsToggleablePart(clickedObject, out partIndex))
                    {
                        _currentStatefulInspection.TogglePart(partIndex);
                        return;
                    }
                }

                Transform parent = hit.transform.parent;
                while (parent != null && parent != _currentInspectionModel.transform)
                {
                    if (_currentStatefulInspection != null)
                    {
                        int partIndex;
                        if (_currentStatefulInspection.IsToggleablePart(parent.gameObject, out partIndex))
                        {
                            _currentStatefulInspection.TogglePart(partIndex);
                            return;
                        }
                    }
                    parent = parent.parent;
                }
            }
        }
    }

    // ----- Start / End Inspection -----

    public void StartInspection(Interactive item)
    {
        OnInspectionStateChanged?.Invoke(true, item);
        AudioListener.pause = true;

        if (isInspecting || item == null) return;

        _currentInspect = item;

        if (_uiManager != null)
            _uiManager.HideAllCrosshairs();

        _currentInspectionModel = item.CreateInspectionModel();
        if (_currentInspectionModel == null)
        {
            Debug.LogWarning("Failed to create inspection model for: " + item.name);
            return;
        }
        if (_inspectionRoom != null)
        {
            _currentInspectionModel.transform.SetParent(_inspectionRoom.transform);
            _currentInspectionModel.transform.localPosition = _inspectionRoom.ObjectPosition;
            _currentInspectionModel.transform.localEulerAngles = _inspectionRoom.ObjectRotation;
        }
        else
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _currentInspectionModel.transform.position = mainCamera.transform.position +
                    mainCamera.transform.forward * 1.5f;
                _currentInspectionModel.transform.LookAt(mainCamera.transform);
                _currentInspectionModel.transform.Rotate(0, 180, 0);
            }
        }

        isInspecting = true;
        _currentInspectionModel.transform.localScale = Vector3.zero;

        _currentStatefulInspection = _currentInspectionModel.GetComponent<StatefulInteractive>();
        if (_currentStatefulInspection != null)
            _currentStatefulInspection.SetupForInspection();

        SwitchToInspectionCamera(true);

        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleInAnimation());
        _playerMovement.enabled = false;

        // Allow cursor in inspection
        InteractionManager.instance.SetCursorAllowed(true);
    }

    private IEnumerator ScaleInAnimation()
    {
        _stillInAnimation = true;
        if (_currentInspectionModel == null) yield break;

        float elapsedTime = 0f;
        Vector3 targetScale = Vector3.one * 0.5f;

        while (elapsedTime < _scaleDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _scaleDuration);
            t = 1f - Mathf.Pow(1f - t, 3);
            _currentInspectionModel.transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            yield return null;
        }

        _currentInspectionModel.transform.localScale = targetScale;
        _scaleCoroutine = null;
        _stillInAnimation = false;
    }

    private void SwitchToInspectionCamera(bool toInspection)
    {
        if (_inspectionRoom.InspectionCamera == null || _mainCamera == null) return;

        if (toInspection)
        {
            _mainCamera.enabled = false;
            _inspectionRoom.InspectionCamera.enabled = true;
            _inspectionRoom.InspectionCamera.nearClipPlane = 0.1f;
            _inspectionRoom.InspectionCamera.farClipPlane = 10f;
        }
        else
        {
            _mainCamera.enabled = true;
            _inspectionRoom.InspectionCamera.enabled = false;
        }
    }

    private void HandleInspectionInput()
    {
        if (!isInspecting) return;

        bool shouldRotate = false;
        float speed = _rotationSpeedMouse;

        if (_isUsingGamepad)
        {
            if (_rotationInput.sqrMagnitude > 0.01f)
            {
                shouldRotate = true;
                speed = _rotationSpeedGamepad;
            }
        }
        else
        {
            if (Mouse.current != null && Mouse.current.rightButton.isPressed && _rotationInput.sqrMagnitude > 0.01f)
            {
                shouldRotate = true;
                speed = _rotationSpeedMouse;
            }
        }

        if (shouldRotate)
        {
            float mouseX = _rotationInput.x * speed * Time.deltaTime;
            float mouseY = _rotationInput.y * speed * Time.deltaTime;

            if (_currentInspectionModel != null)
            {
                _currentInspectionModel.transform.Rotate(Vector3.up, -mouseX, Space.World);
                _currentInspectionModel.transform.Rotate(Vector3.right, mouseY, Space.World);
            }
        }
    }

    public void EndInspection()
    {
        OnInspectionStateChanged?.Invoke(false, null);

        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = null;
        }
        StartCoroutine(ScaleOutAndEnd());
        AudioListener.pause = false;
    }

    private IEnumerator ScaleOutAndEnd()
    {
        yield return null;

        if (_currentInspectionModel != null)
        {
            float elapsedTime = 0f;
            Vector3 startScale = _currentInspectionModel.transform.localScale;

            while (elapsedTime < _scaleDuration * 0.5f)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / (_scaleDuration * 0.5f));
                t = t * t;
                _currentInspectionModel.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }
        }
        SwitchToInspectionCamera(false);
        CleanupAfterScaleOut();
    }

    private void CleanupAfterScaleOut()
    {
        if (_currentInspect != null && _currentInspectionModel != null)
        {
            _currentInspect.UpdateFromInspectionModel(_currentInspectionModel);

            if (_currentStatefulInspection != null)
                _currentStatefulInspection.CleanupAfterInspection();
        }

        if (_currentInspectionModel != null)
        {
            Destroy(_currentInspectionModel);
            _currentInspectionModel = null;
        }

        if (_spawnedInspectObject != null)
        {
            Destroy(_spawnedInspectObject);
            _spawnedInspectObject = null;
        }

        isInspecting = false;
        _currentInspect = null;
        _currentStatefulInspection = null;

        _playerMovement.enabled = true;

        InteractionManager.instance.SetCursorAllowed(false);
        EventSystem.current?.SetSelectedGameObject(null);
            if (_uiManager != null)
        _uiManager.ShowDefaultCrosshair();
    }
}