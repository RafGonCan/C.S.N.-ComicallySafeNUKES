using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Launch : Interactive
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;

    protected override void InteractSelf(bool direct)
    {
        base.InteractSelf(direct);

        if (playerMovement != null)
            playerMovement.enabled = false;

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        SceneManager.LoadScene(sceneToLoad);
    }
}