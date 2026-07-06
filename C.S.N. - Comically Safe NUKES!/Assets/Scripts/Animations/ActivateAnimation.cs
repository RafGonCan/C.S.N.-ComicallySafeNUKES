using Unity.VisualScripting;
using UnityEngine;

public class ActivateAnimation : MonoBehaviour
{
    public Animator _awakeAnimation;
    public Animator _interactAnimation;
    public Animator _interactWrongAnimation;
    public AudioSource _audioSource;

    public void Activate()
    {
        if (_awakeAnimation == null)
            return;
        _awakeAnimation.SetTrigger("Awake");
    }

    public void Interactive()
    {
        if (_interactAnimation == null)
            return;
        _interactAnimation.SetTrigger("Interact");
    }
    public void InteractWrong()
    {
        if (_interactWrongAnimation == null)
            return;
        _interactAnimation.SetTrigger("InteractWrong");
    }

    public void PlaySound()
    {
        if (_audioSource == null)
            return;
        _audioSource.Play();
    }
}
