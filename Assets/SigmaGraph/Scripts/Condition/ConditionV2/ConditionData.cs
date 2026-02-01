using System;

[Serializable]
public class ConditionData
{
    public System.Object objet;

    public CheckEvent checkEvent;

}

[Serializable] public class CheckEvent : SerializableCallback<string, int, bool> { }



