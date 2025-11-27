using UnityEngine;

public class PcButtons : MonoBehaviour
{
    [SerializeField] private GameObject[] targetObjects;

    private void OnMouseDown()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }
}