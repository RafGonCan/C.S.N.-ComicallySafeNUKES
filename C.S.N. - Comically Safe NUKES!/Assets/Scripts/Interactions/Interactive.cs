using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Interactive : MonoBehaviour
{
    [SerializeField] private InteractiveData _interactiveData;
    [SerializeField] private StatefulInteractive statefulPrefab;
    [SerializeField] private Transform _focusPoint;
    private InteractionManager      _interactionManager;
    private PlayerInventory         _playerInventory;
    private List<Interactive>       _requirements;
    private List<Interactive>       _dependents;
    private Animator                _animator;
    private bool                    _requirementsMet;
    private int                     _interactionCount;
    private StatefulInteractive    _statefulInstance;
    private bool                    _firstTime;
    public bool                     isOn;
    public InteractiveData          interactiveData => _interactiveData;
    public string                   inventoryName   => _interactiveData?.inventoryName;
    public Sprite                   inventoryIcon   => _interactiveData.inventoryIcon;
    public StatefulInteractive      CurrentStatefulItem => _statefulInstance;
    public bool                     AreRequirementsMet => _requirementsMet;
    public bool                     firstTime => _firstTime;

    void Awake()
    {
        {
            _requirements       = new List<Interactive>();
            _dependents         = new List<Interactive>();
            _animator           = GetComponent<Animator>();
            _interactionCount   = 0;
            
            if (_interactiveData != null)
            {
                _requirementsMet = _interactiveData.requirements.Length == 0;
                isOn = _interactiveData.startsOn;
            }
            else
            {
                Debug.LogError($"Interactive {name} has no InteractiveData assigned!");
                _requirementsMet = true;
                isOn = true;
            }

            if (statefulPrefab != null)
            {
                _statefulInstance = Instantiate(statefulPrefab, transform);
                _statefulInstance.transform.localPosition = Vector3.zero;
                _statefulInstance.transform.localRotation = Quaternion.identity;
                _statefulInstance.gameObject.SetActive(false);
            }
        }
    }
    void Start()
    {
        Initialize();
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

    private bool IsType(InteractiveData.Type type)
    {
        return _interactiveData != null && _interactiveData.type == type;
    }

    public bool GetInteractionMessage()
{
    if (IsType(InteractiveData.Type.Pickable) && !_playerInventory.Contains(this) && _requirementsMet)
        return true;
    else if (IsType(InteractiveData.Type.InteractOnce) && _requirementsMet) 
        return true;
    else if (IsType(InteractiveData.Type.InteractMulti) && _requirementsMet) 
        return true;
    else if (IsType(InteractiveData.Type.Focusable) && _requirementsMet)
        return true;
    else if (!_requirementsMet && PlayerHasRequirementSelected())
        return true;
    
    return false;
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
        if (_requirementsMet && _firstTime)
        {
            InteractSelf(true);
            _firstTime = false;
        }            
        else if (PlayerHasRequirementSelected())
            UseRequirementFromInventory();
    }

    protected virtual void InteractSelf(bool direct)
    {
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
        PlayAnimation(_interactionManager.awakeAnimationName);

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

        requirement.PlayAnimation(_interactionManager.interactAnimationName);

        CheckRequirements();
    }
    public void ForceCheckRequirements()
    {
        CheckRequirements();
    }
     public void SetRequirementsMet(bool met)
    {
        _requirementsMet = met;
        
        if (met)
        {
            PlayAnimation(_interactionManager?.awakeAnimationName);
        }
    }

    private void TriggerCameraFocus()
    {
        if(_focusPoint  != null)
        {
            if (!InteractionManager.instance.CameraFocusController.GetFocusing())
            {
                InteractionManager.instance.CameraFocusController.EnterFocus(_focusPoint);
            }
            else
            {
                InteractionManager.instance.CameraFocusController.ExitFocus();
            }
        }
    }
}
