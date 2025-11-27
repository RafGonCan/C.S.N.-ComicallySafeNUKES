using System.Linq;
using UnityEngine;

public class SafeController : MonoBehaviour
{
    [SerializeField] private int[] currentNumbers = new int[3] {0, 0, 0};
    [SerializeField] private int[] correctCombination = new int[3] {5, 2, 7};
    public bool IsUnlocked => _unlocked;
    private bool _unlocked = false;
    public int _minNumber = 0;
    public int _maxNumber = 9;

    public void IncreaseNumber(int currentNumber)
    {
        if (_unlocked) return;

        currentNumbers[currentNumber]++;
        if (currentNumbers[currentNumber] > _maxNumber)
            currentNumbers[currentNumber] = _minNumber;

        CheckCombination();
    }

    public void DecreaseNumber(int currentNumber)
    {
        if (_unlocked) return;

        currentNumbers[currentNumber]--;
        if (currentNumbers[currentNumber] < _minNumber)
            currentNumbers[currentNumber] = _maxNumber;

        CheckCombination();
    }

    private void CheckCombination()
    {
        bool correctCode = currentNumbers.SequenceEqual(correctCombination);
        if (correctCode && !_unlocked)
        {
            UnlockSafe();
        }
    }

    private void UnlockSafe()
    {
        _unlocked = true;
        Debug.Log("Bro it opens");
    }
}