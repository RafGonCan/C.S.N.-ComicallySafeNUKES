using UnityEngine;
using UnityEngine.Events;

public class AudioStations : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _soundClip1;
    [SerializeField] private AudioClip _soundClip2;
    [SerializeField] private AudioClip _soundClip3;
    [SerializeField] private AudioClip _soundClip4;
    [SerializeField] private bool _playOnEnable;
    private UnityEvent _onButtonPressed;
    private int _currentClipIndex = 0;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
        
        _audioSource.spatialBlend = 1f;
        _audioSource.playOnAwake = false;
    }

    public void PressButton()
    {
        if (_audioSource.isPlaying)
        {
        _audioSource.Stop();
        }
        AudioClip currentClip = GetClipFromIndex(_currentClipIndex);
        
        _audioSource?.PlayOneShot(currentClip);

        _currentClipIndex++;
        if (_currentClipIndex >= 4)
        {
            _currentClipIndex = 0;
        }
        
        _onButtonPressed?.Invoke();
    }
    private AudioClip GetClipFromIndex(int index)
    {
        switch (index)
        {
            case 0: return _soundClip1;
            case 1: return _soundClip2;
            case 2: return _soundClip3;
            case 3: return _soundClip4;
            default: return null;
        }
    }
    void OnEnable()
    {
        if (_playOnEnable)
            PressButton();
    }
}