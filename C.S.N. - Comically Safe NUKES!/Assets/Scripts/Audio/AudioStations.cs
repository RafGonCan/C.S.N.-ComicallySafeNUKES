using UnityEngine;
using UnityEngine.Events;

public class AudioStations : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _soundClips;
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
        if (_currentClipIndex >= _soundClips.Length)
        {
            _currentClipIndex = 0;
        }
        
        _onButtonPressed?.Invoke();
    }
    void OnEnable()
    {
        if (_playOnEnable)
            PressButton();
    }
     private AudioClip GetClipFromIndex(int index)
    {   
        return _soundClips[index];
    }
}