using UnityEngine;

public class SequenceButton : Interactive
{
    [SerializeField] private TableController tableController;
    [SerializeField] private int buttonIndex;
    private bool _isPressed;
    /// <summary>
    /// Overrides the InteractSelf function by adding a checker feature for the buttons.
    /// Checking if they available to be pressed.
    /// </summary>
    /// <param name="direct"></param>
    protected override void InteractSelf(bool direct)
    {
        // Checks if the button is able to be pressed
        if (_isPressed || (tableController != null && !tableController.CanPressButton(buttonIndex)))
        {
            return;
        }

        // If the button can be pressed, it will call the base function    
        base.InteractSelf(direct);
        
        // Then it records it
        _isPressed = true;
        if (tableController != null)
        {
            tableController.OnButtonPressed(buttonIndex);
        }
    }
    /// <summary>
    /// Simple button reset function
    /// </summary>
    public void ResetButton()
    {
        _isPressed = false;
    }
}