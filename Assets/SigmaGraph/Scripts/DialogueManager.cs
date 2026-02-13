using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine.Events;
using System.Linq;
using TMPro;
using System.ComponentModel;
using UnityEngine.SceneManagement;

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

    private bool _isWaitingForChoice = false;

    [Header("Speakers")]
    public Speakers SpeakersScriptable;
    [SerializeField] private Animator _speakerAnimator;
    [SerializeField] private Animator _characterAnimator;

    private SpeakerInfo _currentSpeaker;

    [Header("Textes")]
    [SerializeField] private GameObject _prefabCensor;

    //node
    private Dictionary<string, DSNodeSaveData> _nodeLookup = new Dictionary<string, DSNodeSaveData>();
    private static DSNodeSaveData _currentNode;
    public static DSNodeSaveData CurrentNode => _currentNode;

    public dialogueContainer currentDialogueContainer;
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
    

    public void Fr()
    {
        UpdateLanguageSetting(language.FR);
    }
    
    public void En()
    {
        UpdateLanguageSetting(language.EN);
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

        //next button
        foreach (var container in _bubleContainerList)
            container.OnClicNextButton(NextButtonFunc);
    }


    private DSNodeSaveData GetNextNode(string nextID)
    {
        if (!string.IsNullOrEmpty(nextID) && _nodeLookup.TryGetValue(nextID, out var node))
        {
            return node;
        }

        return null;
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
        if (_currentNode.ChoicesInNode[0].ConditionsKey.Count <= 0)
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

        // -- CONTAINERS --
        //Narrator
        if (_currentNode.Speaker == Espeaker.Narrator) 
        {
            if(_bubleContainers.TryGetValue(bubleType.THINK, out var container)) 
                currentDialogueContainer = container;
        }
        //Default = Normal
        else if (_bubleContainers.TryGetValue(bubleType.NORMAL, out var container)) //_currentNode.GetBubleType() to avoid GD errors
        {
            currentDialogueContainer = container;
        }
        else
        {
            return;
        }

        if (currentDialogueContainer == null)
        {
            Debug.Log("Current Dialogue Container is null.");
            return;
        }

        if (_oldDialogueContainer != null)
        {
            _oldDialogueContainer.HideContainer();
        }
        _oldDialogueContainer = currentDialogueContainer;

        // -- TEXT --
        ChangeSpeaker(_currentNode.Speaker);
        if (_currentSpeaker == null)
        {
            Debug.Log("Current Speaker is null.");
            return;
        }

        //dialogue text
        string targetDialogue = FantasyDialogueTable.LocalManager.FindDialogue(_currentNode.GetDropDownKeyDialogue(), Enum.GetName(typeof(language), languageSetting));
        currentDialogueContainer.InitializeDialogueContainer(this, targetDialogue, _currentSpeaker.DisplayName, _currentSpeaker.GetSpriteForHumeur(_currentNode.GetHumeur()), _currentNode.isMultipleChoice);

        // -- EVENTS --
        if (_currentNode.HasEvent)
        {
            foreach (var e in _currentNode.EventKeys)
            {
                UnityEvent targetEvent = EventsManager.Instance.FindEvent(e);
                targetEvent?.Invoke();
            }
        }

        // -- ANIMS --
        //move
        if (_characterAnimator && _speakerAnimator)
        {
            if (_currentNode.DialogueType == DSDialogueType.MultipleChoice)
            {
                // Anim multiple choices
                if (_currentNode.isMultipleChoice && !IsSpeakerOffset)
                    FocusOnSpeaker(true);
                //reset anim multiple choices
                else if (!_currentNode.isMultipleChoice && IsSpeakerOffset)
                    FocusOnSpeaker(false);
            }
        }

        //react
        if (_currentNode.GetHumeur() != HumeurSpeaker.Neutre) _speakerAnimator.SetTrigger("Reaction");

    }

    // -- BUTTONS --
    private void NextButtonFunc()
    {
        if (MainEvents.GoToHub)
            SceneManager.LoadScene("Hub_Scene");
        else if (CanInteract)
            TryToUpdateNextDialogueFromNextNode();
    }


    private void CreateButtonsChoice()
    {
        if (_currentNode.ChoicesInNode.Count > 1)
        {
            _isWaitingForChoice = true;
            foreach (DSChoiceSaveData choice in _currentNode.ChoicesInNode)
            {
                // init choice button
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
                    buttonController.InitializeButtonChoiceController(this, fillCondition, textButton);
                }

                // action on choice clic
                choiceButton.onClick.AddListener(() =>
                {
                    if (!CanInteract) return;

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

    // -- CONDITIONS --
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
        currentDialogueContainer.HideContainer();
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


    // Censorship
    public void CheckCensorship(ref GameObject censorObject, TextMeshProUGUI tmp_text, Transform parent)
    {
        ResetCensorship(ref censorObject);

        if (NeedCensorship(tmp_text.text))
            censorObject = CensorSentence(ref censorObject, tmp_text, parent);
    }


    private bool NeedCensorship(string text) {
        var count = text.Count(x => x == '*');
        if (count % 2 == 0 && count != 0) 
            return true;

        return false;
    }

    public GameObject CensorSentence(ref GameObject censorObject, TextMeshProUGUI tmp_text, Transform parent)
    {
        string text = tmp_text.text;
        bool censored = false;

        censorObject = new GameObject("GO_Censorship");
        censorObject.transform.parent = parent;

        for (int i = 0; i < text.Length; i++)
        {
            //toggle censorship
            if (text[i].ToString() == "*")
                censored = !censored;
            //spawn censorship on letter
            else if (censored)
            {
                GameObject censorship = GetPositionOfLetterAsGameObject(tmp_text, i);
                censorship.transform.parent = censorObject.transform;
            }
        }

        return censorObject;
    }

    private void ResetCensorship(ref GameObject censorParent)
    {
        if(censorParent) 
            Destroy(censorParent);
    }

    private GameObject GetPositionOfLetterAsGameObject(TextMeshProUGUI tmp_text, int index)
    {
        tmp_text.ForceMeshUpdate();

        TMP_CharacterInfo charInfo = tmp_text.textInfo.characterInfo[index];

        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        Vector3[] vertices = tmp_text.textInfo.meshInfo[materialIndex].vertices; // for tmp_submesh too

        Vector2 charMidTopLine = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, (charInfo.bottomLeft.y + charInfo.topLeft.y) / 2);
        Vector3 worldPos = tmp_text.transform.TransformPoint(charMidTopLine);

        //Instantiate(parent, transform, worldPositionStays:false);

#if UNITY_EDITOR
        UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath("Assets/SigmaGraph/OPTIONNAL/Prefabs/CensorshipPrefab.prefab", typeof(GameObject));
#else
        //UnityEngine.Object prefab = Resources.Load("Assets/SigmaGraph/OPTIONNAL/Prefabs/CensorshipPrefab.prefab", typeof(GameObject));
        UnityEngine.Object prefab = _prefabCensor;
#endif
        if (prefab == null)  {
            Debug.LogError("Censorship prefab wasn't found by path.");
            return null;
        }

        GameObject censorship = Instantiate(prefab,transform) as GameObject;
        censorship.transform.position = worldPos;

        return censorship;

    }

    // Anims

    private void FocusOnSpeaker(bool focused)
    {
        // Focus On Speaker
        if (focused)
        {
            _speakerAnimator.SetBool("Offset", true);
            _characterAnimator.SetTrigger("Remove");
        }
        // Initial Set up
        else
        {
            _speakerAnimator.SetBool("Offset", false);
            _characterAnimator.SetTrigger("Start");
        }
    }

    private bool IsSpeakerOffset => _speakerAnimator.GetBool("Offset");


}