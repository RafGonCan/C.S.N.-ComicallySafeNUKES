using UnityEngine;

public class PcButtons : Interactive
{
    [SerializeField] private GameObject[] targetObjects;

    protected override void InteractSelf(bool direct)
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
        base.InteractSelf(direct);
    }
}