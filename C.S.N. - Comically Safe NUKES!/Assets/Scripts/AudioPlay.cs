using UnityEngine;
using UnityEngine.Events;

public class AudioPlay : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundClip;
    [SerializeField] private bool playOnEnable;
    private UnityEvent onButtonPressed;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    public void PressButton()
    {
        AudioManager.Instance?.StopAllSounds();
        
        audioSource.PlayOneShot(soundClip);
        
        AudioManager.Instance?.RegisterActiveSound(audioSource);
        
        onButtonPressed?.Invoke();
    }

    void Update()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            AudioManager.Instance?.UnregisterActiveSound(audioSource);
        }
    }

    void OnEnable()
    {
        if (playOnEnable)
            PressButton();
    }
}