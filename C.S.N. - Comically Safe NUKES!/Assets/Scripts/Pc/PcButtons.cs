using UnityEngine;

public class PcButtons : Interactive
{
    [SerializeField] private GameObject[] targetObjects;
    [SerializeField] private GameObject[] allPipes;
    [SerializeField] private Interactive microwave;

    protected override void InteractSelf(bool direct)
    {
        foreach (GameObject obj in targetObjects)
            if (obj != null) obj.SetActive(!obj.activeSelf);

        int activeCount = 0;
        foreach (GameObject obj in allPipes)
            if (obj != null && obj.activeSelf) activeCount++;

        if (microwave != null)
            microwave.isOn = (activeCount == allPipes.Length);

        base.InteractSelf(direct);
    }
}