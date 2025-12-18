using UnityEngine;

public class SafeButton : Interactive
{
    [SerializeField] private SafeController safeController;
    [SerializeField] private int digitIndex;
    [SerializeField] private bool isIncreaseButton;
    /// <summary>
    /// Overrides the InteractSelf button to add a function to also increase/decrease the value.
    /// </summary>
    /// <param name="direct"></param>
    protected override void InteractSelf(bool direct)
    {
        if (safeController != null)
        {
            if (isIncreaseButton)
                safeController.IncreaseNumber(digitIndex);
            else
                safeController.DecreaseNumber(digitIndex);
        }
        base.InteractSelf(direct);
    }
}