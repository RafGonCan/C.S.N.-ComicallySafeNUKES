using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

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


    [SerializeField] private string             _interactPrefix;
    [SerializeField] private string             _pickPrefix;
    [SerializeField] private string             _awakeAnimationName;
    [SerializeField] private string             _interactAnimationName;
    public static event Action<bool, Interactive> OnInspectionStateChanged;
    private InspectionRoomData _inspectionRoom;
    private Camera             _mainCamera;
    private PlayerInventory    _playerInventory;
    private PlayerMovement     _playerMovement;
    private Pause_Menu         _pauseMenu;
    private Coroutine           _scaleCoroutine;
    private float               _scaleDuration = 0.5f;
    private Interactive         _currentInspect;
    private GameObject          _spawnedInspectObject;
    private GameObject          _currentInspectionModel;
    private StatefulInteractive _currentStatefulInspection;
    private float               _rotationSpeed = 100f;
    private List<Interactive>   _interactives;
    public bool                 isInspecting = false;
    public bool                 IsInspecting => isInspecting;
    public PlayerInventory      playerInventory         => _playerInventory;
    public string               awakeAnimationName      => _awakeAnimationName;
    public string               interactAnimationName   => _interactAnimationName;
    private bool                _dependenciesProcessed  = false;
    


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
        
        Debug.Log($"Interactives count after clearing: {_interactives.Count}");
    }

    public void RegisterInteractive(Interactive interactive)
    {
        _interactives.Add(interactive);
    }
    private void FindSceneReferences()
    {
        if (_playerMovement == null)
        {
            _playerMovement = FindFirstObjectByType<PlayerMovement>();
        }
        if (_playerInventory == null)
        {
            _playerInventory = FindFirstObjectByType<PlayerInventory>();
        }
        if (_pauseMenu == null)
        {
            _pauseMenu = FindFirstObjectByType<Pause_Menu>();
        }
        if (_mainCamera == null)
        {
            try
            {
                _mainCamera = _playerMovement.GetComponentInChildren<Camera>();
            }
            catch
            {
                Debug.Log("Main camera not found");
            }           
        }    
        if (_inspectionRoom == null)
        {
            _inspectionRoom = FindFirstObjectByType<InspectionRoomData>();
        }
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
    private void Update()
    {
        if (isInspecting)
        {
            HandleInspectionInput();
        

            if (Input.GetMouseButtonDown(0) && _currentStatefulInspection != null)
            {
                CheckForPartInteraction();
            }
        }
        else if(!isInspecting && _pauseMenu != null && !_pauseMenu.Paused)
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
        OnInspectionStateChanged?.Invoke(true, item);

        if (isInspecting || item == null) return;
    
        _currentInspect = item;
    
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
        {
            _currentStatefulInspection.SetupForInspection();
        }

        SwitchToInspectionCamera(true);
        
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleInAnimation());
        EnablePlayerControls(false);
    }
    private IEnumerator ScaleInAnimation()
    {
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

        if (Input.GetKeyDown(KeyCode.E))
        {
            EndInspection();
            return;
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
            
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * (_rotationSpeed*2) * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * (_rotationSpeed*2) * Time.deltaTime;
                
            if (_currentInspectionModel != null)
            {
                _currentInspectionModel.transform.Rotate(Vector3.up, -mouseX, Space.World);
                _currentInspectionModel.transform.Rotate(Vector3.right, mouseY, Space.Self);
            }
        }
    }

    public void EndInspection()
    {
        OnInspectionStateChanged?.Invoke(false, null);
        SwitchToInspectionCamera(false);
        

        if (_scaleCoroutine != null)
        {
            StopCoroutine(_scaleCoroutine);
            _scaleCoroutine = null;
        }
        
        StartCoroutine(ScaleOutAndEnd());
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
        
        CleanupAfterScaleOut();
    }
    
    private void CleanupAfterScaleOut()
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

        isInspecting = false;
        _currentInspect = null;
        _currentStatefulInspection = null;
        
        EnablePlayerControls(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void EnablePlayerControls(bool enable)
    {
        if (_playerMovement != null)
        {
            _playerMovement.SetControlsEnabled(enable);
        }
        else if (_playerInventory != null)
        {
            PlayerMovement playerMovement = _playerInventory.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetControlsEnabled(enable);
            }
        }
    }
    public Interactive GetCurrentInspectedItem()
    {
        return _currentInspect;
    }
    void OnDestroy()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
