using UnityEngine;

public class RadioStationShaderController : MonoBehaviour
{
    [SerializeField] private AudioStations _audioStations;
    [SerializeField] private Material _radioMaterial;
    [SerializeField] private int _silentStationIndex = 0;
    [SerializeField] private float _activeSharpness = 0.7f;

    public void UpdateSharpness()
    {
        if (_audioStations == null || _radioMaterial == null) return;

        int index = _audioStations.CurrentClipIndex;
        if (index == _silentStationIndex)
            _radioMaterial.SetFloat("_Sharpness", 0.2f);
        else
            _radioMaterial.SetFloat("_Sharpness", _activeSharpness);
    }
}