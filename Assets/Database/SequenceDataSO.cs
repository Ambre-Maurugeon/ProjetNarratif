using UnityEngine;

[CreateAssetMenu(fileName = "SequenceData", menuName = "Animation/SequenceData")]
public class SequenceDataSO : ScriptableObject
{
    public Sprite[] sprites;
    public int framesPerSprite = 6;
    public bool loop = true;
    public bool destroyOnEnd = false;
    
}