using UnityEngine;
using UnityEngine.UI;   
using UnityEngine.SceneManagement;
using System.Collections; 

public class FadeOuter : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private float delayBeforeFade = 0f;

    public void Start()
    { 
        if (playOnStart)
        {
            int nextIndex = GetNextSceneIndex();
            StartCoroutine(FadeAndLoad(nextIndex));
        }
    }

    public void FadeToNextScene()
    {
        int nextIndex = GetNextSceneIndex();
        StartCoroutine(FadeAndLoad(nextIndex));
    }

    private int GetNextSceneIndex()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        if (nextIndex >= SceneManager.sceneCountInBuildSettings)
            nextIndex = 0;
        return nextIndex;
    }

    public IEnumerator FadeAndLoad(int sceneToLoad)
    {
        yield return new WaitForSeconds(delayBeforeFade);
        float elapsedTime = 0f;

        // Fade from 0 to 1
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