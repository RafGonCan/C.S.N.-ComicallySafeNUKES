using UnityEngine;

public class RadioStationShaderController : MonoBehaviour
{
    [SerializeField] private AudioStations _audioStations;
    [SerializeField] private Material _radioMaterial;
    [SerializeField] private float _activeSharpness = 0.7f;

    public void UpdateSharpness()
    {
        if (_audioStations == null || _radioMaterial == null) return;

        if (!_audioStations.audioOn)
            _radioMaterial.SetFloat("_Sharpness", 0.2f);
        else
            _radioMaterial.SetFloat("_Sharpness", _activeSharpness);
    }
}