using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    [SerializeField] private AudioSource teste;
    
    void Awake()
    {
        teste.Play();
    }
}
