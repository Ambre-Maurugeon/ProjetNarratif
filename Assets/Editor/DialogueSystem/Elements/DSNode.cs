using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Events;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DSNode : Node
{
    public string ID { get; set; }

    public string DialogueName;
    public Espeaker Speaker { get; set; }
    public bubleType BubleType { get; set; }
    public HumeurSpeaker Humeur { get; set; }
    public DSNodeSaveData Saves { get; set; }
    public string Text { get; set; }
    public bool HasEvent { get; set; }

    public DropdownField DialogueTypeField { get; set; }
    public Label LanguageLabel { get; set; }
    public DSDialogueType DialogueType { get; set; }
    public DSGroup Group { get; set; }

    protected DSGraphView graphView;

    private Color defaultBackgroundColor;

    private TextField _fieldDialogueLabel;
    
    private TextField _fieldEventCaption;

    private DropdownField _dropdownFieldDialogue;

    private DropdownField _dropdownFieldEvent;

    public bool OnlyOneConditionNeeded;
    
    private TextField _dialogeNameTextField;

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
        evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

        base.BuildContextualMenu(evt);
    }

    public virtual void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
    {
        ID = Guid.NewGuid().ToString();

        DialogueName = nodeName;

        Saves = new DSNodeSaveData();
        Saves.SetChoices(new List<DSChoiceSaveData>());

        Text = "Dialogue text.";
        SetPosition(new Rect(position, Vector2.zero));

        graphView = dsGraphView;
        defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

        mainContainer.AddToClassList("ds-node__main-container");
        extensionContainer.AddToClassList("ds-node__extension-container");
    }
    
    // LA FONCTION LA PLUS IMPORTANTE QUI DESSINE LE NODE // ICI EN BASE.DRAW() DEPUIS LES NODES UNDERCLASS PERMETTENT DE CUSTOMISER LEUR APPARENCE // PAS OBLIGATOIRE D'EN HERITEER //
    public virtual void Draw(Color colorNode)
{
    // --- HEADER CUSTOMISATION --- //
    
    var header = new VisualElement();
    header.style.flexDirection = FlexDirection.Row;
    header.style.alignItems = Align.Center;
    header.style.paddingTop = 4;
    header.style.paddingBottom = 4;
    header.style.paddingLeft = 6;
    header.style.paddingRight = 6;
    header.AddToClassList("ds-node__header");

    var colorStripe = new VisualElement();
    colorStripe.style.width = 6;
    colorStripe.style.height = 34;
    colorStripe.style.marginRight = 8;
    colorStripe.style.backgroundColor = new StyleColor(new Color(0.12f, 0.6f, 0.8f)); // turquoise
    colorStripe.style.borderTopLeftRadius = 6;
    colorStripe.style.borderBottomLeftRadius = 6;
    header.Add(colorStripe);

    // PETITE ICON PRCQ ON EST FANCY //
    
    var icon = new VisualElement();
    icon.style.width = 28;
    icon.style.height = 28;
    icon.style.marginRight = 8;
    
    icon.style.borderBottomLeftRadius = 4;
    icon.style.borderBottomRightRadius = 4;
    icon.style.borderTopLeftRadius = 4;
    icon.style.borderTopRightRadius = 4;
 
    icon.style.backgroundColor = new StyleColor(colorNode);
    icon.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
    header.Add(icon);

    // DIALOGUE NAME TEXT FIELD //
    
    _dialogeNameTextField = DSElementUtility.CreateTextField(DialogueName, null, (ChangeEvent<string> evt) =>
    {
        var target = (TextField)evt.target;
        string newValue = evt.newValue;
        string oldValue = DialogueName;
        
        newValue = newValue.RemoveWhitespaces().RemoveSpecialCharacters();
        bool Empty = string.IsNullOrEmpty(newValue);

        if (Empty)
        {
            target.value = oldValue;
            Debug.LogWarning("LE NOM DU NODE NE PEUT PAS ETRE VIDE !");
            return;
        }
        
        if(graphView.GetUnGroupedNodesNames().Contains(newValue.ToLower()))
        {
            target.value = oldValue; // Revert to the old name
            Debug.LogWarning($"Y'A DEJA UN NODE QUI S'APPELLE COMME {oldValue} DANS LE GRAPHE, ON TE REMET {oldValue} ! (stp)");
            return;
        }
        if (Group == null)
        {
            // ON RETIRE LE UNGROUPNODE DE LA LISTE // ON MET DIALOGUENAME A JOUR ENTRE LES DEUX CAR LA FONCTION GRAPHWIEW A BESOIN DU OLD NAME !!!!! // ON REAJOUTE LE NODE AU GROUPE //
            graphView.RemoveUngroupedNode(this);
            DialogueName = newValue;
            
            graphView.AddUngroupedNode(this);
        }
        else
        {
            var currentGroup = Group;
            // ON RETIRE LE NODE GROUPE // ON MET DIALOGUENAME A JOUR ENTRE LES DEUX CAR LA FONCTION GRAPHWIEW A BESOIN DU OLD NAME !!!!! // ON REAJOUTE LE NODE AU GROUPE //
            graphView.RemoveGroupedNode(this, currentGroup);
            DialogueName = newValue;
            
            graphView.AddGroupedNode(this, currentGroup);
        }

        target.value = newValue;
    });

    _dialogeNameTextField.AddToClassList("ds-node__text-field");
    _dialogeNameTextField.style.flexGrow = 1;
    _dialogeNameTextField.style.height = 28;
    _dialogeNameTextField.style.marginRight = 4;
    _dialogeNameTextField.style.unityTextAlign = TextAnchor.MiddleLeft;
    header.Add(_dialogeNameTextField);

        // UNCOMMENT LE CODE CI DESSOUS POUR AJOUTER UN PETIT SUBTITLE EN HAUT A DROITE DU NODE //

        //var subtitle = new Label(DialogueType.ToString());
        //subtitle.AddToClassList("ds-node__subtitle");
        //subtitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        //subtitle.style.fontSize = 10;
        //subtitle.style.unityTextAlign = TextAnchor.MiddleCenter;
        //subtitle.style.minWidth = 54;
        //subtitle.style.marginLeft = 6;
        //header.Add(subtitle);

        // TITLE CONTAINER //

        titleContainer.Clear();
    titleContainer.Add(header);

    // INPUT CONTAINER //
    
    inputContainer.Clear();
    
    // EXTENSION CONTAINER //
    
    extensionContainer.Clear();

    // STYLISATION DES CONTENEURS //
    mainContainer.AddToClassList("ds-node__main-container");
    extensionContainer.AddToClassList("ds-node__extension-container");

    RefreshExpandedState();
}

    // CREE LE DROPDOWN POUR CHOISIR LA KEY DU DIALOGUE // UTILE POUR LES CHOIX DE TEXTES LOCALIZER //
    public VisualElement CreateFoldoutDialogueKeyDropDown()
    {
        VisualElement customDataContainer = new VisualElement();
        customDataContainer.AddToClassList("ds-node__custom-data-container");

        Foldout textFoldout = DSElementUtility.CreateFoldout("Dialogue Section");
        _dropdownFieldDialogue = DSElementUtility.CreateDropdownArea("Dialogue Key", "Key");
        FillCsvDialogueDropdown(_dropdownFieldDialogue, true);
        _dropdownFieldDialogue.RegisterValueChangedCallback((ChangeEvent<string> evt) => OnDialogueDropdown(_dropdownFieldDialogue));
        textFoldout.Add(_dropdownFieldDialogue);

        _fieldDialogueLabel = DSElementUtility.CreateTextField("waiting for key...");
        _fieldDialogueLabel.style.marginTop = 6;
        textFoldout.Add(_fieldDialogueLabel);

        if (Saves.GetDropDownKeyDialogue() != "")
        {
            _dropdownFieldDialogue.value = Saves.GetDropDownKeyDialogue();
            OnDialogueDropdown(_dropdownFieldDialogue);
        }

        customDataContainer.Add(textFoldout);
        return customDataContainer;
    }

    // CREE EVENT FOLDOUT
    protected VisualElement CreateFoldoutEvent()
    {

        VisualElement customDataContainer = new VisualElement();
        customDataContainer.AddToClassList("ds-node__event-container");

        Foldout textFoldout = DSElementUtility.CreateFoldout("Event Section");
        _dropdownFieldEvent = DSElementUtility.CreateDropdownArea("Event Key", "Key");

        // TO EDIT 
        EventsSO SC = (EventsSO)AssetDatabase.LoadAssetAtPath("Assets/SigmaGraph/Scripts/Events/Events.asset", typeof(EventsSO));
        SC?.FillEventDropdown(ref _dropdownFieldEvent);
        _dropdownFieldEvent.RegisterValueChangedCallback((ChangeEvent<string> evt) => OnEventDropdown(_dropdownFieldEvent));

        // field 
        textFoldout.Add(_dropdownFieldEvent);
        _fieldEventCaption = DSElementUtility.CreateTextField(""); // TO EDIT text to label (read only ?)
        DSElementUtility.CreateLabelField("");
        textFoldout.Add(_fieldEventCaption);

        if (Saves.GetDropDownKeyEvent() != "")
        {
            _dropdownFieldEvent.value = Saves.GetDropDownKeyEvent();
            OnEventDropdown(_dropdownFieldEvent);
        }


        // button delete Event 

        Button deleteButton = DSElementUtility.CreateButton("X", () =>
        {
            extensionContainer.Remove(customDataContainer);
            Saves.hasEvent = false;
        });

        deleteButton.AddToClassList("ds-node__buttonDelete");

        // container
        customDataContainer.Add(textFoldout);

        customDataContainer.style.marginTop = 6;

        customDataContainer.Add(deleteButton);

        return customDataContainer;
    }

    // CREE UN PORT D'ENTREE MULTI-CONNECTION //
    public Port CreateInputPort(string portName)
    {
        Port inputPort = this.CreatePort(portName, Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
        inputPort.style.marginTop = 6;
        return inputPort;
    }
    
    // PERMET DE DECONNECTER TOUS LES PORTS DU NODE //
    public void DisconnectAllPorts()
    {
        DisconnectInputPorts();
        DisconnectOutputPorts();
    }

    // GERE L'EVENT QUAND ON CHANGE DE VALEUR DANS LE DROPDOWN //
    // TO EDIT ONE FUNCTION FOR ALL DORPDOWNS
    private void OnDialogueDropdown(DropdownField dropdownField)
    {
        if (_fieldDialogueLabel == null)
            return;

        _fieldDialogueLabel.value = FantasyDialogueTable.LocalManager.GetAllDialogueFromValue(dropdownField.value);
        Saves.SaveDropDownKeyDialogue(dropdownField.value);
    }

    protected void OnEventDropdown(DropdownField dropdownField)
    {
        if (_fieldEventCaption == null)
            return;

        EventsSO SC = (EventsSO)AssetDatabase.LoadAssetAtPath("Assets/SigmaGraph/Scripts/Events/Events.asset", typeof(EventsSO)); // TO EDIT
        _fieldEventCaption.value = SC?.GetEventByKey(dropdownField.value).caption;
        Saves.SaveDropDownKeyEvent(dropdownField.value);
    }

    // REMPLIT LE DROPDOWN AVEC LES KEYS DU CSV // "loadSpecificKeysToSpeaker" POUR SAVOIR SI ON CHARGE TOUTES LES KEYS OU JUSTE CELLES DU SPEAKER //
    public void FillCsvDialogueDropdown(DropdownField dropdownField, bool loadSpecificKeysToSpeaker = false)
    {
        dropdownField.choices.Clear();
        
        string speakerName = "";
        if (loadSpecificKeysToSpeaker)
        {
            speakerName = Enum.GetName(typeof(Espeaker), Speaker);
        }
        
        List<string> keys = FantasyDialogueTable.FindAll_Keys(speakerName);
        
        if (keys == null) return;
        foreach (string key in keys)
        {
            dropdownField.choices.Add(key);
        }
    }

    private void DisconnectInputPorts()
    {
        DisconnectPorts(inputContainer);
    }

    private void DisconnectOutputPorts()
    {
        DisconnectPorts(outputContainer);
    }

    private void DisconnectPorts(VisualElement container)
    {
        if (container == null)
        {
            return;
        }

        foreach (var visualElement in container.Children().ToList())
        {
            var port = visualElement as Port;
            if (port == null)
            {
                continue;
            }

            if (!port.connected)
            {
                continue;
            }

            graphView.DeleteElements(port.connections);
        }
    }

    public bool IsStartingNode()
    {
        if (!inputContainer.Children().Any())
            return true;

        Port inputPort = (Port)inputContainer.Children().First();
        return !inputPort.connected;
    }

    public void SetErrorStyle(Color color)
    {
        mainContainer.style.backgroundColor = color;
    }

    public void ResetStyle()
    {
        mainContainer.style.backgroundColor = defaultBackgroundColor;
    }

    public void SetSpeaker(Espeaker speaker)
    {
        Speaker = speaker;
        
        string speakerName = Enum.GetName(typeof(Espeaker), Speaker);

        List<string> keys = FantasyDialogueTable.FindAll_Keys(speakerName);
        if(_dropdownFieldDialogue == null) return;
        _dropdownFieldDialogue.choices.Clear();
        foreach (string key in keys)
        {
            _dropdownFieldDialogue.choices.Add(key);
        }
    }
    
    public void SetHumeur(HumeurSpeaker humeur)
    {
        Humeur = humeur;
    }
    



}
