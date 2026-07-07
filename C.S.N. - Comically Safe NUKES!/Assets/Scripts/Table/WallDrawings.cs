using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WallDrawings : MonoBehaviour
{
    [Header("Drawings")]
    [SerializeField] private List<GameObject> _drawings;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float delayBetween = 0.2f;
    [SerializeField] private int groupSize = 4;

    private int _currentGroup = 0;
    private bool _isFading = false;

    private void Start()
    {
        foreach (GameObject drawing in _drawings)
        {
            if (drawing == null) continue;
            drawing.SetActive(true);
            SetMaterialAlpha(drawing, 0f);
        }
        Debug.Log($"Initialised {_drawings.Count} drawings with alpha 0.");
    }

    public void ActivateDrawing()
    {
        Debug.Log("ActivateDrawing called.");
        if (_isFading) { Debug.Log("Already fading, skipping."); return; }

        int startIndex = _currentGroup * groupSize;
        if (startIndex >= _drawings.Count)
        {
            Debug.Log("All drawings already activated.");
            return;
        }

        StartCoroutine(FadeInGroup(startIndex));
    }

    private IEnumerator FadeInGroup(int startIndex)
    {
        _isFading = true;
        Debug.Log($"Fading in group starting at {startIndex}");

        int endIndex = Mathf.Min(startIndex + groupSize, _drawings.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            GameObject drawing = _drawings[i];
            if (drawing == null) continue;

            drawing.SetActive(true);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                SetMaterialAlpha(drawing, alpha);
                yield return null;
            }
            SetMaterialAlpha(drawing, 1f);

            yield return new WaitForSeconds(delayBetween);
        }

        _currentGroup++;
        _isFading = false;
        Debug.Log($"Group {_currentGroup - 1} faded in.");
    }

    private void SetMaterialAlpha(GameObject obj, float alpha)
    {
        if (obj == null) return;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"{obj.name} has no Renderer!");
            return;
        }

        Material mat = renderer.material;

        // Force transparent mode (for URP)
        mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
        mat.SetFloat("_Blend", 0);   // 0 = Alpha, 1 = Premultiply
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Color color = mat.color;
        color.a = alpha;
        mat.color = color;
    }

    public void ResetDrawings()
    {
        StopAllCoroutines();
        _isFading = false;
        _currentGroup = 0;

        foreach (GameObject drawing in _drawings)
        {
            if (drawing != null)
                SetMaterialAlpha(drawing, 0f);
        }
        Debug.Log("Drawings reset.");
    }
}