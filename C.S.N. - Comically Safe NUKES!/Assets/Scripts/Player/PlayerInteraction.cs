using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private float _maxInteractionDistance;
    [SerializeField] private InspectionTool inspectionTool;

    private PlayerInventory _playerInventory;
    private Transform _cameraTransform;
    private Interactive _currentInteractive;
    private bool _refreshCurrentInteractive;

    private InputSystem_Actions _inputActions;
    private InputAction _inspectAction;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();

        _inputActions.Player.Enable();

        _inspectAction = _inputActions.Player.Inspect;
        _inspectAction.performed += OnInspect;

        _inputActions.Player.Interact.performed += OnInteract;

        Debug.Log("Inspect action enabled: " + _inspectAction.enabled);
        Debug.Log("Inspect action: " + _inspectAction);
    }

    private void Start()
    {
        _cameraTransform = GetComponentInChildren<Camera>().transform;
        _currentInteractive = null;
        _refreshCurrentInteractive = false;
        _playerInventory = GetComponent<PlayerInventory>();
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inspectAction.performed -= OnInspect;
            _inputActions.Player.Interact.performed -= OnInteract;
            _inputActions.Disable();
        }
    }

    private void Update()
    {
        if (!inspectionTool.isInspecting)
        {
            UpdateCurrentInteractive();
        }
        else
        {
            if (_currentInteractive != null)
                ClearCurrentInteractive();
        }

        if (_inspectAction != null && _inspectAction.WasPressedThisFrame())
        {
            Interactive selectedItem = _playerInventory.GetSelected();
            if (selectedItem != null)
                inspectionTool.StartInspection(selectedItem);
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (inspectionTool.isInspecting) return;
        if (_currentInteractive != null)
        {
            _currentInteractive.Interact();
            _refreshCurrentInteractive = true;
        }
    }

    private void OnInspect(InputAction.CallbackContext context)
    {
        Debug.Log("OnInspect performed callback triggered");
        if (inspectionTool.isInspecting) return;

        Interactive selectedItem = _playerInventory.GetSelected();
        if (selectedItem != null)
        {
            inspectionTool.StartInspection(selectedItem);
        }
        else
        {
            Debug.LogWarning("No item selected in inventory");
        }
    }

    private void UpdateCurrentInteractive()
    {
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward,
            out RaycastHit hitInfo, _maxInteractionDistance))
            CheckObjectForInteraction(hitInfo.collider);
        else if (_currentInteractive != null)
            ClearCurrentInteractive();
    }

    private void CheckObjectForInteraction(Collider collider)
    {
        Interactive interactive = collider.GetComponent<Interactive>();

        if (interactive == null || !interactive.isOn)
        {
            if (_currentInteractive != null)
                ClearCurrentInteractive();
        }
        else if (interactive != _currentInteractive || _refreshCurrentInteractive)
            SetCurrentInteractive(interactive);
    }

    private void ClearCurrentInteractive()
    {
        _currentInteractive = null;
        _uiManager.ShowDefaultCrosshair();
        _uiManager.HideInteractionPanel();
    }

    private void SetCurrentInteractive(Interactive interactive)
    {
        _currentInteractive = interactive;
        _refreshCurrentInteractive = false;

        (bool hasCorrectItem, InteractiveData.Type interactionType) = interactive.GetInteractionMessage();

        if (!hasCorrectItem)
        {
            _uiManager.ShowDefaultCrosshair();
            _uiManager.HideInteractionPanel();
            return;
        }

        switch (interactionType)
        {
            case InteractiveData.Type.Pickable:
                _uiManager.ShowPickupInteractionCrosshair();
                break;

            case InteractiveData.Type.Focusable:
                _uiManager.ShowFocusedInteractionCrosshair();
                break;

            case InteractiveData.Type.Indirect:
                _uiManager.ShowIndirectInteractionCrosshair();
                _uiManager.HideInteractionPanel();
                break;

            default:
                _uiManager.ShowInteractionCrosshair();
                break;
        }
    }

    public void RefreshCurrentInteractive()
    {
        _refreshCurrentInteractive = true;
    }
}