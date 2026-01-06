using UnityEngine;

public class Manual_Menu : MonoBehaviour
{
    [SerializeField] private GameObject manualMenu;
    private bool _isManualOpen = true;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            if (_isManualOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }
    }
    public void Open()
    {
        _isManualOpen = true;
        manualMenu.SetActive(true);
    }
    public void Close()
    {
        _isManualOpen = false;
        manualMenu.SetActive(false);
    }
}
