using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FluidScale : MonoBehaviour
{
    [SerializeField] private GameObject fluidObject;
    [SerializeField] private float animationDelay = 2f;
    [SerializeField] private float animationDuration = 1f;

    private readonly List<float> targetScales = new List<float> { 0.33f, 0.66f, 1f };
    private int plutoniumCount = 0;
    private Coroutine currentAnimation = null;
    private bool isAnimating = false;

    public void ReceivePlutonium()
    {
        if (plutoniumCount >= targetScales.Count || isAnimating)
            return;

        isAnimating = true;
        currentAnimation = StartCoroutine(AnimateScaleTo(targetScales[plutoniumCount]));
        plutoniumCount++;
    }

    private IEnumerator AnimateScaleTo(float targetY)
    {
        yield return new WaitForSeconds(animationDelay);

        Transform t = fluidObject.transform;
        Vector3 startScale = t.localScale;
        Vector3 targetScale = new Vector3(startScale.x, targetY, startScale.z);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float tFactor = elapsed / animationDuration;
            float newY = Mathf.Lerp(startScale.y, targetY, tFactor);
            t.localScale = new Vector3(startScale.x, newY, startScale.z);
            yield return null;
        }

        t.localScale = targetScale;
        currentAnimation = null;
        isAnimating = false;
    }
}