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
        _interactives = null;
    }
    private void Update()
    {
        if (_isInspecting)
            HandleInspectionInput();
    }

    private void ProcessDependencies()
    {
        foreach (Interactive interactive in _interactives)
        {
            foreach (InteractiveData requirementData in interactive.interactiveData.requirements)
            {
                Interactive requirement = FindInteractive(requirementData);
                interactive.AddRequirement(requirement);
                requirement.AddDependent(interactive);
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
    if (_isInspecting) return;
    _currentInspect = item;

    Camera mainCamera = Camera.main;
    Vector3 inspectPosition = mainCamera.transform.position + mainCamera.transform.forward * 2f;


    _isInspecting = true;
    _spawnedInspectObject = Instantiate(item.gameObject, inspectPosition, Quaternion.identity);
    _spawnedInspectObject.SetActive(true);
    Interactive interactiveComponent = _spawnedInspectObject.GetComponent<Interactive>();
    if (interactiveComponent != null)
    {
        interactiveComponent.enabled = false;
    }

        Collider[] colliders = _spawnedInspectObject.GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }
        EnablePlayerControls(false);
    }

    private void HandleInspectionInput()
    {
    if (!_isInspecting) return;

    if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Escape))
    {
        EndInspection();
        return;
    }

    if (Input.GetMouseButton(0))
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Multiply by Time.deltaTime for frame-rate independent rotation
        float mouseX = Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _rotationSpeed * Time.deltaTime;
        
        RotateInspectionObject(mouseX, mouseY);
    }
    else
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    }

    private void RotateInspectionObject(float xRotation, float yRotation)
    {
    if (_spawnedInspectObject != null)
    {
        Debug.Log($"Rotating: X={xRotation}, Y={yRotation}");
        
        // Create rotation based on mouse movement
        Vector3 rotation = new Vector3(yRotation, -xRotation, 0);
        _spawnedInspectObject.transform.Rotate(rotation, Space.World);
    }
    }

    public void EndInspection()
    {
    if (_spawnedInspectObject != null)
        {
            Interactive inspectedInteractive = _spawnedInspectObject.GetComponent<Interactive>();

            if (inspectedInteractive != null && _currentInspect != null)
            {
                _playerInventory.Remove(_currentInspect);

                _playerInventory.Add(inspectedInteractive);

            }

            Destroy(_spawnedInspectObject);
            _spawnedInspectObject = null;
            EnablePlayerControls(true);
        }

        _isInspecting = false;
        _currentInspect = null;
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
