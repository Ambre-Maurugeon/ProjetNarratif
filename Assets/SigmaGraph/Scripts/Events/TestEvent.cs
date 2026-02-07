using System.Collections.Generic;
using NaughtyAttributes;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class TestEvent : MonoBehaviour
{
    [Header("Anims")]
    [SerializeField] Animator _mantisAnimator;
    [SerializeField] Animator _speakerAnimator;
    

    public static void testPrefab()
    {
        Debug.Log("test prefab");
    }

    public void testNoPrefab()
    {
        Debug.Log("test no prefab");
    }

    public bool ConditionTest(bool value)
    {
        return value;
    }

    public void MoveMantis()
    {
        _mantisAnimator.SetTrigger("Move");

    }

    public void HideMantis()
    {
        _mantisAnimator.SetTrigger("Remove");

    }


}
