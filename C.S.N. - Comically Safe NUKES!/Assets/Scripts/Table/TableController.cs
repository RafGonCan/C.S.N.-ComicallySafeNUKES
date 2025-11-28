using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour
{
    [SerializeField] private int[] correctSequence = new int[3] { 3, 7, 11 };
    [SerializeField] private List<GameObject> buttons = new List<GameObject>();
    [SerializeField] private Interactive _plutonium1;
    [SerializeField] private Interactive _plutonium2;
    
    private List<int> currentSequence = new List<int>(); //sequencia que está a ser introduzida
    private bool _isSolved = false;
    private List<int> pressedButtons = new List<int>(); // lista de botões clicados
    
    public bool IsSolved => _isSolved;

    void Start()
    {
        SetButtonState(true);
    }

    private void CheckTableActivation()
    {
        bool onChecker = (_plutonium1 != null && _plutonium1.isOn) && (_plutonium2 != null && _plutonium2.isOn);
        if (onChecker) SetButtonState(true);
    }
    private void SetButtonState(bool state)
    {
        foreach (GameObject button in buttons)
        {
            if(button != null) button.SetActive(state);
        }
    }

    public void OnButtonPressed(int buttonIndex)
    {
        if (_isSolved) return;
        
        // Se o botão ja tiver clicado, ele não clica
        if (pressedButtons.Contains(buttonIndex)) return;
        
        pressedButtons.Add(buttonIndex);
        currentSequence.Add(buttonIndex);
        
        if (currentSequence.Count >= correctSequence.Length)
        {
            CheckSequence();
        }
    }

    private void CheckSequence()
    {
        bool correct = true;
        
        for (int i = 0; i < correctSequence.Length; i++)
        {
            if (currentSequence[i] != correctSequence[i])
            {
                correct = false;
                break;
            }
        }
        
        if (correct)
        {
            PuzzleSolved();
        }
        else
        {
            PuzzleFailed();
        }
    }

    private void PuzzleSolved()
    {
        _isSolved = true;
        Debug.Log("Tá certo");
    }

    private void PuzzleFailed()
    {
        Debug.Log("Tá errado");
        Invoke(nameof(ResetPuzzle), 1f);
    }

    private void ResetPuzzle()
    {
        Debug.Log("Resetting");
        currentSequence.Clear();
        pressedButtons.Clear();
        
        foreach (var button in buttons)
        {
            var seqButton = button.GetComponent<SequenceButton>();
            if (seqButton != null)
            {
                seqButton.ResetButton();
            }
        }
    }
    public bool CanPressButton(int buttonIndex)
    {
        return !_isSolved && !pressedButtons.Contains(buttonIndex);
    }
}