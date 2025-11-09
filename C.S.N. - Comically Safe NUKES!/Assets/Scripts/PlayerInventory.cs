using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private UIManager _uiManager;

    private PlayerInteraction   _playerInteraction;
    private List<Interactive>   _inventory;
    private int                 _selectedSlotIndex;

    void Start()
    {
        _playerInteraction  = GetComponent<PlayerInteraction>();
        _inventory          = new List<Interactive>();
        _selectedSlotIndex  = -1;
    }

    public void Add(Interactive item)
    {
        _inventory.Add(item);

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

    public string GetSelectedInteractionMessage()
    {
        return _inventory[_selectedSlotIndex].GetInteractionMessage();
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
        CheckForPlayerSlotSelection();
    }

    private void CheckForPlayerSlotSelection()
    {
        for (int i = 0; i < _inventory.Count; ++i)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i) && i != _selectedSlotIndex)
                SelectInventorySlot(i);
    }
}
