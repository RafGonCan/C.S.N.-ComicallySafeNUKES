using System.Linq;
using UnityEngine;

public class SafeController : Interactive
{
    [SerializeField] private int[] currentNumbers = new int[3] {0, 0, 0};
    [SerializeField] private int[] correctCombination = new int[3] {5, 2, 7};
    [SerializeField] private CameraFocusController focusCamera;
    private ActivateAnimation _activateAnimation => GetComponent<ActivateAnimation>();
    [SerializeField] private GameObject vaultDoor;
    private ActivateAnimation vaultDoorActivateAnimation => vaultDoor.GetComponentInChildren<ActivateAnimation>();
    
    public bool IsUnlocked => _unlocked;
    private bool _unlocked = false;
    public int _minNumber = 0;
    public int _maxNumber = 9;


    public void IncreaseNumber(int digitIndex)
    {
        currentNumbers[digitIndex]++;
        if (currentNumbers[digitIndex] > _maxNumber)
            currentNumbers[digitIndex] = _minNumber;
    }

    public void DecreaseNumber(int digitIndex)
    {
        currentNumbers[digitIndex]--;
        if (currentNumbers[digitIndex] < _minNumber)
            currentNumbers[digitIndex] = _maxNumber;
    }

    private void CheckCombination()
    {
        bool isCorrect = currentNumbers.SequenceEqual(correctCombination);
        
        if (isCorrect && !_unlocked)
        {
            _unlocked = true;
            SetRequirementsMet(true);
            focusCamera.ExitButton();
        }
        else if (!isCorrect && _unlocked)
        {
            _unlocked = false;
            SetRequirementsMet(false);
        }
    }

    protected override void InteractSelf(bool direct)
    {
        CheckCombination();
        if (_activateAnimation != null)
        {
            if (_unlocked)
            {
                if (vaultDoorActivateAnimation != null) vaultDoorActivateAnimation.Interactive();
                _activateAnimation.Interactive();
                _activateAnimation.PlaySound();
            }
            else
            {
                _activateAnimation.InteractWrong();
                _activateAnimation.PlaySound();
            }
        }
    }
    public void InteractUI()
    {
        InteractSelf(true);
    }
}