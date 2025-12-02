using UnityEngine;

public class TubePuzzleManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] lights;
    [SerializeField]
    private Interactive microwave;
    [SerializeField]
    private int numoftrue;

    private void Update()
    {
        if (lights != null)
        {
            foreach (GameObject luz in lights)
            {
                if (luz.activeSelf == true)
                {
                    numoftrue += 1;
                }
                else 
                {
                    numoftrue -= 1; //TESTE PROVAVELMENTE ERRADO 
                }
            }
        }
        if (numoftrue == lights.Length)
        {
            microwave.isOn = true;
        }
        else
        {
            microwave.isOn = false;
        }
    }
}