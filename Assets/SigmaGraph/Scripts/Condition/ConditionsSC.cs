using UnityEngine;

[CreateAssetMenu(fileName = "Conditions", menuName = "Scriptable Objects/Conditions")]
public class ConditionsSC : ScriptableObject
{
    public Items conditionItem;
    public int conditionValue;
}


public enum Items
{
    Key,
    Coins,
}