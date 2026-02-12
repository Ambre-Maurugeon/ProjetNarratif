using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;

public class EventsManager : MonoBehaviour 
{
    #region Champs

    // INSTANCE
    public static EventsManager Instance;

    // EVENTS
    [SerializeField] private EventsSO SO;

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
        if (!changesTracker.TrackFieldChanges(this, x => x.SO))
            return;

        list.Clear();

        if (!SO)
        {
            Debug.LogError("No Scriptable Object referenced in " + this.name);
            return;
        }

        foreach (EventData e in SO.eventDatas)
        {
            if(!e.isPrefab) // event is depending on scene
                list.Add(new EventController(e.Key));
        }
    }

    [Button]
    private void Refresh()
    {
        // Add new event
        foreach (EventData e in SO.eventDatas)
        {
            if (!e.isPrefab)
            {
                bool contains = false;
                foreach (var c in list)
                {
                    if(c.Key == e.Key) {
                        contains = true; 
                        break; 
                    }
                }

                if(!contains)
                    list.Add(new EventController(e.Key));
            }
        }

        // delete old event
        for(int i = 0; i < list.Count; i++) 
        {
            bool contains = false;
            foreach (EventData e in SO.eventDatas)
            {
                if(e.Key == list[i].Key)
                {
                    contains = true;
                    break;
                }
            }

            if (!contains)
                list.Remove(list[i]);
        }
    }

#endif

    // ---- EVENTS ----

    // ReSharper disable Unity.PerformanceAnalysis
    public UnityEvent FindEvent(string Key)
    {
        EventData data = SO.GetEventByKey(Key);

        // prefab event => data from sc ; scene event => data from scene
        UnityEvent e = new UnityEvent();

        if(data.isPrefab)
            e = data.Event;
        else 
            if (dico[Key] == null)
            {
                e = null;
                Debug.LogError($"No EventData with Key {Key}");
            }
            else
                e = dico[Key];

        return e;
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
