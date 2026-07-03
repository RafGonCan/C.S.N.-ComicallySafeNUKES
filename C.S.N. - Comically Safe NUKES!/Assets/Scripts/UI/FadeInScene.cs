using UnityEngine;
using System.Collections;

public class FadeInScene : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool fadeOutOnStart = true;
    void Start()
    {
        if (fadeOutOnStart)
        {
            StartCoroutine(FadeOut());
        }
    }
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

}
