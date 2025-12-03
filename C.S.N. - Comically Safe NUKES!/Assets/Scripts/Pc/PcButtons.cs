using UnityEngine;

public class PcButtons : Interactive
{
    [SerializeField] private GameObject[] targetObjects;
    [SerializeField] private int numoftrue;
    [SerializeField] private Interactive microwave;

    protected override void InteractSelf(bool direct)
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
                    foreach (GameObject luz in targetObjects)
                    {
                        if (luz.activeSelf == true)
                        {
                            numoftrue += 1;
                        }
                        else
                        {
                            numoftrue = 0; //TESTE PROVAVELMENTE ERRADO 
                        }
                    }
                if (numoftrue == targetObjects.Length)
                {
                    microwave.isOn = true;
                }
                else
                {
                    microwave.isOn = false;
                }
            }
        }
        base.InteractSelf(direct);
    }
}