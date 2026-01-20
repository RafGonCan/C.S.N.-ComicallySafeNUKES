using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class Launch : Interactive
{
    [SerializeField] private string sceneToLoad;
    protected override void InteractSelf(bool direct)
    {
        StartCoroutine(LoadSceneAfterDelay(1f, sceneToLoad));
        base.InteractSelf(direct);
    }
    private IEnumerator LoadSceneAfterDelay(float delay, string sceneName)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}