using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[Serializable]
public class DSChoiceSaveData
{
    public string NodeID;
    [SerializeField] public string _dropDownKeyChoice;

    [field: SerializeField] public List<ConditionsSC> ConditionsSc { get; set; } = new List<ConditionsSC>();
    [field: SerializeField] public List<string> ConditionsKey { get; set; } = new List<string>();


    public void SavePortDirection(Direction direction)
    {
        _directionPort = direction;
    }
    public Direction GetPortDirection()
    {
        return _directionPort;
    }

    public Direction _directionPort;
    public void SaveDropDownKeyChoice(string key) => _dropDownKeyChoice = key;
    public string GetDropDownKeyChoice() => _dropDownKeyChoice;
}

