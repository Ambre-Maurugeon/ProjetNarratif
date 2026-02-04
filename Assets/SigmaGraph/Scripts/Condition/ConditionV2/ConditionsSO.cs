using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Conditions", menuName = "Scriptable Objects/KeyConditions")] 
public class ConditionsSO : ScriptableObject
{
    public List<ConditionData> conditionDatas = new List<ConditionData>();

    public void FillConditionDropdown(ref DropdownField dropdownField)
    {
        dropdownField.choices.Clear();

        List<ConditionData> list = conditionDatas;

        foreach (ConditionData Data in list)
            dropdownField.choices.Add(Data.Key);
    }
}
