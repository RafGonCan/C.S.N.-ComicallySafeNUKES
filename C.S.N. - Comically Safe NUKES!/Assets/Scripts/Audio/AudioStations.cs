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
    private UnityEvent  _onButtonPressed;
    public UnityEvent OnButtonPressed => _onButtonPressed;
    private int         _currentClipIndex = 0;
    public int          CurrentClipIndex => _currentClipIndex;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f;
        _audioSource.playOnAwake = false;


        PreloadAudioClips();
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

    public void PressButton()
    {
        if (_audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
        AudioClip currentClip = GetClipFromIndex(_currentClipIndex);
        
        _audioSource?.PlayOneShot(currentClip);

        InteractiveData data = GetComponent<Interactive>()?.interactiveData;
        if (data != null && !data.hasPlayedVoiceLine && _playerVoiceLine != null)
        {
            _audioSourceFromPlayer?.PlayOneShot(_playerVoiceLine);
            data.hasPlayedVoiceLine = true;
        }

        _currentClipIndex++;
        if (_currentClipIndex >= _soundClips.Length)
        {
            _currentClipIndex = 0;
        }
        
        _onButtonPressed?.Invoke();
    }
    public void PlayPlayerVoiceLine()
    {
        if (_playerVoiceLine != null && _audioSourceFromPlayer != null)
        {
            _audioSourceFromPlayer.PlayOneShot(_playerVoiceLine);
        }
    }    
    void OnEnable()
    {
        if (_playOnEnable)
            PressButton();
    }
    
    protected AudioClip GetClipFromIndex(int index)
    {   
        return _soundClips[index];
    }
    protected void SetCurrentClip(int index)
    {
        if (index >= 0 && index < _soundClips.Length)
        {
            _currentClipIndex = index;
        }
    }
}