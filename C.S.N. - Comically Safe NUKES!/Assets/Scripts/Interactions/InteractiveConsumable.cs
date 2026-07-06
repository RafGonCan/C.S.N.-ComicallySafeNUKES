using UnityEngine;
using System.Collections;

public class InteractiveConsumable : Interactive
{
    [Header("Eating Effects")]
    [SerializeField] private ParticleSystem eatParticles;
    [SerializeField] private float particleLifetime = 2f;
    [SerializeField] private float spawnDistance = 1.5f;

    [Header("Eating Sounds (random queue)")]
    [SerializeField] private AudioClip[] eatSounds;
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private float soundInterval = 0.15f;
    [SerializeField] private int soundCount = 4;

    private bool _wasPickedUp = false;

    protected override void InteractSelf(bool direct)
    {
        if (_wasPickedUp) return;
        _wasPickedUp = true;

        // Disable mesh and collider immediately – object "disappears"
        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Spawn particles on camera
        if (eatParticles != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 spawnPos = cam.transform.position + cam.transform.forward * spawnDistance;
                ParticleSystem ps = Instantiate(eatParticles, spawnPos, Quaternion.identity);
                ps.transform.SetParent(cam.transform);
                ps.Play();
                Destroy(ps.gameObject, particleLifetime);
            }
            else
            {
                Debug.LogWarning("No main camera found for particle spawn.");
            }
        }

        // Play sounds, then fully deactivate the object
        StartCoroutine(PlayRandomSoundsAndDisable());

        base.InteractSelf(direct);
    }

    private IEnumerator PlayRandomSoundsAndDisable()
    {
        if (eatSounds == null || eatSounds.Length == 0)
        {
            gameObject.SetActive(false);
            yield break;
        }

        for (int i = 0; i < soundCount; i++)
        {
            AudioClip clip = eatSounds[Random.Range(0, eatSounds.Length)];

            if (clip != null)
            {
                if (playerAudioSource != null)
                    playerAudioSource.PlayOneShot(clip);
                else
                    AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
            }

            yield return new WaitForSeconds(soundInterval);
        }

        // Fully disable the object after all sounds
        gameObject.SetActive(false);
    }
}