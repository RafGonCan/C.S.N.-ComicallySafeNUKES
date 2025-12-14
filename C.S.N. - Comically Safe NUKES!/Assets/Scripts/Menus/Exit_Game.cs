using System.Collections;
using UnityEngine;

public class Exit_Game : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(ExitAfterDelay());
    }

    private IEnumerator ExitAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
