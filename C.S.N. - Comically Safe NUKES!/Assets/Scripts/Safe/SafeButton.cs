using UnityEngine;

public class SafeButton : Interactive
{
    [SerializeField] private SafeController safeController;
    [SerializeField] private int digitIndex;
    [SerializeField] private bool isIncreaseButton;

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