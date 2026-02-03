using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour 
{
    #region Champs

    // INSTANCE
    public static EventsManager Instance;

    // EVENTS
    [SerializeField] private EventsSC SC;

    public List<EventController> list = new List<EventController>();

    public Dictionary<string,UnityEvent> dico = new Dictionary<string,UnityEvent>();


    #endregion


    // ---- GLOBAL ----
    private void Awake()
    {
        // Instance
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        // dico
        dico = list.ToDictionary(x => x.Key, x => x.Event);
    }


#if UNITY_EDITOR

    FieldChangesTracker changesTracker = new FieldChangesTracker();

    private void OnValidate()
    {
        if (!changesTracker.TrackFieldChanges(this, x => x.SC))
            return;

        if (!SC)
        {
            Debug.LogError("No Scriptable Object referenced in " + this.name);
            return;
        }

        foreach (EventData e in SC.eventDatas)
        {
            if(!e.isPrefab) // event is depending on scene
                list.Add(new EventController(e.Key));
        }
    }
#endif

    // ---- EVENTS ----

    public UnityEvent FindEvent(string Key)
    {
        EventData e = SC.GetEventByKey(Key);

        // prefab event => data from sc ; scene event => data from scene
        if(e.isPrefab)
            return e.Event;
        else
            return dico[Key];
    }

    public static void DisplayName(string name)
    {
        Debug.Log("string " + name);
    }

    public void Event01()
    {
        Debug.Log("Event 01 ");
    }
}

[System.Serializable]
public class EventController
{
    //[ReadOnly]
    public string Key; 
    public UnityEvent Event;

    public EventController(string key)
    {
        Key = key;
    }
}
