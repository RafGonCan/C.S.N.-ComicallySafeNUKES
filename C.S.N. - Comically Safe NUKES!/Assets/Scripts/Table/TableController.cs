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
    [SerializeField] private AudioSource _playerAudioSource;
    [SerializeField] private AudioClip _wrongVoiceline;
    [SerializeField] private bool _hasPlayedVoiceline = false;
    [SerializeField] private int _failAmount = 0;
    
    private List<int> currentSequence = new List<int>(); //Introduced sequence
    private bool _isSolved = false;
    private List<int> pressedButtons = new List<int>(); //Buttons that have been added to the sequence
    
    public bool IsSolved => _isSolved;

    void Start()
    {
        //Sets every button to "on", if we want to add requirements to turning on the puzzle
        // We just need to call this function at the end
        SetButtonState(true);
        
    }
    /// <summary>
    /// Sets every button state to true/false
    /// </summary>
    /// <param name="state"></param>
    private void SetButtonState(bool state)
    {
        foreach (GameObject button in buttons)
        {
            if(button != null) button.SetActive(state);
        }
    }
    /// <summary>
    /// If the puzzle is solved or that button has been pressed, you can't press the button again until finishing a sequence.
    /// However, everytime a button is pressed, it is added to the sequence.
    /// Everytime it is added to the sequence, CheckSequence() is called to see if it is correct.
    /// </summary>
    /// <param name="buttonIndex"></param>
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

    /// <summary>
    /// Compares the correct sequence to the current sequence and sets "correct" to true/false
    /// if "correct" is true, it calls PuzzleSolved(), otherwise PuzzleFailed();
    /// </summary>
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
    /// <summary>
    /// When the sequence is correct, it calls the animator's boolean "solved" and "_isSolved", setting both to true
    /// Playing the compartment opening animation
    /// </summary>
    private void PuzzleSolved()
    {
        _isSolved = true;
        _panelAnimator.SetBool("Solved", true);
        AudioSource.PlayClipAtPoint(_rightSequence, transform.position);
    }
    /// <summary>
    /// When the sequence is incorrect, it resets the puzzle by calling ResetPuzzle() after 0.5 seconds
    /// </summary>
    private void PuzzleFailed()
    {
        Debug.Log("Tá errado");
        Invoke(nameof(ResetPuzzle), 0.5f);
    } 
    /// <summary>
    /// Resets te puzzle by clearing the pressed buttons and the current sequence
    /// Also plays the sound for when doing the wrong sequence
    /// </summary>
    private void ResetPuzzle()
    {
        Debug.Log("Resetting");
        currentSequence.Clear();
        pressedButtons.Clear();
        
        foreach (var button in buttons)
        {
            SequenceButton seqButton = button.GetComponent<SequenceButton>();
            if (seqButton != null)
            {
                seqButton.ResetButton();
            }
        }
        AudioSource.PlayClipAtPoint(_wrongSequence, transform.position);
        _failAmount++;
        if (_failAmount >= 3 && !_hasPlayedVoiceline)
        {
            _playerAudioSource.PlayOneShot(_wrongVoiceline);
            _hasPlayedVoiceline = true;
        }
    }
    /// <summary>
    /// Simple boolean checker for when you can or can't press a button
    /// You have to finish a sequence to re-press a button
    /// </summary>
    /// <param name="buttonIndex"></param>
    /// <returns></returns>
    public bool CanPressButton(int buttonIndex)
    {
        return !_isSolved && !pressedButtons.Contains(buttonIndex);
    }
}