using UnityEngine;

public abstract class StatefulInteractive : MonoBehaviour
{
    [System.Serializable]
    public class ItemState
    {
        public string stateName;
        public bool[] partStates;
    }

    [SerializeField] protected GameObject _inspectModelPrefab;
    [SerializeField] protected GameObject[] _toggleableParts;
    
    protected ItemState _currentState;
    
    public GameObject inspectModelPrefab => _inspectModelPrefab;
    
    public virtual ItemState GetCurrentState()
    {
        ItemState state = new ItemState();
        state.stateName = "Current State";
        
        if (_toggleableParts != null)
        {
            state.partStates = new bool[_toggleableParts.Length];
            
            for (int i = 0; i < _toggleableParts.Length; i++)
            {
                if (_toggleableParts[i] != null)
                {
                    state.partStates[i] = _toggleableParts[i].activeSelf;
                }
            }
        }
        
        return state;
    }
    
    public virtual void ApplyState(ItemState state)
    {
        if (state == null || _toggleableParts == null) return;
        
        int minLength = Mathf.Min(state.partStates.Length, _toggleableParts.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (_toggleableParts[i] != null)
            {
                _toggleableParts[i].SetActive(state.partStates[i]);
            }
        }
        
        _currentState = state;
    }
    
    public virtual void TogglePart(int partIndex)
    {
        if (partIndex >= 0 && partIndex < _toggleableParts.Length && 
            _toggleableParts[partIndex] != null)
        {
            bool newState = !_toggleableParts[partIndex].activeSelf;
            _toggleableParts[partIndex].SetActive(newState);
            
            if (_currentState != null && partIndex < _currentState.partStates.Length)
            {
                _currentState.partStates[partIndex] = newState;
            }
            
            Debug.Log($"Toggled part {partIndex} to: {newState}");
        }
    }
    
    // NEW: Check if a GameObject is one of our toggleable parts
    public bool IsToggleablePart(GameObject obj, out int partIndex)
    {
        partIndex = -1;
        
        if (_toggleableParts == null || obj == null) return false;
        
        for (int i = 0; i < _toggleableParts.Length; i++)
        {
            if (_toggleableParts[i] == obj)
            {
                partIndex = i;
                return true;
            }
        }
        
        return false;
    }
    
    public virtual void SetupForInspection()
    {

        for (int i = 0; i < _toggleableParts.Length; i++)
        {
            if (_toggleableParts[i] != null)
            {

                if (_toggleableParts[i].GetComponent<Collider>() == null)
                {
                    BoxCollider collider = _toggleableParts[i].AddComponent<BoxCollider>();
                    collider.isTrigger = true;
                    collider.size = Vector3.one * 0.1f;
                }
            }
        }
    }
    
    public virtual void CleanupAfterInspection()
    {
        
    }
}