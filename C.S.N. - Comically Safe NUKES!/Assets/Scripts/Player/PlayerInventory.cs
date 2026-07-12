using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private AudioClip _pickUp;
    private AudioSource         _audioSource;
    private PlayerInteraction   _playerInteraction;
    private List<Interactive>   _inventory;
    private int                 _selectedSlotIndex;
    private bool                firstTimeAddingItem = true;
    private InputSystem_Actions _inputActions;
    private InputAction _previousAction;
    private InputAction _nextAction;

    void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _audioSource = GetComponentInChildren<AudioSource>();
        _inputActions.Enable();

        _previousAction = _inputActions.Player.Previous;
        _nextAction = _inputActions.Player.Next;

        _previousAction.performed += OnPrevious;
        _nextAction.performed += OnNext;
    }

    void Start()
    {
        _playerInteraction  = GetComponent<PlayerInteraction>();
        _inventory          = new List<Interactive>();
        _selectedSlotIndex  = -1;
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _previousAction.performed -= OnPrevious;
            _nextAction.performed -= OnNext;
            _inputActions.Disable();
        }
    }

    private void OnPrevious(InputAction.CallbackContext context)
    {
        if (_inventory.Count == 0) return;
        int newIndex = _selectedSlotIndex - 1;
        if (newIndex < 0) newIndex = _inventory.Count - 1;
        SelectInventorySlot(newIndex);
    }

    private void OnNext(InputAction.CallbackContext context)
    {
        if (_inventory.Count == 0) return;
        int newIndex = _selectedSlotIndex + 1;
        if (newIndex >= _inventory.Count) newIndex = 0;
        SelectInventorySlot(newIndex);
    }

    public void Add(Interactive item)
    {
        if (firstTimeAddingItem)
        {
            _uiManager.TriggerInspectionReminder();
            firstTimeAddingItem = false;
        }
        _inventory.Add(item);
        _audioSource.pitch = Random.Range(0.75f, 1.5f);
        _audioSource.PlayOneShot(_pickUp);
        

        _uiManager.ShowInventoryIcon(_inventory.Count - 1, item.inventoryIcon);

        if (_selectedSlotIndex == -1)
            SelectInventorySlot(0);
    }

    public void Remove(Interactive item)
    {
        _inventory.Remove(item);

        _uiManager.HideInventoryIcons();

        for (int i = 0; i < _inventory.Count; ++i)
            _uiManager.ShowInventoryIcon(i, _inventory[i].inventoryIcon);

        if (_selectedSlotIndex == _inventory.Count)
            SelectInventorySlot(_selectedSlotIndex - 1);
    }

    public bool Contains(Interactive item)
    {
        return _inventory.Contains(item);
    }

    public bool IsFull()
    {
        return _inventory.Count == _uiManager.GetInventorySlotCount();
    }

    private void SelectInventorySlot(int index)
    {
        _selectedSlotIndex = index;
        _uiManager.SelectInventorySlot(index);
        _playerInteraction.RefreshCurrentInteractive();
    }

    public bool IsSelected(Interactive item)
    {
        return GetSelected() == item;
    }

    public Interactive GetSelected()
    {
        return _selectedSlotIndex != -1 ? _inventory[_selectedSlotIndex] : null;
    }

    void Update()
    {
        for (int i = 0; i < _inventory.Count && i < 9; ++i)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i != _selectedSlotIndex)
                SelectInventorySlot(i);
    }
}