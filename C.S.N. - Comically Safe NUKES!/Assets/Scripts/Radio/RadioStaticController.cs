using UnityEngine;
using System.Collections;

public class RadioStaticController : MonoBehaviour
{
    [Header("Static")]
    [SerializeField] private AudioSource _staticSource;
    [SerializeField] private AudioClip _staticClip;
    [SerializeField] private float _staticVolume = 0.8f;

    [Header("Fix & Dependencies")]
    [SerializeField] private Interactive _requirementSlot;
    [SerializeField] private Material _radioMaterial;
    [SerializeField] private Collider _buttonCollider;
    [SerializeField] private Collider _radioCollider;

    private Interactive _radioInteractive;
    private bool _isFixed = false;
    private bool _isStaticPlaying = false;
    private readonly WaitForSeconds _staticCheckInterval = new WaitForSeconds(5f);

    void Start()
    {
        if (_buttonCollider != null)
            _buttonCollider.enabled = false;
        _radioInteractive = GetComponent<Interactive>();

        if (_staticSource == null)
            _staticSource = gameObject.AddComponent<AudioSource>();
        _staticSource.spatialBlend = 1f;
        _staticSource.playOnAwake = false;
        _staticSource.volume = _staticVolume;

        if (_requirementSlot != null)
            _requirementSlot.onRequirementUsed.AddListener(OnAntennaPlaced);
        else
            Debug.LogWarning("Requirement slot not assigned – will rely on Update check.");

        StartStatic();
    }

    void Update()
    {
        if (!_isFixed && _radioInteractive != null && _radioInteractive.AreRequirementsMet)
        {
            FixRadio();
        }
    }
    private void OnAntennaPlaced(Interactive usedItem)
    {
        if (_isFixed) return;
        FixRadio();
    }

    private void FixRadio()
    {
        if (_isFixed) return;
        _isFixed = true;

        if (_buttonCollider != null)
        _buttonCollider.enabled = true;
        _radioCollider.enabled = false;

        if (_isStaticPlaying)
            StopStatic();
        else
            SetSharpness(0f);

        if (_radioInteractive != null)
            _radioInteractive.enabled = false;

        Debug.Log("Radio fixed – static stopped, button enabled.");
    }

    public void ToggleStatic()
    {
        if (_isFixed) return;
        if (_isStaticPlaying)
            StopStatic();
        else
            StartStatic();
    }
    private IEnumerator RestartStaticAfterDelay()
    {
        yield return _staticCheckInterval;
        StartStatic();
    }

    private void StartStatic()
    {
        if (_staticClip == null || _isStaticPlaying) return;
        _staticSource.clip = _staticClip;
        _staticSource.Play();
        _isStaticPlaying = true;
        SetSharpness(0.95f);
    }

    private void StopStatic()
    {
        if (!_isStaticPlaying) return;
        _staticSource.Stop();
        _staticSource.clip = null;
        _isStaticPlaying = false;
        SetSharpness(0f);
        StartCoroutine(RestartStaticAfterDelay());
    }

    private void SetSharpness(float value)
    {
        if (_radioMaterial != null)
            _radioMaterial.SetFloat("_Sharpness", value);
    }
}