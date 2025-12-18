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
    /// <summary>
    /// Increases the value of the button with this digitIndex
    /// Checks combination
    /// </summary>
    /// <param name="digitIndex"></param>
    public void IncreaseNumber(int digitIndex)
    {
        currentNumbers[digitIndex]++;
        if (currentNumbers[digitIndex] > _maxNumber)
            currentNumbers[digitIndex] = _minNumber;

        CheckCombination();
    }
    /// <summary>
    /// Increases the value of the button with this digitIndex, currently unused
    /// Checks combination
    /// </summary>
    /// <param name="digitIndex"></param>
    public void DecreaseNumber(int digitIndex)
    {
        currentNumbers[digitIndex]--;
        if (currentNumbers[digitIndex] < _minNumber)
            currentNumbers[digitIndex] = _maxNumber;

        CheckCombination();
    }
    /// <summary>
    /// Checks if the number combination is equal to the correct combination.
    /// If it is, it will set the requirements to true, otherwise, false
    /// </summary>
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