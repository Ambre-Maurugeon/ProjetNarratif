using System;
using UnityEngine;

[Serializable]
public struct Sequence
{
    public Sprite[] sprites;
    public int framesPerSprite;
    public bool loop;
    public bool destroyOnEnd;
}

[System.Serializable]
public class CharacterEntry
{
    public string perso;
    public int id;
    public bool isCompleted = false;
    public DSGraphSaveDataSO refDialogue;
    public int NewSpeedBPM;
    public Sprite characterSprite;
    public string[] description = new string[3];
    public Sprite BgSprite;

    public Sequence[] Sequences;

    public int sequenceIndex = 0;
}