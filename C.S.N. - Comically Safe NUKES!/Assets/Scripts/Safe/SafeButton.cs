using UnityEngine;

public class SafeButton : MonoBehaviour
{
    [SerializeField] private SafeController safeController;
    [SerializeField] private int digitIndex;
    private ActivateAnimation _activateAnimation => GetComponent<ActivateAnimation>();

    public void GUIInteract()
    {
        Debug.Log("SafeButton GUIInteract called");
        if (safeController != null)
        {
            safeController.IncreaseNumber(digitIndex);
            if (_activateAnimation != null)
            {
                _activateAnimation.Interactive();
                _activateAnimation.PlaySound();
            }
        }
    }
}