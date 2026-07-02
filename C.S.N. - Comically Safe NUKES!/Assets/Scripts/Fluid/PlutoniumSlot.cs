using UnityEngine;

public class PlutoniumSlotListener : MonoBehaviour
{
    [SerializeField] private Interactive slot;
    [SerializeField] private FluidScale fluidScale;

    private void Awake()
    {
        if (slot == null)
            slot = GetComponent<Interactive>();

        slot.onRequirementUsed.AddListener(_ => fluidScale.ReceivePlutonium());
    }
}