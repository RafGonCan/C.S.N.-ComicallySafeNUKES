using System.Collections.Generic;
using UnityEngine;

public class Interactive : MonoBehaviour
{
    [SerializeField] private InteractiveData _interactiveData;

    private InteractionManager  _interactionManager;
    private PlayerInventory     _playerInventory;
    private List<Interactive>   _requirements;
    private List<Interactive>   _dependents;
    private Animator            _animator;
    private bool                _requirementsMet;
    private int                 _interactionCount;

    public bool             isOn;
    public InteractiveData  interactiveData => _interactiveData;
    public string           inventoryName   => _interactiveData.inventoryName;
    public Sprite           inventoryIcon   => _interactiveData.inventoryIcon;

    void Awake()
    {
        _interactionManager = InteractionManager.instance;
        _playerInventory    = _interactionManager.playerInventory;
        _requirements       = new List<Interactive>();
        _dependents         = new List<Interactive>();
        _animator           = GetComponent<Animator>();
        _requirementsMet    = _interactiveData.requirements.Length == 0;
        _interactionCount   = 0;
        isOn                = _interactiveData.startsOn;

        _interactionManager.RegisterInteractive(this);
    }

    public void AddRequirement(Interactive requirement)
    {
        _requirements.Add(requirement);
    }

    public void AddDependent(Interactive dependent)
    {
        _dependents.Add(dependent);
    }

    private bool IsType(InteractiveData.Type type)
    {
        return _interactiveData.type == type;
    }

    public string GetInteractionMessage()
    {
        if (IsType(InteractiveData.Type.Pickable) && !_playerInventory.Contains(this) && _requirementsMet)
            return _interactionManager.GetPickMessage(_interactiveData.inventoryName);
        else if (!_requirementsMet)
        {
            if (PlayerHasRequirementSelected())
                return _playerInventory.GetSelectedInteractionMessage();
            else
                return _interactiveData.requirementsMessage;
        }
        else if (interactiveData.interactionMessages.Length > 0)
            return _interactionManager.GetInteractionMessage(interactiveData.interactionMessages[_interactionCount % _interactiveData.interactionMessages.Length]);
        else
            return null;
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
            InteractSelf(true);
        else if (PlayerHasRequirementSelected())
            UseRequirementFromInventory();
    }

    private void InteractSelf(bool direct)
    {
        if (direct && IsType(InteractiveData.Type.Indirect))
            return;
        else if (IsType(InteractiveData.Type.Pickable) && !_playerInventory.IsFull())
            PickUpInteractive();
        else if (IsType(InteractiveData.Type.InteractOnce) || IsType(InteractiveData.Type.InteractMulti))
            DoDirectInteraction();
        else if (IsType(InteractiveData.Type.Indirect))
            PlayAnimation(_interactionManager.interactAnimationName);
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
}
