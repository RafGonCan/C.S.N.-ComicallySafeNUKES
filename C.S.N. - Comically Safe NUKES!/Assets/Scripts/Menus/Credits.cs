using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Credits : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ExitAfterDelay());
    }

    private IEnumerator ExitAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Credits");
    }
}
