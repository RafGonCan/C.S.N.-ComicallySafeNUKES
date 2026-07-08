using UnityEngine;
using System.Collections;

public class SequenceButton : Interactive
{
    [Header("Button Settings")]
    [SerializeField] private TableController tableController;
    [SerializeField] private int buttonIndex;
    [SerializeField] private float pressIntensity = 0.2f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Power State")]
    [SerializeField] private Material poweredMaterial;
    [SerializeField] private Material unpoweredMaterial;

    private Light spotLight => GetComponentInChildren<Light>(true);
    private bool _isPressed;
    private Coroutine _fadeCoroutine;
    private bool _isPowered = false;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
            _renderer = GetComponentInChildren<Renderer>();
    }

    public void SetPoweredState(bool powered)
    {
        _isPowered = powered;

        if (_renderer != null)
            _renderer.material = powered ? poweredMaterial : unpoweredMaterial;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = powered;

        isOn = powered;
    }

    public override void Interact()
    {
        if (!isOn || !_isPowered)
            return;

        InteractSelf(true);
    }

    protected override void InteractSelf(bool direct)
    {
        if (spotLight == null) return;

        if (_isPressed || (tableController != null && !tableController.CanPressButton(buttonIndex)))
            return;

        FadeLightTo(pressIntensity);

        // Fire the direct interaction event (if any) – optional
        onDirectInteract?.Invoke();

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