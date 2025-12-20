using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private UIManager  _uiManager;
    [SerializeField] private float      _maxInteractionDistance;
    [SerializeField] private InspectionTool inspectionTool;
    private PlayerInventory _playerInventory;

    private Transform   _cameraTransform;
    private Interactive _currentInteractive;
    private bool        _refreshCurrentInteractive;

    void Start()
    {
        _cameraTransform            = GetComponentInChildren<Camera>().transform;
        _currentInteractive         = null;
        _refreshCurrentInteractive  = false;
        _playerInventory            = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (!inspectionTool.isInspecting)
        {
            UpdateCurrentInteractive();
            CheckForPlayerInteraction();
            CheckForInspection();
        }
        else
        {
            if (_currentInteractive != null)
            ClearCurrentInteractive();
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
        _currentInteractive         = interactive;
        _refreshCurrentInteractive  = false;

        bool hasCorrectItem = interactive.GetInteractionMessage();

        if (hasCorrectItem)
        {
            _uiManager.ShowInteractionCrosshair();
        }
        else
        {
            _uiManager.ShowDefaultCrosshair();
            _uiManager.HideInteractionPanel();
        }
    }

    private void CheckForPlayerInteraction()
    {
        if (Input.GetButtonDown("Interact") && _currentInteractive != null)
        {
            _currentInteractive.Interact();
            _refreshCurrentInteractive = true;
        }
    }
    private void CheckForInspection()
    {
        if(Input.GetKeyDown(KeyCode.E) && inspectionTool.isInspecting == false)
        {
            Interactive selectedItem = _playerInventory.GetSelected();
            if (selectedItem != null)
            {
                inspectionTool.StartInspection(selectedItem);
            }
            else
            return;
        }
    }

    public void RefreshCurrentInteractive()
    {
        _refreshCurrentInteractive = true;
    }
}
