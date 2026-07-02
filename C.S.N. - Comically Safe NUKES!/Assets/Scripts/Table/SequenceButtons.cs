using UnityEngine;
using System.Collections;

public class SequenceButton : Interactive
{
    [Header("Button Settings")]
    [SerializeField] private TableController tableController;
    [SerializeField] private int buttonIndex;
    [SerializeField] private float pressIntensity = 0.2f;
    [SerializeField] private float fadeDuration = 0.5f;

    private Light spotLight => GetComponentInChildren<Light>(true);
    private bool _isPressed;
    private Coroutine _fadeCoroutine;

    protected override void InteractSelf(bool direct)
    {
        if (spotLight == null) return;

        if (_isPressed || (tableController != null && !tableController.CanPressButton(buttonIndex)))
            return;

        FadeLightTo(pressIntensity);

        base.InteractSelf(direct);

        _isPressed = true;
        if (tableController != null)
            tableController.OnButtonPressed(buttonIndex);
    }

    public void ResetButton()
    {
        _isPressed = false;
        FadeLightTo(0f);
    }

    public void FadeLightTo(float targetIntensity)
    {
        if (spotLight == null) return;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeLightCoroutine(targetIntensity));
    }

    private IEnumerator FadeLightCoroutine(float targetIntensity)
    {
        float startIntensity = spotLight.intensity;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            spotLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
            yield return null;
        }

        spotLight.intensity = targetIntensity;
        _fadeCoroutine = null;
    }
}