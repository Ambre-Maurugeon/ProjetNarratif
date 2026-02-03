using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EventData
{
    public string label;
    public string Key;
    public string caption;

    [Tooltip("Is depending on the scene")]
    public bool isPrefab;

    [ShowIf("isPrefab")]
    public UnityEvent Event;

}
