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
    private bool isAnimating = false;
    private Queue<float> pendingTargets = new Queue<float>();
    private bool firstInBatch = false;

    public void ReceivePlutonium()
    {
        if (plutoniumCount >= targetScales.Count)
            return;

        pendingTargets.Enqueue(targetScales[plutoniumCount]);
        plutoniumCount++;

        if (!isAnimating)
        {
            firstInBatch = true;
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator ProcessQueue()
    {
        isAnimating = true;

        while (pendingTargets.Count > 0)
        {
            float targetY = pendingTargets.Dequeue();
            
            bool shouldDelay = firstInBatch;
            firstInBatch = false;

            yield return StartCoroutine(AnimateScaleTo(targetY, shouldDelay));
        }

        isAnimating = false;
    }

    private IEnumerator AnimateScaleTo(float targetY, bool shouldDelay)
    {
        if (shouldDelay)
            yield return new WaitForSecondsRealtime(animationDelay);

        Transform t = fluidObject.transform;
        Vector3 startScale = t.localScale;
        Vector3 targetScale = new Vector3(startScale.x, targetY, startScale.z);

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float tFactor = Mathf.Clamp01(elapsed / animationDuration);
            float newY = Mathf.Lerp(startScale.y, targetY, tFactor);
            t.localScale = new Vector3(startScale.x, newY, startScale.z);
            yield return null;
        }

        t.localScale = targetScale;
    }
}