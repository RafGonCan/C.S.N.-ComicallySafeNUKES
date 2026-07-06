using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactive : MonoBehaviour
{
    [SerializeField] private InteractiveData _interactiveData;
    [SerializeField] private StatefulInteractive statefulPrefab;
    [SerializeField] private Transform _focusPoint;
    
    [SerializeField] private AudioClip _requirementMetSound;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private bool _playSoundOnRequirementsMet = true;
    [SerializeField] private AudioClip _fallbackSound;
    public UnityEvent<Interactive> onRequirementUsed;
    public UnityEvent onFallbackInteract;
    public UnityEvent onDirectInteract;
    [SerializeField] private bool _allowCancelExit = true;

    private Collider _collider;
    private InteractionManager      _interactionManager;
    private PlayerInventory         _playerInventory;
    private List<Interactive>       _requirements;
    private List<Interactive>       _dependents;
    private Animator                _animator;
    private bool                    _requirementsMet;
    private bool                    _wasRequirementsMetLastFrame;
    private int                     _interactionCount;
    private StatefulInteractive    _statefulInstance;
    public bool                     isOn;
    public InteractiveData          interactiveData => _interactiveData;
    public string                   inventoryName   => _interactiveData?.inventoryName;
    public Sprite                   inventoryIcon   => _interactiveData.inventoryIcon;
    public StatefulInteractive      CurrentStatefulItem => _statefulInstance;
    public bool                     AreRequirementsMet => _requirementsMet;

    void Awake()
    {
        _requirements       = new List<Interactive>();
        _dependents         = new List<Interactive>();
        _animator           = GetComponent<Animator>();
        _interactionCount   = 0;
        _wasRequirementsMetLastFrame = false;
        
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        

        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 1.0f;

        if (_interactiveData != null)
        {
            _requirementsMet = _interactiveData.requirements.Length == 0;
            isOn = _interactiveData.startsOn;
        }

        if (statefulPrefab != null)
        {
            _statefulInstance = Instantiate(statefulPrefab, transform);
            _statefulInstance.transform.localPosition = Vector3.zero;
            _statefulInstance.transform.localRotation = Quaternion.identity;
            _statefulInstance.gameObject.SetActive(false);
        }
    }
    
    void Start()
    {
        Initialize();
        _wasRequirementsMetLastFrame = _requirementsMet;
    }
    
    private void Initialize()
    {
        _interactionManager = InteractionManager.instance;
        if (_interactionManager == null)
        {
            Debug.LogWarning($"InteractionManager not found for {name}");
            return;
        }
        
        _playerInventory = _interactionManager.playerInventory;
        
        _interactionManager.RegisterInteractive(this);
    }

    void FixedUpdate()
    {
        if (_requirementsMet && !_wasRequirementsMetLastFrame && _playSoundOnRequirementsMet)
        {
            PlayRequirementMetSound();
        }
        
        _wasRequirementsMetLastFrame = _requirementsMet;
    }

    private void PlayRequirementMetSound()
    {
        if (_audioSource != null && _requirementMetSound != null)
        {
            _audioSource.clip = _requirementMetSound;
            _audioSource.Play();
            Debug.Log($"Played requirement met sound on {gameObject.name}");
        }
    }

    public StatefulInteractive GetStatefulItem()
    {
        return _statefulInstance;
    }

    public void AddRequirement(Interactive requirement)
    {
        _requirements.Add(requirement);
    }

    public void AddDependent(Interactive dependent)
    {
        _dependents.Add(dependent);
    }
    
    public GameObject CreateInspectionModel()
    {
        if (_statefulInstance != null)
        {
            GameObject inspectionModel = Instantiate(_statefulInstance.gameObject);
            

            StatefulInteractive stateful = inspectionModel.GetComponent<StatefulInteractive>();
            if (stateful != null)
            {
                StatefulInteractive.ItemState currentState = _statefulInstance.GetCurrentState();
                stateful.ApplyState(currentState);
                stateful.SetupForInspection();
            }
            
            inspectionModel.SetActive(true);
            return inspectionModel;
        }
        else if (_interactiveData.inspectModel != null && _interactiveData.inspectModel != null)
        {
            GameObject model = Instantiate(_interactiveData.inspectModel);
            return model;
        }
        
        return null;
    }
    
    public void UpdateFromInspectionModel(GameObject inspectionModel)
    {
        if (_statefulInstance != null && inspectionModel != null)
        {
            StatefulInteractive inspectedStateful = inspectionModel.GetComponent<StatefulInteractive>();
            if (inspectedStateful != null)
            {
                StatefulInteractive.ItemState newState = inspectedStateful.GetCurrentState();
                _statefulInstance.ApplyState(newState);
                
                Debug.Log("Item state updated from inspection");
            }
        }
    }

    protected bool IsType(InteractiveData.Type type)
    {
        return _interactiveData != null && _interactiveData.type == type;
    }

    public (bool, InteractiveData.Type) GetInteractionMessage()
    {
        if (IsType(InteractiveData.Type.InteractOnce) || IsType(InteractiveData.Type.InteractMulti))
            return (true, InteractiveData.Type.InteractMulti);

        if (IsType(InteractiveData.Type.Indirect))
            return (true, InteractiveData.Type.Indirect);

        if (IsType(InteractiveData.Type.Pickable) && !_playerInventory.Contains(this) && _requirementsMet)
            return (true, InteractiveData.Type.Pickable);

        if (IsType(InteractiveData.Type.Focusable) && _requirementsMet)
            return (true, InteractiveData.Type.Focusable);

        if (!_requirementsMet && PlayerHasRequirementSelected())
            return (true, InteractiveData.Type.InteractMulti);

        return (false, InteractiveData.Type.None);
    }

    private bool PlayerHasRequirementSelected()
    {
        foreach (Interactive requirement in _requirements)
            if (_playerInventory.IsSelected(requirement))
                return true;

        return false;
    }

    public void Interact()
    {
        if (_requirementsMet)
        {
            InteractSelf(true);
        }            
        else if (PlayerHasRequirementSelected())
            UseRequirementFromInventory();
        else
        {
            InteractWithoutRequirements();
        }
    }
    protected virtual void InteractWithoutRequirements()
    {
        onFallbackInteract?.Invoke();

        if (_fallbackSound != null)
            PlayCustomSound(_fallbackSound);

        if (!string.IsNullOrEmpty(_interactionManager?.fallbackAnimationName) && _animator != null)
            _animator.SetTrigger(_interactionManager?.fallbackAnimationName);
        else
        return;
    }

    protected virtual void InteractSelf(bool direct)
    {
        onDirectInteract?.Invoke();

        if (direct && IsType(InteractiveData.Type.Indirect))
            return;
        else if (IsType(InteractiveData.Type.Pickable) && !_playerInventory.IsFull())
            PickUpInteractive();
        else if (IsType(InteractiveData.Type.InteractOnce) || IsType(InteractiveData.Type.InteractMulti))
            DoDirectInteraction();
        else if (IsType(InteractiveData.Type.Indirect))
            PlayAnimation(_interactionManager.interactAnimationName);
        else if (IsType(InteractiveData.Type.Focusable))
        {
            TriggerCameraFocus();
            Debug.Log("Interagi com um focusable");
        }
    }

    private void PickUpInteractive()
    {
        _playerInventory.Add(this);
        gameObject.SetActive(false);
    }

    private void DoDirectInteraction()
    {
        ++_interactionCount;

        if (IsType(InteractiveData.Type.InteractOnce))
            isOn = false;

        CheckDependentsRequirements();
        DoIndirectInteractions();

        PlayAnimation(_interactionManager.interactAnimationName);
    }

    private void CheckDependentsRequirements()
    {
        foreach (Interactive dependent in _dependents)
            dependent.CheckRequirements();
    }

    private void CheckRequirements()
    {
        bool wasMet = _requirementsMet;
        
        foreach (Interactive requirement in _requirements)
        {
            if (!requirement._requirementsMet || 
               (!requirement.IsType(InteractiveData.Type.Indirect) && requirement._interactionCount == 0))
               {
                    _requirementsMet = false;
                    return;
               }
        }

        _requirementsMet = true;
        
        if (!wasMet && _requirementsMet)
        {
            PlayAnimation(_interactionManager.awakeAnimationName);
            
            if (_playSoundOnRequirementsMet)
            {
                PlayRequirementMetSound();
            }
        }

        CheckDependentsRequirements();
    }

    private void DoIndirectInteractions()
    {
        foreach (Interactive dependent in _dependents)
            if (dependent.IsType(InteractiveData.Type.Indirect) && dependent._requirementsMet)
                dependent.InteractSelf(false);
    }
 
    private void PlayAnimation(string animation)
    {
        if (_animator != null)
        {
            gameObject.SetActive(true);
            _animator.SetTrigger(animation);
        }
    }

    private void UseRequirementFromInventory()
    {
        Interactive requirement = _playerInventory.GetSelected();
        _playerInventory.Remove(requirement);
        ++requirement._interactionCount;

        onRequirementUsed?.Invoke(requirement);

        requirement.PlayAnimation(_interactionManager.interactAnimationName);
        CheckRequirements();
    }
    
    public void ForceCheckRequirements()
    {
        CheckRequirements();
    }
    
    public void SetRequirementsMet(bool met)
    {
        bool wasMet = _requirementsMet;
        _requirementsMet = met;
        
        if (met && !wasMet && _playSoundOnRequirementsMet)
        {
            PlayRequirementMetSound();
            PlayAnimation(_interactionManager?.awakeAnimationName);
        }
    }

    private Collider GetCollider()
    {
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
            if (_collider == null)
            {
                _collider = GetComponentInChildren<Collider>();
            }
        }
        return _collider;
    }

    private void TriggerCameraFocus()
    {
        if (_focusPoint != null)
        {
            if (!InteractionManager.instance.CameraFocusController.GetFocusing())
            {
                InteractionManager.instance.CameraFocusController.EnterFocus(_focusPoint, this, _allowCancelExit);
                Collider col = GetCollider();
                if (col != null) col.enabled = false;
            }
            else
            {
                InteractionManager.instance.CameraFocusController.ExitFocus();
            }
        }
    }

    public void RestoreCollider()
    {
        Collider col = GetCollider();
        if (col != null) col.enabled = true;
    }

    public void SetRequirementMetSound(AudioClip clip)
    {
        _requirementMetSound = clip;
    }
    
    public void PlayCustomSound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
}