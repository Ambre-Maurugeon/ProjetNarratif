using UnityEngine;

public class TestEvent : MonoBehaviour
{
    //[SerializeField] DialogueManager dialogueManager;

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

    //private void OnEnable()
    //{
    //    dialogueManager.OnDialogueEnd += OnEnd;
    //}

    //private void OnDisable()
    //{
    //    dialogueManager.OnDialogueEnd -= OnEnd;
    //}

    //private void OnEnd()
    //{
    //    Debug.Log("help");
    //}
}
