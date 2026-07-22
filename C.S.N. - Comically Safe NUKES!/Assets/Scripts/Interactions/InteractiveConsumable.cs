using UnityEngine;
using System.Collections;

public class InteractiveConsumable : Interactive
{
    private static int _pizzasEaten = 0;
    public static int PizzasEaten => _pizzasEaten;

    public static void ResetPizzaCount() => _pizzasEaten = 0;

    [SerializeField] private ParticleSystem eatParticles;
    [SerializeField] private float particleLifetime = 2f;
    [SerializeField] private float spawnDistance = 1.5f;

    [SerializeField] private AudioClip[] eatSounds;
    [SerializeField] private AudioSource playerAudioSource;
    [SerializeField] private float soundInterval = 0.15f;
    [SerializeField] private int soundCount = 4;

    private bool _wasPickedUp = false;

    protected override void InteractSelf(bool direct)
    {
        if (_wasPickedUp) return;
        _wasPickedUp = true;

        _pizzasEaten++;
        Debug.Log($"Pizza eaten! Total: {_pizzasEaten}");
        if (_pizzasEaten >= 18)
        {
            InteractionManager.instance.steamManager.UnlockAchievement("CSN_PIZZA");
        }

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

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

        gameObject.SetActive(false);
    }
}