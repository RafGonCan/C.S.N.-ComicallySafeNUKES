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
    [SerializeField] private int _failAmount = 0;
    [SerializeField] private AudioClip _lightSfx;

    [Header("Lights & Emission")]
    [SerializeField] private Light[] roomLights;
    [SerializeField] private Renderer[] emissiveObjects;

    [Header("Emission Settings")]
    [SerializeField] private Color emissionOn = Color.white;
    [SerializeField] private Color emissionOff = Color.black;

    private bool _lightsOn = false;

    private List<int> currentSequence = new List<int>();
    private static bool _isSolved = false;
    private List<int> pressedButtons = new List<int>();

    private int _plutoniumCount = 0;
    public bool IsPowered => _plutoniumCount >= 2;
    public static bool IsSolved => _isSolved;

    void Start()
    {
        SetButtonState(true);
        UpdateAllButtonsState();
        TurnOffRoomLights();
    }

    private void SetButtonState(bool state)
    {
        foreach (GameObject button in buttons)
        {
            if (button != null) button.SetActive(state);
        }
    }

    public void ActivatePlutonium()
    {
        _plutoniumCount++;
        UpdateAllButtonsState();

        if (_plutoniumCount >= 2 && !_lightsOn)
        {
            TurnOnRoomLights();
        }
    }

    private void UpdateAllButtonsState()
    {
        bool powered = IsPowered;
        foreach (GameObject buttonObj in buttons)
        {
            SequenceButton seq = buttonObj.GetComponent<SequenceButton>();
            if (seq != null)
                seq.SetPoweredState(powered);
        }
    }

    private void TurnOnRoomLights()
    {
        if (roomLights != null)
        {
            AudioSource.PlayClipAtPoint(_lightSfx, transform.position);
            foreach (Light light in roomLights)
            {
                if (light != null)
                    light.enabled = true;
            }
        }

        if (emissiveObjects != null)
        {
            foreach (Renderer renderer in emissiveObjects)
            {
                if (renderer != null)
                {
                    Material mat = renderer.material;
                    mat.SetColor("_EmissionColor", emissionOn);
                    mat.EnableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
            }
        }

        _lightsOn = true;
    }

    private void TurnOffRoomLights()
    {
        if (roomLights != null)
        {
            foreach (Light light in roomLights)
            {
                if (light != null)
                    light.enabled = false;
            }
        }

        if (emissiveObjects != null)
        {
            foreach (Renderer renderer in emissiveObjects)
            {
                if (renderer != null)
                {
                    Material mat = renderer.material;
                    mat.SetColor("_EmissionColor", emissionOff);
                    mat.DisableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                }
            }
        }

        _lightsOn = false;
    }

    public void OnButtonPressed(int buttonIndex)
    {
        if (_isSolved) return;
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
            PuzzleSolved();
        else
            PuzzleFailed();
    }

    private void PuzzleSolved()
    {
        _isSolved = true;
        _panelAnimator.SetBool("Solved", true);
        AudioSource.PlayClipAtPoint(_rightSequence, transform.position);
        InteractionManager.instance.steamManager.UnlockAchievement("CSN_P3");
    }

    private void PuzzleFailed()
    {
        Invoke(nameof(ResetPuzzle), 0.5f);
    }

    private void ResetPuzzle()
    {
        currentSequence.Clear();
        pressedButtons.Clear();

        foreach (var button in buttons)
        {
            SequenceButton seqButton = button.GetComponent<SequenceButton>();
            if (seqButton != null)
                seqButton.ResetButton();
        }
        AudioSource.PlayClipAtPoint(_wrongSequence, transform.position);
        _failAmount++;
        if (_failAmount >= 3)
        {
            _playerAudioSource.PlayOneShot(_wrongVoiceline);
            SubtitleManager.Instance?.PlaySubtitles(_wrongVoiceline.name, _playerAudioSource, _wrongVoiceline.length);
            _failAmount = 0;
        }
    }

    public bool CanPressButton(int buttonIndex)
    {
        return IsPowered && !_isSolved && !pressedButtons.Contains(buttonIndex);
    }
}