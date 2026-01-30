using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventData
{
    public string label;
    public string Key;
    public string caption;
    public UnityEvent Event;

    [Tooltip("Depends on the scene")]
    public bool isPrefab;
}
