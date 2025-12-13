using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject     _interactionCrosshair;
    [SerializeField] private GameObject     _interactionPanel;
    [SerializeField] private GameObject     _inventorySlotsContainer;
    [SerializeField] private GameObject     _inventoryIconsContainer;
    [SerializeField] private int            _defaultCrosshairScale;
    [SerializeField] private int            _interactionCrosshairScale;
    [SerializeField] private Color          _unselectedSlotColor;
    [SerializeField] private Color          _selectedSlotColor;
    [SerializeField] private RectTransform _topBlackBar;
    [SerializeField] private RectTransform _bottomBlackBar;
    [SerializeField] private float         _barAnimationDuration = 0.3f;
    [SerializeField] private InspectionInfoUI _inspectionInfoUI;
    private Vector3         _topBarVisiblePos;
    private Vector3         _bottomBarVisiblePos;
    private float           _barMoveDistance = 200f;
    private Coroutine       _barAnimationCoroutine;
    private TextMeshProUGUI _interactionMessage;
    private Image[]         _inventorySlots;
    private Image[]         _inventoryIcons;
    private int             _selectedSlotIndex;

    void Start()
    {
        _interactionMessage = GetComponentInChildren<TextMeshProUGUI>();
        _inventorySlots = _inventorySlotsContainer.GetComponentsInChildren<Image>();
        _inventoryIcons = _inventoryIconsContainer.GetComponentsInChildren<Image>();
        _selectedSlotIndex = -1;

        HideCursor();
        HideInteractionPanel();
        HideInventoryIcons();
        ResetInventorySlots();

        if (_topBlackBar != null)
            _topBlackBar.gameObject.SetActive(false);
        if (_bottomBlackBar != null)
            _bottomBlackBar.gameObject.SetActive(false);
        if (_topBlackBar != null)
        {
        _topBarVisiblePos = _topBlackBar.localPosition;
        _topBlackBar.localPosition = new Vector3(_topBarVisiblePos.x, _topBarVisiblePos.y - _barMoveDistance, _topBarVisiblePos.z);
        _topBlackBar.gameObject.SetActive(false);
        }
        if (_bottomBlackBar != null)
        {
            _bottomBarVisiblePos = _bottomBlackBar.localPosition;
            _bottomBlackBar.localPosition = new Vector3(_bottomBarVisiblePos.x, _bottomBarVisiblePos.y + _barMoveDistance, _bottomBarVisiblePos.z);
            _bottomBlackBar.gameObject.SetActive(false);
        }
    }
    void OnEnable()
    {
        InteractionManager.OnInspectionStateChanged += HandleInspectionStateChanged;
    }
    void OnDisable()
    {
        InteractionManager.OnInspectionStateChanged -= HandleInspectionStateChanged;
    }
    void HandleInspectionStateChanged(bool isInspecting, Interactive item)
    {
        ShowInspectionBars(isInspecting);
        
        if (isInspecting && item != null && _inspectionInfoUI != null)
        {
            _inspectionInfoUI.ShowInfo(item);
        }
        else if (!isInspecting && _inspectionInfoUI != null)
        {
            _inspectionInfoUI.HideInfo();
        }
    }
    void HandleInspectionStarted(Interactive item)
    {
        if (_inspectionInfoUI != null && item != null)
        {
            _inspectionInfoUI.ShowInfo(item);
        }
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowDefaultCrosshair()
    {
        _interactionCrosshair.transform.localScale = new Vector3(_defaultCrosshairScale, _defaultCrosshairScale, _defaultCrosshairScale);
    }
    
    public void ShowInteractionCrosshair()
    {
        _interactionCrosshair.transform.localScale = new Vector3(_interactionCrosshairScale, _interactionCrosshairScale, _interactionCrosshairScale);
    }

    public void HideInteractionPanel()
    {
        _interactionPanel.SetActive(false);
    }

    public void ShowInteractionPanel(string message)
    {
        _interactionMessage.text = message;
        _interactionPanel.SetActive(true);
    }

    public int GetInventorySlotCount()
    {
        return _inventorySlots.Length;
    }

    public void HideInventoryIcons()
    {
        foreach (Image image in _inventoryIcons)
            image.enabled = false;
    }

    private void ResetInventorySlots()
    {
        foreach (Image image in _inventorySlots)
            image.color = _unselectedSlotColor;
    }

    public void ShowInventoryIcon(int index, Sprite icon)
    {
        _inventoryIcons[index].sprite   = icon;
        _inventoryIcons[index].enabled  = true;
    }

    public void SelectInventorySlot(int index)
    {
        if (_selectedSlotIndex != -1)
            _inventorySlots[_selectedSlotIndex].color = _unselectedSlotColor;

        if (index != -1)
        {
            _inventorySlots[index].color = _selectedSlotColor;
            _selectedSlotIndex = index;
        }
    }
    public void ShowInspectionBars(bool show)
    {
        if (_barAnimationCoroutine != null)
            StopCoroutine(_barAnimationCoroutine);
        
        _barAnimationCoroutine = StartCoroutine(AnimateBlackBars(show));
    }
    private IEnumerator AnimateBlackBars(bool show)
    {
        if (show)
        {
            if (_topBlackBar != null) _topBlackBar.gameObject.SetActive(true);
            if (_bottomBlackBar != null) _bottomBlackBar.gameObject.SetActive(true);
        }
        float elapsedTime = 0f;
        
        Vector3 topCurrentPos = _topBlackBar != null ? _topBlackBar.localPosition : Vector3.zero;
        Vector3 bottomCurrentPos = _bottomBlackBar != null ? _bottomBlackBar.localPosition : Vector3.zero;
        
        Vector3 topTargetPos = show ? _topBarVisiblePos : new Vector3(_topBarVisiblePos.x, _topBarVisiblePos.y - 
        _barMoveDistance, _topBarVisiblePos.z);
        
        Vector3 bottomTargetPos = show ? _bottomBarVisiblePos : new Vector3(_bottomBarVisiblePos.x,  _bottomBarVisiblePos.y +
         _barMoveDistance, _bottomBarVisiblePos.z);
        
        while (elapsedTime < _barAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _barAnimationDuration);
            
            t = t * t * (3f - 2f * t);
            
            if (_topBlackBar != null)
                _topBlackBar.localPosition = Vector3.Lerp(topCurrentPos, topTargetPos, t);
            
            if (_bottomBlackBar != null)
                _bottomBlackBar.localPosition = Vector3.Lerp(bottomCurrentPos, bottomTargetPos, t);
            
            yield return null;
        }
        if (_topBlackBar != null) _topBlackBar.localPosition = topTargetPos;
        if (_bottomBlackBar != null) _bottomBlackBar.localPosition = bottomTargetPos;
        if (!show)
        {
            if (_topBlackBar != null) _topBlackBar.gameObject.SetActive(false);
            if (_bottomBlackBar != null) _bottomBlackBar.gameObject.SetActive(false);
        }
        
        _barAnimationCoroutine = null;
    }
}