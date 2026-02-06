using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.Events;

public enum language
{
    FR,
    EN,
}
public enum bubleType
{
    NORMAL,
    THINK,
    SHOUT,
}


public class DialogueManager : MonoBehaviour
{
    #region Fields

    public DSGraphSaveDataSO runtimeGraph;
    
    [Header("PLAYER SETTINGS")]
    
    [SerializeField] private language languageSetting = language.FR;

    private language _previewLanguage;

    [Header("UI Elements")]
    
    private Dictionary<bubleType, dialogueContainer> _bubleContainers = new Dictionary<bubleType, dialogueContainer>();
    [SerializeField] private List<dialogueContainer> _bubleContainerList = new List<dialogueContainer>();

    [Header("UI Buttons")] 
    public Button ChoiceButtonPrefab;
    public Transform ChoiceButtonContainer;
    public Button _nextButton;

    [Header("Speakers")]
    [SerializeField] private Image _speakerImage;
    public Speakers SpeakersScriptable;
    private SpeakerInfo _currentSpeaker;

    private Dictionary<string, DSNodeSaveData> _nodeLookup = new Dictionary<string, DSNodeSaveData>();
    private DSNodeSaveData _currentNode;
    
    private bool _isWaitingForChoice = false;
    
    private dialogueContainer _currentDialogueContainer;
    private dialogueContainer _oldDialogueContainer;

    // Interaction

    private bool _canInteract = true;
    public bool CanInteract              
    {                                    
        get => _canInteract;             
        set                              
        {                                
            _canInteract = value;        
        }                                
    }                                    

    // end Dialogue
    public delegate void MyDelegate();
    public static event MyDelegate OnDialogueEnd;

    #endregion

    [Button]
    public void LoadCsv()
    {
        FantasyDialogueTable.Load();
    }

#if UNITY_EDITOR
    // POUR METTRE A JOUR LE DIALOGUE ET LES TEXTES SI UNE NOUVELLE LANGUE EST CHOISIE //
    private void OnValidate()
    {
        if (_previewLanguage != languageSetting)
        {
            _previewLanguage = languageSetting;
            UpdateLanguageSetting(languageSetting);
        }
    }
#endif
    
    
    // MET A JOUR LA LANGUE DU DIALOGUE //
    public void UpdateLanguageSetting(language newLanguage)
    {
        languageSetting = newLanguage;
        if (_isWaitingForChoice)
        {
            foreach (Transform child in ChoiceButtonContainer)
            {
                Destroy(child.gameObject);
            }
        }
        UpdateDialogueFromNode(_currentNode);
    }
    
    private void Awake()
    {
        // ON FAIT CA EN BRUT PRCQ NSM PAS LE TEMPS // Courage pour le projet UNITY MOBILE je vous aime tous <3 //
        
        _bubleContainers.Add(bubleType.NORMAL, _bubleContainerList[0]);
        _bubleContainers.Add(bubleType.THINK, _bubleContainerList[1]);
        _bubleContainers.Add(bubleType.SHOUT, _bubleContainerList[2]);
    }

    private void Start()
    {
        _previewLanguage = languageSetting;
        FantasyDialogueTable.Load();
        foreach (var node in runtimeGraph.Nodes)
        {
            _nodeLookup[node.ID] = node;
        }

        // GET NODE LIFE CYCLE
        _currentNode = GetNodeStart();
        //////////////////////
        
        if (_currentNode == null)
        {
            EndDialogue();
            return;
        }
        else
        {
            TryToUpdateNextDialogueFromNextNode();
        }

        // Next Button
        if (_nextButton != null)
        {
            _nextButton.onClick.AddListener(() =>
            {
             if(CanInteract && !_isWaitingForChoice)
                TryToUpdateNextDialogueFromNextNode();
            });
        }
    }
    
    private DSNodeSaveData GetNextNode(string nextID)
    {
        if (!string.IsNullOrEmpty(nextID) && _nodeLookup.TryGetValue(nextID, out var node))
        {
            return node;
        }
        return null;
    }

