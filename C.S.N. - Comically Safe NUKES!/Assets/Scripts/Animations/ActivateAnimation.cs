using UnityEngine;

public class ActivateAnimation : MonoBehaviour
{
    public Animator _gameObjectAnimator;
    public AudioSource _audioSource;

    public void Activate()
    {
        if (_gameObjectAnimator == null)
            return;
        _gameObjectAnimator.SetTrigger("Awake");
    }

    public void PlaySound()
    {
        if (_audioSource == null)
            return;
        _audioSource.Play();
    }
}
