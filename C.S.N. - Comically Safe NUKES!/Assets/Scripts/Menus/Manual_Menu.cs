using UnityEngine;
using UnityEngine.InputSystem;

public class Manual_Menu : MonoBehaviour
{
    [SerializeField] private GameObject manualMenu;
    private bool _isManualOpen = false;

    private InputSystem_Actions _inputActions;

    private void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Enable();
    }

    private void Update()
    {
        if (_inputActions.Player.Help.WasPressedThisFrame())
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        if (_isManualOpen)
            Close();
        else
            Open();
    }

    public void Open()
    {
        _isManualOpen = true;
        manualMenu.SetActive(true);
    }

    public void Close()
    {
        _isManualOpen = false;
        manualMenu.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Disable();
        }
    }
}