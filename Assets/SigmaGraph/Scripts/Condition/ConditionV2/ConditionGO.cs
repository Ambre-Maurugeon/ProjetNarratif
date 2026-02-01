using System;
using UnityEngine;
using UnityEngine.Events;

public class ConditionGO : MonoBehaviour
{
    public ConditionData ConditionData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Debug.Log(ConditionData.checkEvent.Equals(true)) ;
        //ConditionData.checkEvent.Invoke("test",0);
        Debug.Log(ConditionData.checkEvent.func.Invoke());
        //Debug.Log(ConditionData.checkEvent.Invoke);
    }
}
