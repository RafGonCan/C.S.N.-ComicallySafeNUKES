using UnityEngine;
using System.Collections;

public class RadioStaticController : MonoBehaviour
{
    [Header("Static")]
    [SerializeField] private AudioSource _staticSource;
    [SerializeField] private AudioClip _staticClip;
    [SerializeField] private float _staticVolume = 0.8f;
    [SerializeField] private float _staticWhenOn = 0.5f;
    [SerializeField] private float _staticWhenOff = 0.2f;
    private bool _stationsToggle = true;
    private AudioStations _audioStations => GetComponent<AudioStations>();

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

    void FixedUpdate()
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
        if (_radioCollider != null)
            _radioCollider.enabled = false;

        if (_isStaticPlaying)
            StopStatic();

        if (_radioInteractive != null)
            _radioInteractive.enabled = false;

        Debug.Log("Radio fixed – static stopped, button enabled.");
    }

    public void OnStationToggled()
    {
        if (!_isFixed) return;
        if (_radioMaterial == null) return;

        float target = _stationsToggle ? _staticWhenOn : _staticWhenOff;
        SetSharpness(target);
        _stationsToggle = !_stationsToggle;
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
        SetSharpness(0.7f);
    }

    private void StopStatic()
    {
        if (!_isStaticPlaying) return;
        _staticSource.Stop();
        _staticSource.clip = null;
        _isStaticPlaying = false;
        SetSharpness(0f);
        if (!_isFixed) StartCoroutine(RestartStaticAfterDelay());
    }

    private void SetSharpness(float value)
    {
        if (_radioMaterial != null)
            _radioMaterial.SetFloat("_Sharpness", value);
    }
}