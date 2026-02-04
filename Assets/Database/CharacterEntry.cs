using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterEntry
{
    public string perso;        
    public int id;
    public bool isCompleted = false;
    public DSGraphSaveDataSO refDialogue;
    public int NewSpeedBPM;
    public Sprite characterSprite;
    public string[] description;
}