using UnityEngine;

public class TestEvent : MonoBehaviour
{
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
        Debug.Log("move mantis");

        _mantisAnimator.SetTrigger("Move");

    }
}