    private void Update()
    {
        //// ON CLICK //
        //if (Input.GetMouseButtonDown(0) && !_isWaitingForChoice)
        //{
        //    TryToUpdateNextDialogueFromNextNode();
        //}
    }

    
    // MET A JOUR LE DIALOGUE EN FONCTION DU NODE SUIVANT //
    private void TryToUpdateNextDialogueFromNextNode()
    {
        // CHECK SI NEXT NODE EXISTE //
        if (_currentNode == null)
        {
            return;
        }

        DSNodeSaveData nextNode = new DSNodeSaveData();
        
        switch (_currentNode.DialogueType) // QUEL TYPE DE NODE ON EST ACUTELLEMENT //
        {
            case DSDialogueType.Start:
                nextNode = GetNextNode(_currentNode.ChoicesInNode[0].NodeID);
                break;
            case DSDialogueType.End:
                EndDialogue();
                return;
            case DSDialogueType.Branch:
                nextNode = GetCorrectNextNodeFromBranch();
                break;
            case DSDialogueType.MultipleChoice:
                nextNode = GetNextNode(_currentNode.ChoicesInNode[0].NodeID);
                break;
        }
        
        if (nextNode == null)
        {
            Debug.Log("Next Node is null. Ending dialogue.");
            EndDialogue();
            return;
        }
        UpdateDialogueFromNode(nextNode);
    }
    
    // GERE LES CONDITIONS DU BRANCH // RETOURNE LE BON NODE EN FONCTION DES CONDITIONS //s
    private DSNodeSaveData GetCorrectNextNodeFromBranch()
    {
        //Debug.Log("Evaluating branch conditions for Node ID: " + _currentNode.ID);
        // SI Y'A PAS DE CONDITIONS DANS UN IF ON SKIP // NORMALEMENT CA DEVRAIT JAMAIS ARRIVER MDR//
        if(_currentNode.ChoicesInNode[0].ConditionsKey.Count <= 0)
        {
            Debug.Log("No choices available in the current branch node.");
            return GetNextNode(_currentNode.ChoicesInNode[1].NodeID);
        }
        
        bool hasMetConditions = false;

        foreach (var choice in _currentNode.ChoicesInNode[0].ConditionsKey)
        {
            if (_currentNode.OnlyOneConditionNeeded)
            {
                if (DoesFillKeyConditions(choice))
                {
                    hasMetConditions = true;
                    // ON RECUP LE [1] CAR C'EST LE TRUE //
                    break;
                }
                continue;
            }
            
            if (!DoesFillKeyConditions(choice))
            {
                hasMetConditions = false;
                break;
            }
            hasMetConditions = true;
        }

        if (hasMetConditions)
        {
            // ON RECUP LE [1] CAR C'EST LE TRUE //
            return GetNextNode(_currentNode.ChoicesInNode[1].NodeID);
        }
        // ON RECUP LE [2] CAR C'EST LE FALSE //
        return GetNextNode(_currentNode.ChoicesInNode[2].NodeID);
    }

