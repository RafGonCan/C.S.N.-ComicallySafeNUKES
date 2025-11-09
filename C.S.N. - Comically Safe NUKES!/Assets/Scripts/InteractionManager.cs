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

    private List<Interactive> _interactives;

    public PlayerInventory  playerInventory         => _playerInventory;
    public string           awakeAnimationName      => _awakeAnimationName;
    public string           interactAnimationName   => _interactAnimationName;

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
}
