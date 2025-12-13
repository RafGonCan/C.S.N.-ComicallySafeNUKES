using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    private static InteractionManager _instance;

    public static InteractionManager instance
    {
        get
        {
            if (_instance == null)
                FindFirstObjectByType<InteractionManager>().Init();

            return _instance;
        }
    }

    [SerializeField] private PlayerInventory    _playerInventory;
    [SerializeField] private string             _interactPrefix;
    [SerializeField] private string             _pickPrefix;
    [SerializeField] private string             _awakeAnimationName;
    [SerializeField] private string             _interactAnimationName;

    [SerializeField] private PlayerMovement _playerMovement;
    private Interactive       _currentInspect;
    private GameObject _spawnedInspectObject;
    private Interactive _originalInspectedItem;
    private GameObject _currentInspectionModel;
    private StatefulInteractive _currentStatefulInspection;

    private float             _rotationSpeed = 100f;
    private Vector3           _lastMousePosition;

    private List<Interactive> _interactives;
    public bool               _isInspecting = false;
    public bool IsInspecting => _isInspecting;

    public PlayerInventory    playerInventory         => _playerInventory;
    public string             awakeAnimationName      => _awakeAnimationName;
    public string             interactAnimationName   => _interactAnimationName;


    void Awake()
    {
        if (_instance == null)
            Init();
        else if (_instance != this)
            Destroy(gameObject);
    }
    
    private void Init()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _interactives = new List<Interactive>();
    }

    public void RegisterInteractive(Interactive interactive)
    {
        _interactives.Add(interactive);
    }

    void Start()
    {
        ProcessDependencies();
    }
    private void Update()
    {
        if (_isInspecting)
        {
            HandleInspectionInput();
        

            if (Input.GetMouseButtonDown(0) && _currentStatefulInspection != null)
            {
                CheckForPartInteraction();
            }
        }
        else
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
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
        
            if (interactive == null)
            {
                Debug.LogWarning($"Interactive at index {i} is null");
                continue;
            }
        
            if (interactive.interactiveData == null)
            {
                Debug.LogWarning($"Interactive '{interactive.name}' has no InteractiveData assigned");
                continue;
            }
        
            if (interactive.interactiveData.requirements == null)
            {
                Debug.LogWarning($"Interactive '{interactive.name}' has null requirements list");
                continue;
            }
        
            foreach (InteractiveData requirementData in interactive.interactiveData.requirements)
            {
                if (requirementData == null)
                {
                    Debug.LogWarning($"Interactive '{interactive.name}' has a null requirement in its list");
                    continue;
                }
            
                Interactive requirement = FindInteractive(requirementData);
                if (requirement != null)
                {
                    interactive.AddRequirement(requirement);
                    requirement.AddDependent(interactive);
                }
                else
                {
                    Debug.LogWarning($"Could not find Interactive for requirement: {requirementData.name}");
                }
            }
        }
    }
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
    public void StartInspection(Interactive item)
    {
        if (_isInspecting || item == null) return;
    
        _currentInspect = item;
    
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;
    
        _currentInspectionModel = item.CreateInspectionModel();
        if (_currentInspectionModel == null)
        {
        Debug.LogWarning("Failed to create inspection model for: " + item.name);
        return;
        }

        Vector3 inspectPosition = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;

        _isInspecting = true;
    
        _currentInspectionModel.transform.position = inspectPosition;
        _currentInspectionModel.transform.localScale = Vector3.one * 0.5f;
        _currentInspectionModel.transform.LookAt(mainCamera.transform);
        _currentInspectionModel.transform.Rotate(0, 180, 0);
    
        _currentStatefulInspection = _currentInspectionModel.GetComponent<StatefulInteractive>();
        if (_currentStatefulInspection != null)
        {
            _currentStatefulInspection.SetupForInspection();
        }
        EnablePlayerControls(false);
    }

    private void HandleInspectionInput()
    {
        if (!_isInspecting) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            EndInspection();
            return;
        }
        
        if (_currentStatefulInspection != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * _rotationSpeed * Time.deltaTime;
                
                if (_currentInspectionModel != null)
                {
                    _currentInspectionModel.transform.Rotate(Vector3.up, -mouseX, Space.World);
                    _currentInspectionModel.transform.Rotate(Vector3.right, mouseY, Space.World);
                }
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * _rotationSpeed * Time.deltaTime;
                
                if (_currentInspectionModel != null)
                {
                    _currentInspectionModel.transform.Rotate(Vector3.up, -mouseX, Space.World);
                    _currentInspectionModel.transform.Rotate(Vector3.right, mouseY, Space.World);
                }
            }
        }
    }

    public void EndInspection()
    {
        if (_currentInspect != null && _currentInspectionModel != null)
        {
            _currentInspect.UpdateFromInspectionModel(_currentInspectionModel);
        
            if (_currentStatefulInspection != null)
            {
            _currentStatefulInspection.CleanupAfterInspection();
            }
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

        _isInspecting = false;
        _currentInspect = null;
        _currentStatefulInspection = null;
        _originalInspectedItem = null;
    
        EnablePlayerControls(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void EnablePlayerControls(bool enable)
    {
        PlayerMovement playerMovement = _playerInventory.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetControlsEnabled(enable);
        }
    }
}
