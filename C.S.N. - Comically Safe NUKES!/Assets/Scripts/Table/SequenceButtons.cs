using UnityEngine;

public class SequenceButton : Interactive
{
    [SerializeField] private TableController tableController;
    [SerializeField] private int buttonIndex;
    private bool _isPressed;

    protected override void InteractSelf(bool direct)
    {
        // Verifica se dá para clicar
        if (_isPressed || (tableController != null && !tableController.CanPressButton(buttonIndex)))
        {
            return;
        }
    
        base.InteractSelf(direct);
        
        // Clica e notifica
        _isPressed = true;
        if (tableController != null)
        {
            tableController.OnButtonPressed(buttonIndex);
        }
    }
    
    public void ResetButton()
    {
        _isPressed = false;
    }
}