using UnityEngine;

[CreateAssetMenu(fileName = "InteractiveData", menuName = "Scriptable Objects/InteractiveData")]
public class InteractiveData : ScriptableObject
{
    public enum Type { Pickable, InteractOnce, InteractMulti, Indirect, Focusable};

    public Type                 type;
    public bool                 startsOn = true;
    public string               inventoryName;
    public Sprite               inventoryIcon;
    public InteractiveData[]    requirements;
    public string               requirementsMessage;
    public string[]             interactionMessages;
    public GameObject           inspectModel;
    
    [TextArea(3, 5)]
    public string               inspectionDescription = "";
}