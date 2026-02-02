using System;
using System.Collections.Generic;
using System.Reflection;
using NaughtyAttributes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static Arg;

public class ConditionsManager : MonoBehaviour
{
    //Instance
    public static ConditionsManager instance;

    public ConditionsSO SO;

    public List<ConditionController> list = new();



    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

#if UNITY_EDITOR

    FieldChangesTracker changesTracker = new FieldChangesTracker();

    // TO EDIT use dropdown and auto fill at any change
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

        foreach (var c in SO.conditionDatas)
        {
           list.Add(new ConditionController(c.Key));
        }
    }
#endif

    public bool EvaluateCondition(string key)
    {
        CheckEvent checkEvent = GetConditionByKey(key);

        if (checkEvent.func == null)
        {
            checkEvent.func = checkEvent.GetPersistentMethod();// TO EDIT initially a protected method 
        }

        bool value = checkEvent.func.Invoke(checkEvent.Args);

        return value;
    }

    private CheckEvent GetConditionByKey(string key)
    {
        CheckEvent checkEvent = new CheckEvent();

        foreach (var condition in list)
            if (condition.Key == key)
            {
                checkEvent = condition.check;
                break;
            }

        return checkEvent;
    }


}

[System.Serializable]
public class ConditionController
{
    public string Key;
    public CheckEvent check;

    public ConditionController(string key)
    {
        Key = key;
    }
}
