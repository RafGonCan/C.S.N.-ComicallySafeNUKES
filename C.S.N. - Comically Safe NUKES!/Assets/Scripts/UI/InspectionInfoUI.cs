using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectionInfoUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _infoPanel;
    [SerializeField] private TextMeshProUGUI _itemNameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _notesText;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private float _fadeDuration = 0.3f;
    
    private Coroutine _fadeCoroutine;
    
    void Start()
    {
        if (_infoPanel != null)
        {
            _infoPanel.alpha = 0f;
            _infoPanel.gameObject.SetActive(false);
        }
    }
    
    public void ShowInfo(Interactive item)
    {
        if (item == null || _infoPanel == null) return;
        
        if (_itemNameText != null)
            _itemNameText.text = item.inventoryName;
        
        if (_descriptionText != null && item.interactiveData != null)
            _descriptionText.text = item.interactiveData.inspectionDescription;
        
        if (_notesText != null && item.interactiveData != null)
            _notesText.text = item.interactiveData.inspectionNotes;
        
        if (_itemIcon != null)
        {
            _itemIcon.sprite = item.inventoryIcon;
            _itemIcon.enabled = item.inventoryIcon != null;
        }
        
        FadePanel(true);
    }
    
    public void HideInfo()
    {
        FadePanel(false);
    }
    
    private void FadePanel(bool show)
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        
        _fadeCoroutine = StartCoroutine(FadeRoutine(show));
    }
    
    private IEnumerator FadeRoutine(bool show)
    {
        float startAlpha = _infoPanel.alpha;
        float targetAlpha = show ? 1f : 0f;
        
        if (show && !_infoPanel.gameObject.activeSelf)
            _infoPanel.gameObject.SetActive(true);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _fadeDuration);
            
            _infoPanel.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        _infoPanel.alpha = targetAlpha;
        
        if (!show && _infoPanel.gameObject.activeSelf)
            _infoPanel.gameObject.SetActive(false);
        
        _fadeCoroutine = null;
    }
}