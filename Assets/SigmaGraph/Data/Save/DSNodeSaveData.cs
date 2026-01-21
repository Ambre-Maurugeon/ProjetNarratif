using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[Serializable]
public class DSNodeSaveData
{
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public Espeaker Speaker { get; set; }

    [field: SerializeField] public bubleType BubleType;
    
    
    public void SetBubleType(bubleType bubleType)
    {
        BubleType = bubleType;
    }
    public bubleType GetBubleType()
    {
        return BubleType;
    }

    [field: SerializeField] public HumeurSpeaker Humeur;
    public HumeurSpeaker GetHumeur()
    {
        return Humeur;
    }
    public void SetHumeur(HumeurSpeaker humeur)
    {
        Humeur = humeur;
    }

    public void SaveSpeaker(Espeaker speaker)
    {
        Speaker = speaker;
    }
    
    public void SaveHumeur(HumeurSpeaker humeur)
    {
        Humeur = humeur;
    }

    public bool isMultipleChoice = false;

    public Dictionary<Port, List<VisualElement>> ConditionsMapElement = new Dictionary<Port, List<VisualElement>>();
    public Dictionary<Port, List<ConditionsSC>> ConditionsMapSc = new Dictionary<Port, List<ConditionsSC>>();

    public string _dropDownKeyDialogue;

    public void SaveDropDownKeyDialogue(string key)
    {
        _dropDownKeyDialogue = key;
    }

    public string GetDropDownKeyDialogue()
    {
        return _dropDownKeyDialogue;
    }

    public void SetChoices(List<DSChoiceSaveData> choicesSaveData)
    {
        ChoicesInNode = choicesSaveData;
    }
    public void AddChoice(DSChoiceSaveData choiceSaveData)
    {
        ChoicesInNode.Add(choiceSaveData);
    }
    
    [field: SerializeField] public bool OnlyOneConditionNeeded { get; set; }
    [field: SerializeField] public List<DSChoiceSaveData> ChoicesInNode { get; private set; }
    
    [field: SerializeField] public string GroupID { get; set; }
    [field: SerializeField] public DSDialogueType DialogueType { get; set; }
    [field: SerializeField] public Vector2 Position { get; set; }
}
