using UnityEngine;

public class Touch : MonoBehaviour
{
    private static Event OnTouchScreen;

    private Vector3 position;
    private float width;
    private float height;


    private void Awake()
    {
        width = Screen.width / 2.0f;
        height = Screen.height / 2.0f;

        position = new Vector3(0.0f, 0.0f, 0.0f);

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}



