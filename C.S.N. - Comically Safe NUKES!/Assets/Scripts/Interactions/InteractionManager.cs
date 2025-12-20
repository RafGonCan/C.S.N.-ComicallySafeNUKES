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
    [SerializeField] private CameraFocusController _cameraFocusController;
    private PlayerInventory     _playerInventory;
    private PlayerMovement      _playerMovement;
    private Pause_Menu          _pauseMenu;
    private List<Interactive>   _interactives;
    public CameraFocusController CameraFocusController => _cameraFocusController;
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
            _playerInventory = _playerMovement?.GetComponent<PlayerInventory>();
        } 
        if (_pauseMenu == null)
        {
            _pauseMenu = FindFirstObjectByType<Pause_Menu>();
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
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
