using System;
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;
using UnityEditor.MPE;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Events", menuName = "Scriptable Objects/EventsSC")]
public class EventsSO : ScriptableObject
{
    public List<EventData> eventDatas = new List<EventData>();

    public EventData GetEventByKey(string Key)
    {
        foreach (var Data in eventDatas)
            if (Data.Key == Key) return Data;

        return null;
    }

    public void FillEventDropdown(ref DropdownField dropdownField)
    {
        dropdownField.choices.Clear();

        List<EventData> list = eventDatas;

        foreach (EventData Data in list)
            dropdownField.choices.Add(Data.Key);
    }
}
