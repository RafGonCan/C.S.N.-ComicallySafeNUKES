using System.Linq;
using UnityEngine;

public class SafeController : MonoBehaviour
{
    [SerializeField] private int[] currentNumbers = new int[3] {0, 0, 0};
    [SerializeField] private int[] correctCombination = new int[3] {5, 2, 7};
    [SerializeField] private Interactive safeDoor;
    
    public bool IsUnlocked => _unlocked;
    private bool _unlocked = false;
    public int _minNumber = 0;
    public int _maxNumber = 9;

    public void IncreaseNumber(int digitIndex)
    {
        currentNumbers[digitIndex]++;
        if (currentNumbers[digitIndex] > _maxNumber)
            currentNumbers[digitIndex] = _minNumber;

        CheckCombination();
    }

    public void DecreaseNumber(int digitIndex)
    {
        currentNumbers[digitIndex]--;
        if (currentNumbers[digitIndex] < _minNumber)
            currentNumbers[digitIndex] = _maxNumber;

        CheckCombination();
    }

    private void CheckCombination()
    {
        bool isCorrect = currentNumbers.SequenceEqual(correctCombination);
        
        if (isCorrect && !_unlocked)
        {
            _unlocked = true;
            
            if (safeDoor != null)
            {
                safeDoor.SetRequirementsMet(true);
            }
        }
        else if (!isCorrect && _unlocked)
        {
            _unlocked = false;
            
            if (safeDoor != null)
            {
                safeDoor.SetRequirementsMet(false);
            }
        }
    }
}