    // MET A JOUR LE DIALOGUE EN FONCTION DU NODE DONNE //
    private void UpdateDialogueFromNode(DSNodeSaveData node)
    {
        _currentNode = node;

        if (_currentNode == null)
        {
            EndDialogue();
            return;
        }
        
        switch (_currentNode.DialogueType) // QUEL TYPE DE NODE ON EST ACUTELLEMENT //
        {
            case DSDialogueType.Start:
                // Normalement on devrait jamais y arriver mdrr // Mais au cas où //
                Debug.Log("Current Node is of type Start, moving to next node.");
                UpdateDialogueFromNode(GetNextNode(_currentNode.ChoicesInNode[0].NodeID));
                break;
            case DSDialogueType.End:
                EndDialogue();
                return;
            case DSDialogueType.Branch:
                TryToUpdateNextDialogueFromNextNode();
                break;
            case DSDialogueType.MultipleChoice:
                CreateButtonsChoice();
                break;
        }

        if (_bubleContainers.TryGetValue(_currentNode.GetBubleType(), out var container))
        {
            _currentDialogueContainer = container;
        }
        else
        {
            return;
        }

        if (_currentDialogueContainer == null)
        {
            Debug.Log("Current Dialogue Container is null.");
            return;
        }
        
        if(_oldDialogueContainer != null)
        {
            _oldDialogueContainer.HideContainer();
        }
        _oldDialogueContainer = _currentDialogueContainer;

        ChangeSpeaker(_currentNode.Speaker);
        if(_currentSpeaker == null)
        {
            Debug.Log("Current Speaker is null.");
            return;
        }
        
        string targetDialogue = FantasyDialogueTable.LocalManager.FindDialogue(_currentNode.GetDropDownKeyDialogue(), Enum.GetName(typeof(language), languageSetting));
        _currentDialogueContainer.InitializeDialogueContainer(targetDialogue, _currentSpeaker.Name, _currentSpeaker.GetSpriteForHumeur(_currentNode.GetHumeur()));

        if(_speakerImage!=null)
            _speakerImage.sprite = _currentSpeaker.GetSpriteForHumeur(_currentNode.GetHumeur());


        // event
        if (!string.IsNullOrEmpty(_currentNode.GetDropDownKeyEvent()))
        {
            UnityEvent targetEvent = EventsManager.Instance.FindEvent(_currentNode.GetDropDownKeyEvent());
            targetEvent?.Invoke();
        }


    }

    private void CreateButtonsChoice()
    {
        if (_currentNode.ChoicesInNode.Count > 1)
        {
            _isWaitingForChoice = true;
            foreach (DSChoiceSaveData choice in _currentNode.ChoicesInNode)
            {
                Button choiceButton = Instantiate(ChoiceButtonPrefab, ChoiceButtonContainer);
                buttonChoiceController buttonController = choiceButton.GetComponent<buttonChoiceController>();
                if (buttonController != null)
                {
                    bool fillCondition = true;
                    foreach (var condition in choice.ConditionsKey)
                    {
                        fillCondition = DoesFillKeyConditions(condition);
                        if (!fillCondition)
                        {
                            break;
                        }
                    }
                    string textButton = FantasyDialogueTable.LocalManager.FindDialogue(choice.GetDropDownKeyChoice(), Enum.GetName(typeof(language), languageSetting));
                    buttonController.InitializeButtonChoiceController(fillCondition, textButton);
                }
                
                choiceButton.onClick.AddListener(() =>
                {
                    Debug.Log("help");

                    _isWaitingForChoice = false;

                    //clear
                    foreach (Transform child in ChoiceButtonContainer)
                    {
                        if (child == null) continue;
                        Destroy(child.gameObject);
                    }

                    // next node
                    UpdateDialogueFromNode(GetNextNode(choice.NodeID));

                });
            }
        }
    }

    private bool DoesFillScConditions(ConditionsSC choice)
    {
        return PlayerInventoryManager.instance.DoesPlayerFillCondition(choice);
    }
    private bool DoesFillKeyConditions(string key)
    {
        return ConditionsManager.instance.EvaluateCondition(key);
    }

    private void EndDialogue()
    {
        _currentDialogueContainer.HideContainer();
        _nextButton.gameObject.SetActive(false);
        _currentNode = null;

        foreach (Transform child in ChoiceButtonContainer)
        {
            Destroy(child.gameObject);
        }

        OnDialogueEnd?.Invoke();
    }

    public void ChangeSpeaker(Espeaker speak)
    {
        foreach (var speaker in SpeakersScriptable.speakers)
        {
            if (speaker.speakEnum == speak)
            {
                SetNewSpeaker(speaker);
            }
        }
    }
    


    private void SetNewSpeaker(SpeakerInfo speaker)
    {
        _currentSpeaker = speaker;
    }

    private DSNodeSaveData GetNodeStart()
    {
        foreach (var node in runtimeGraph.Nodes)
        {
            if (node.DialogueType == DSDialogueType.Start)
            {
                return node;
            }
        }
        return null;
    }

}