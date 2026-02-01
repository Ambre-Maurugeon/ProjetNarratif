using UnityEngine;

public class TestEvent : MonoBehaviour
{
    public static void testPrefab()
    {
        Debug.Log("test prefab");
    }

    public void testNoPrefab()
    {
        Debug.Log("test no prefab");
    }

    public bool ConditionBoolTest()
    {
        return false;
    }
}
