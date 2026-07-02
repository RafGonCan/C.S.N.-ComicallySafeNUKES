using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _interactionCrosshair;
    [SerializeField] private GameObject _indirectInteractionCrosshair;
    [SerializeField] private GameObject _interactionPanel;
    [SerializeField] private GameObject _inventorySlotsContainer;
    [SerializeField] private GameObject _inventoryIconsContainer;
    [SerializeField] private int _defaultCrosshairScale;
    [SerializeField] private int _interactionCrosshairScale;
    [SerializeField] private Color _unselectedSlotColor;
    [SerializeField] private Color _selectedSlotColor;
    [SerializeField] private RectTransform _topBlackBar;
    [SerializeField] private RectTransform _bottomBlackBar;
    [SerializeField] private float _barAnimationDuration = 0.3f;
    [SerializeField] private InspectionInfoUI _inspectionInfoUI;
    [SerializeField] private GameObject _inspectionReminer;

    [Header("Inspection Reminder Animation")]
    [SerializeField] private float _reminderFadeDuration = 0.5f;
    [SerializeField] private float _reminderGlowDuration = 3.0f;
    [SerializeField] private Color _reminderGlowColor = new Color(0f, 0.5f, 0f);

    private Vector3 _topBarVisiblePos;
    private Vector3 _bottomBarVisiblePos;
    private float _barMoveDistance = 200f;
    private Coroutine _barAnimationCoroutine;
    private TextMeshProUGUI _interactionMessage;
    private Image[] _inventorySlots;
    private Image[] _inventoryIcons;
    private int _selectedSlotIndex;

    private CanvasGroup _reminderCanvasGroup;
    private TextMeshProUGUI _reminderText;
    private Image _reminderImage;
    private Coroutine _reminderCoroutine;
    private Color _originalReminderColor;

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

        // Ensure crosshair states start correctly
        _interactionCrosshair.SetActive(true);
        _indirectInteractionCrosshair.SetActive(false);
        ShowDefaultCrosshair();

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

        if (_inspectionReminer != null)
        {
            _reminderCanvasGroup = _inspectionReminer.GetComponent<CanvasGroup>();
            if (_reminderCanvasGroup == null)
                _reminderCanvasGroup = _inspectionReminer.AddComponent<CanvasGroup>();

            _reminderText = _inspectionReminer.GetComponentInChildren<TextMeshProUGUI>();
            if (_reminderText == null)
                _reminderImage = _inspectionReminer.GetComponentInChildren<Image>();

            if (_reminderText != null)
                _originalReminderColor = _reminderText.color;
            else if (_reminderImage != null)
                _originalReminderColor = _reminderImage.color;
            else
                _originalReminderColor = Color.white;

            _reminderCanvasGroup.alpha = 0f;
            _inspectionReminer.SetActive(false);
        }
    }

    void OnEnable()
    {
        InspectionTool.OnInspectionStateChanged += HandleInspectionStateChanged;
    }

    void OnDisable()
    {
        InspectionTool.OnInspectionStateChanged -= HandleInspectionStateChanged;
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
        _indirectInteractionCrosshair.SetActive(false);
        _interactionCrosshair.SetActive(true);
        _interactionCrosshair.transform.localScale = new Vector3(_defaultCrosshairScale, _defaultCrosshairScale, _defaultCrosshairScale);
    }

    public void ShowInteractionCrosshair()
    {
        _indirectInteractionCrosshair.SetActive(false);
        _interactionCrosshair.SetActive(true);
        _interactionCrosshair.transform.localScale = new Vector3(_interactionCrosshairScale, _interactionCrosshairScale, _interactionCrosshairScale);
    }

    public void ShowIndirectInteractionCrosshair()
    {
        _interactionCrosshair.SetActive(false);
        _indirectInteractionCrosshair.SetActive(true);
        _indirectInteractionCrosshair.transform.localScale = new Vector3(_interactionCrosshairScale, _interactionCrosshairScale, _interactionCrosshairScale);
    }
    public void TriggerInspectionReminder()
    {
        if (_reminderCoroutine != null)
            StopCoroutine(_reminderCoroutine);

        _reminderCoroutine = StartCoroutine(AnimateInspectionReminder());
    }

    private IEnumerator AnimateInspectionReminder()
    {
        if (_inspectionReminer == null) yield break;

        _inspectionReminer.SetActive(true);
        _reminderCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < _reminderFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _reminderFadeDuration;
            _reminderCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        _reminderCanvasGroup.alpha = 1f;

        elapsed = 0f;
        while (elapsed < _reminderGlowDuration)
        {
            elapsed += Time.deltaTime;

            float pulse = Mathf.PingPong(elapsed * 2f, 1f);

            Color currentColor = Color.Lerp(_originalReminderColor, _reminderGlowColor, pulse * 0.6f);

            if (_reminderText != null)
                _reminderText.color = currentColor;
            else if (_reminderImage != null)
                _reminderImage.color = currentColor;

            float scale = 1f + (pulse * 0.05f);
            _inspectionReminer.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        if (_reminderText != null)
            _reminderText.color = _originalReminderColor;
        else if (_reminderImage != null)
            _reminderImage.color = _originalReminderColor;

        _inspectionReminer.transform.localScale = Vector3.one;

        elapsed = 0f;
        while (elapsed < _reminderFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _reminderFadeDuration;
            _reminderCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        _reminderCanvasGroup.alpha = 0f;
        _inspectionReminer.SetActive(false);
        _reminderCoroutine = null;
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
        _inventoryIcons[index].sprite = icon;
        _inventoryIcons[index].enabled = true;
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

        Vector3 topTargetPos = show ? _topBarVisiblePos : new Vector3(_topBarVisiblePos.x, _topBarVisiblePos.y - _barMoveDistance, _topBarVisiblePos.z);
        Vector3 bottomTargetPos = show ? _bottomBarVisiblePos : new Vector3(_bottomBarVisiblePos.x, _bottomBarVisiblePos.y + _barMoveDistance, _bottomBarVisiblePos.z);

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