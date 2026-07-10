using UnityEngine;
using UnityEngine.Events;

public class AudioStations : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    public AudioSource AudioSource => _audioSource;
    [SerializeField] private AudioSource _audioSourceFromPlayer;
    public AudioSource AudioSourceFromPlayer => _audioSourceFromPlayer;
    [SerializeField] private AudioClip[] _soundClips;
    public AudioClip[] SoundClips => _soundClips;
    [SerializeField] private AudioClip _playerVoiceLine;
    public AudioClip PlayerVoiceLine => _playerVoiceLine;
    [SerializeField] private bool _playOnEnable;
    private UnityEvent _onButtonPressed;
    public UnityEvent OnButtonPressed => _onButtonPressed;
    private int _currentClipIndex = 0;
    public int CurrentClipIndex => _currentClipIndex;
    public bool audioOn;
    [SerializeField] private GameObject _previousButton;
    [SerializeField] private GameObject _nextButton;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f;
        _audioSource.playOnAwake = false;

        _currentClipIndex = -1;

        PreloadAudioClips();

        if (audioOn && _playOnEnable)
            PlayCurrentClip();
    }

    private void PreloadAudioClips()
    {
        if (_soundClips == null) return;
        foreach (AudioClip clip in _soundClips)
        {
            if (clip != null)
                clip.LoadAudioData();
        }
        Debug.Log($"Preloaded {_soundClips.Length} audio clips.");
    }

    private void OnEnable()
    {
        if (_playOnEnable && audioOn)
        {
            if (_audioSource != null && !_audioSource.isPlaying)
                PlayCurrentClip();
        }
    }

    // For normal objects like open‑close doors
    public void PressButton()
    {
        _currentClipIndex++;
        if (_currentClipIndex >= _soundClips.Length)
            _currentClipIndex = 0;

        StopAndPlayCurrent();

        InteractiveData data = GetComponent<Interactive>()?.interactiveData;
        if (data != null && !data.hasPlayedVoiceLine && _playerVoiceLine != null)
        {
            _audioSourceFromPlayer?.PlayOneShot(_playerVoiceLine);
            data.hasPlayedVoiceLine = true;
        }

        _onButtonPressed?.Invoke();
    }

    // For radio or objects that require back and forth (next station)
    public void Next()
    {
        _currentClipIndex++;
        if (_currentClipIndex >= _soundClips.Length)
            _currentClipIndex = 0;
        StopAndPlayCurrent();
    }

    // Same as above (previous station)
    public void Previous()
    {
        _currentClipIndex--;
        if (_currentClipIndex < 0)
            _currentClipIndex = _soundClips.Length - 1;
        StopAndPlayCurrent();
    }

    private void StopAndPlayCurrent()
    {
        if (_audioSource.isPlaying)
            _audioSource.Stop();
        if (audioOn)
            PlayCurrentClip();
    }

    public void PlayPlayerVoiceLine()
    {
        if (_playerVoiceLine != null && _audioSourceFromPlayer != null)
            _audioSourceFromPlayer.PlayOneShot(_playerVoiceLine);
    }

    protected AudioClip GetClipFromIndex(int index)
    {
        return _soundClips[index];
    }

    protected void SetCurrentClip(int index)
    {
        if (index >= 0 && index < _soundClips.Length)
            _currentClipIndex = index;
    }

    public void ChangeState()
    {
        audioOn = !audioOn;

        if (audioOn)
            PlayCurrentClip();
        else
            _audioSource.Stop();
    }

    private void PlayCurrentClip()
    {
        if (_soundClips == null || _soundClips.Length == 0) return;
        if (_currentClipIndex < 0 || _currentClipIndex >= _soundClips.Length) return;

        AudioClip clip = _soundClips[_currentClipIndex];
        if (clip == null) return;

        if (_audioSource.isPlaying)
            _audioSource.Stop();

        _audioSource.PlayOneShot(clip);
    }
}