using System.Collections.Generic;
using UnityEngine;

public class TableController : MonoBehaviour
{
    [SerializeField] private int[] correctSequence = new int[3] { 3, 7, 11 };
    [SerializeField] private List<GameObject> buttons = new List<GameObject>();
    [SerializeField] private GameObject _panel;
    [SerializeField] private AudioClip _rightSequence;
    [SerializeField] private AudioClip _wrongSequence;
    [SerializeField] private Animator _panelAnimator;
    
    private List<int> currentSequence = new List<int>(); //sequencia que está a ser introduzida
    private bool _isSolved = false;
    private List<int> pressedButtons = new List<int>(); // lista de botões clicados
    
    public bool IsSolved => _isSolved;

    void Start()
    {
        SetButtonState(true);
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
        _panelAnimator.SetBool("Solved", true);
        AudioSource.PlayClipAtPoint(_rightSequence, transform.position);
    }

    private void PuzzleFailed()
    {
        Debug.Log("Tá errado");
        Invoke(nameof(ResetPuzzle), 0.5f);
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
        AudioSource.PlayClipAtPoint(_wrongSequence, transform.position);
    }
    public bool CanPressButton(int buttonIndex)
    {
        return !_isSolved && !pressedButtons.Contains(buttonIndex);
    }
}