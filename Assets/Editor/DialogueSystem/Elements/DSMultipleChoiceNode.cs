using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using PlasticGui.Help;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;



public class DSMultipleChoiceNode : DSNode
{
    private Button _addChoiceButton;
    private Button _changeNodeType;
    private Button _addEventButton; // EVENT
    private TextField _statedNodeField;
    private List<Port> _choicePorts = new List<Port>();
    private Dictionary<Port, TextField> _choicePortsTextField = new Dictionary<Port, TextField>();
    

    private void ClearChoicePorts()
    {
        Saves.ChoicesInNode.Clear();

        foreach (var kv in Saves.ConditionsMapElement)
        {
            ClearConditions(kv.Value);
        }

        Saves.ConditionsMapElement.Clear();

        foreach (Port port in _choicePorts)
        {
            if (port.connected)
            {
                graphView.DeleteElements(port.connections);
            }

            _choicePortsTextField.TryGetValue(port, out TextField textField);
            if (textField != null)
            {
                textField.RemoveFromHierarchy();
            }

            graphView.RemoveElement(port);
        }

        _choicePorts.Clear();
    }


    private void SetNodeTypeLabel()
    {
        if (Saves.isMultipleChoice)
        {
            _changeNodeType.RemoveFromClassList("ds-node__buttonMultiple");
            _changeNodeType.AddToClassList("ds-node__buttonSingle");
            _statedNodeField.value = "Node Type: Multiple Choice";
            mainContainer.Add(_addChoiceButton);
        }
        else
        {
            _statedNodeField.value = "Node Type: Single Choice";
            _changeNodeType.RemoveFromClassList("ds-node__buttonSingle");
            _changeNodeType.AddToClassList("ds-node__buttonMultiple");
            if (mainContainer.Contains(_addChoiceButton))
            {
                mainContainer.Remove(_addChoiceButton);
            }
        }
    }

    private void SwitchNodeType()
    {
        ClearChoicePorts();

        Saves.isMultipleChoice = !Saves.isMultipleChoice;

        UpdateNodeOnSwitch();
    }

    private void UpdateNodeOnSwitch()
    {
        if (Saves.isMultipleChoice)
        {
            CreateSingleChoicePortNew("");
            CreateSingleChoicePortNew("");
        }
        else
            CreateSingleChoicePortNew("Continue");

        SetNodeTypeLabel();

        RefreshExpandedState();
    }

    private void CreateEventSection()
    {
        //ShowEventButton(true);

        extensionContainer.Add(CreateFoldoutEvent());
    }


    // TO EDIT (ranger ici  ou ds node)
    private void ShowEventButton(bool show)
    {
        if (show)
        {
            mainContainer.Remove(_addEventButton);

            ShowEventButton(true);

        }
        else
        {
            _addEventButton = DSElementUtility.CreateButton("Add Event", () => { CreateEventSection(); });

            mainContainer.Add(_addEventButton);

            _addEventButton.AddToClassList("ds-node__buttonEvent");
        }
    }

    public override void Draw(Color color)
    {
        base.Draw(new Color(0.2f, 0.2f, 0.4f));
        
        // TOP LEVEL CONTAINERS // 

        _statedNodeField = DSElementUtility.CreateTextField("Node Type", null);
        titleContainer.Add(_statedNodeField);
        
        // INPUT CONTAINER //
        
        inputContainer.Add(CreateInputPort("Input"));
        
        // TITLE CONTAINER //
        
        // --- CREATION DES ENUMS (HUMEUR, PERSONNAGES, TYPE DE BULLE) --- //
        
        var compactRow = new VisualElement();
        compactRow.style.flexDirection = FlexDirection.Row;
        compactRow.style.alignItems = Align.Center;
        compactRow.style.justifyContent = Justify.SpaceBetween;
        compactRow.style.marginTop = 6;
        compactRow.style.marginBottom = 2;

        var speakerField = new EnumField(Speaker) { tooltip = "Character" };
        var humeurField  = new EnumField(Humeur)  { tooltip = "Mood" };
        var uiField      = new EnumField(BubleType) { tooltip = "Bubble UI" };

        var speakerWrap = CreateCompactLabeledEnum("Char", speakerField, 54);
        var humeurWrap  = CreateCompactLabeledEnum("Mood", humeurField, 48);
        var uiWrap      = CreateCompactLabeledEnum("UI", uiField, 44);

        speakerField.RegisterValueChangedCallback(evt => SetSpeaker((Espeaker)evt.newValue));
        humeurField.RegisterValueChangedCallback(evt => SetHumeur((HumeurSpeaker)evt.newValue));
        uiField.RegisterValueChangedCallback(evt => BubleType = (bubleType)evt.newValue);

        compactRow.Add(speakerWrap);
        compactRow.Add(humeurWrap);
        compactRow.Add(uiWrap);

        titleContainer.Add(compactRow);

        // MAIN CONTAINER //

        _addChoiceButton = DSElementUtility.CreateButton("Add Choice", () =>
        {
            CreateSingleChoicePortNew("New Choice");
        });

        _addEventButton = DSElementUtility.CreateButton("Add Event", () => { CreateEventSection(); });

        _changeNodeType = DSElementUtility.CreateButton("Switch node Type", () => { SwitchNodeType(); });

        mainContainer.Add(_addChoiceButton);
        mainContainer.Add(_addEventButton);
        mainContainer.Add(_changeNodeType);

        _addChoiceButton.AddToClassList("ds-node__button");
        _addEventButton.AddToClassList("ds-node__buttonEvent");
        _changeNodeType.AddToClassList("ds-node__buttonSingle");

        // SECTIONS //

        extensionContainer.Add(CreateFoldoutDialogueKeyDropDown());

        if (Saves.HasEvent)
            CreateEventSection();

        // OUTPUT CONTAINER //

        foreach (DSChoiceSaveData choice in Saves.ChoicesInNode)
        {
            CreateSingleChoicePortForExisting(choice, choice.GetDropDownKeyChoice());
            if (choice.ConditionsKey != null && choice.ConditionsKey.Count > 0)
            {
                Port port = _choicePorts.LastOrDefault();
                if (port != null)
                {
                    foreach (var savedKey in choice.ConditionsKey)
                    {
                        var obj = CreateConditionKeyField(port, savedKey);
                        AddConditionsBelowPort(port, obj, true);
                    }
                }
            }
        }

        if (Saves.ChoicesInNode.Count == 0)
        {
            CreateSingleChoicePortNew("");
        }
        SetNodeTypeLabel();

        RefreshExpandedState();

    }

    private void OnDropDownChoiceTranslate(Port choicePort, DropdownField dropdown, DSChoiceSaveData choiceData)
    {
        if (_choicePortsTextField.TryGetValue(choicePort, out TextField textField))
        {
            if (dropdown != null)
            {
                choiceData.SaveDropDownKeyChoice(dropdown.value);
                textField.value = FantasyDialogueTable.LocalManager.GetAllDialogueFromValue(dropdown.value);
            }
        }
    }

    private TextField CreateLabelChoiceTranslate()
    {
        var text = DSElementUtility.CreateTextField("Choose a key to translate", null);
        return text;
    }

    private void ClearConditions(List<VisualElement> conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition != null && condition.parent != null)
            {
                outputContainer.Remove(condition);
            }
        }
    }
    
    private bool IsAncestor(VisualElement possibleAncestor, VisualElement element)
    {
        var p = element;
        while (p != null)
        {
            if (p == possibleAncestor) return true;
            p = p.parent;
        }
        return false;
    }
    
    private VisualElement FindContainerForPort(Port choicePort)
    {
        if (choicePort?.parent != null) return choicePort.parent;

        var candidates = new[] { outputContainer, inputContainer, extensionContainer, mainContainer };
        foreach (var cand in candidates)
        {
            if (cand == null) continue;
            foreach (var child in cand.Children())
            {
                if (child == choicePort || IsAncestor(child, choicePort))
                    return cand;
            }
        }

        return null;
    }
    
    private VisualElement CreateConditions(Port choicePort, ConditionsSC initialValue = null)
    {
        ObjectField conditionsField = new ObjectField()
        {
            objectType = typeof(ConditionsSC),
            allowSceneObjects = false,
            value = initialValue
        };

        conditionsField.AddToClassList("ds-node__conditions-field");

        conditionsField.RegisterValueChangedCallback(evt =>
        {
            if (evt.previousValue == evt.newValue) return;
            AddConditionsScToObjectField(choicePort, (ConditionsSC)evt.newValue);
        });

        conditionsField.style.marginLeft = new StyleLength(16);
        conditionsField.style.alignSelf = Align.Stretch;

        return conditionsField;

    }

    private VisualElement CreateConditionKeyField(Port choicePort, string initialValue = "")
    {
        // TO EDIT 
        DropdownField dropdownField = new DropdownField();
        ConditionsSO SO = (ConditionsSO)AssetDatabase.LoadAssetAtPath("Assets/SigmaGraph/Scripts/Condition/ConditionV2/Conditions.asset", typeof(ConditionsSO));

        dropdownField.value = initialValue;
        dropdownField.userData = initialValue;

        dropdownField.RegisterValueChangedCallback(evt =>
        {
            if (evt.previousValue == evt.newValue) return;
            
            // clear previous value
            int idx = _choicePorts.IndexOf(choicePort);
            Saves.ChoicesInNode[idx].ConditionsKey.Remove(evt.previousValue);

            // add new value
            dropdownField.userData = evt.newValue;
            AddConditionsKeyToField(choicePort, (string)evt.newValue);
        });

        SO?.FillConditionDropdown(ref dropdownField);

        return dropdownField;
    }

    // ConditionV1
    private void ClearScCondition(Port port, VisualElement condition)
    {
        if (condition == null) return;

        ConditionsSC scToRemove = null;
        var objField = condition as ObjectField ?? condition.Q<ObjectField>();
        if (objField != null)
        {
            scToRemove = objField.value as ConditionsSC;
        }

        if (condition.parent != null)
        {
            condition.parent.Remove(condition);
        }

        if (port != null && Saves.ConditionsMapElement != null && Saves.ConditionsMapElement.ContainsKey(port))
        {
            Saves.ConditionsMapElement[port].Remove(condition);
            if (Saves.ConditionsMapElement[port].Count == 0)
                Saves.ConditionsMapElement.Remove(port);
        }

        if (port != null)
        {
            int idx = _choicePorts.IndexOf(port);
            if (idx >= 0 && idx < Saves.ChoicesInNode.Count)
            {
                var choiceData = Saves.ChoicesInNode[idx];
                if (choiceData.ConditionsSc != null && scToRemove != null)
                {
                    choiceData.ConditionsSc.Remove(scToRemove);
                }
            }
        }

        RefreshExpandedState();
        MarkDirtyRepaint();
    }

    // Condition V2
    private void ClearKeyCondition(Port port, VisualElement condition)
    {
        if (condition == null) return;

        // userdata registered in dropdown value
        string keyToRemove = condition.userData as string; 
        
        if (condition.parent != null)
        {
            condition.parent.Remove(condition);
        }

        if (port != null && Saves.ConditionsMapElement != null && Saves.ConditionsMapElement.ContainsKey(port))
        {
            Saves.ConditionsMapElement[port].Remove(condition);
            if (Saves.ConditionsMapElement[port].Count == 0)
                Saves.ConditionsMapElement.Remove(port);
        }

        if (port != null)
        {
            int idx = _choicePorts.IndexOf(port);
            if (idx >= 0 && idx < Saves.ChoicesInNode.Count)
            {
                var choiceData = Saves.ChoicesInNode[idx];

                if (!string.IsNullOrEmpty(keyToRemove))
                {
                    choiceData.ConditionsKey.Remove(keyToRemove);
                }
            }
        }

        RefreshExpandedState();
        MarkDirtyRepaint();
    }

    private void ClearConditions(List<VisualElement> conditions, Port port = null)
    {
        if (conditions == null) return;

        foreach (var condition in conditions.ToList())
        {
            if (condition == null) continue;

            if (condition.parent != null)
                condition.parent.Remove(condition);

            if (port != null && Saves.ConditionsMapElement != null && Saves.ConditionsMapElement.ContainsKey(port))
            {
                Saves.ConditionsMapElement[port].Remove(condition);
            }
            else if (Saves.ConditionsMapElement != null)
            {
                foreach (var kvp in Saves.ConditionsMapElement.ToList())
                {
                    if (kvp.Value.Contains(condition))
                    {
                        kvp.Value.Remove(condition);
                        if (kvp.Value.Count == 0)
                            Saves.ConditionsMapElement.Remove(kvp.Key);
                    }
                }
            }
        }

        RefreshExpandedState();
        MarkDirtyRepaint();
    }
    
    // condition V1
    private void AddConditionsScToObjectField(Port port, ConditionsSC conditionSc)
    {
        if (port == null || conditionSc == null)
        {
            Debug.Log("Port or ConditionsSC is null, cannot add to ConditionsMapSc.");
            return;
        }

        int idx = _choicePorts.IndexOf(port);
        if (idx < 0 || idx >= Saves.ChoicesInNode.Count)
        {
            Debug.LogWarning("[AddConditionsScToObjectField] Impossible de retrouver l'index du choix pour ce port.");
            return;
        }

        DSChoiceSaveData choiceData = Saves.ChoicesInNode[idx];

        if (choiceData.ConditionsSc == null)
            choiceData.ConditionsSc = new List<ConditionsSC>();

        choiceData.ConditionsSc.Add(conditionSc);
    }

    // condition V2
    private void AddConditionsKeyToField(Port port, string conditionKey)
    {
        if (port == null || conditionKey == null)
        {
            Debug.Log("Port or ConditionsKey is null, cannot add to ConditionsMapKey.");
            return;
        }

        int idx = _choicePorts.IndexOf(port);
        if (idx < 0 || idx >= Saves.ChoicesInNode.Count)
        {
            Debug.LogWarning("[AddConditionsKeyToObjectField] Impossible de retrouver l'index du choix pour ce port.");
            return;
        }

        DSChoiceSaveData choiceData = Saves.ChoicesInNode[idx];

        if (choiceData.ConditionsKey == null)
            choiceData.ConditionsKey = new List<string>();

        choiceData.ConditionsKey.Add(conditionKey);
    }


    private (Port, DropdownField) CreateSingleChoicePortForExisting(DSChoiceSaveData choiceData, string dropDownKey = "")
{
    Port choicePort = this.CreatePort();
    choicePort.userData = choiceData;

    DropdownField choiceDropdown = null;
    if (Saves.isMultipleChoice)
    {
        choiceDropdown = DSElementUtility.CreateDropdownArea("Choice KEY");
        FillCsvDialogueDropdown(choiceDropdown);
        choiceDropdown.RegisterValueChangedCallback(callback => { OnDropDownChoiceTranslate(choicePort, choiceDropdown, choiceData); });

        if (!string.IsNullOrEmpty(dropDownKey))
            choiceDropdown.value = dropDownKey;

        choicePort.Add(choiceDropdown);
    }
    else
    {
        Label choiceLabel = new Label("Continue");
        choicePort.Add(choiceLabel);
    }

    if (Saves.isMultipleChoice && Saves.ChoicesInNode.Count > 2)
    {
        Button deleteChoiceButton = DSElementUtility.CreateButton("X", () =>
        {
            if (choicePort.connected)
            {
                graphView.DeleteElements(choicePort.connections);
            }
            if (Saves.ConditionsMapElement != null && Saves.ConditionsMapElement.TryGetValue(choicePort, out List<VisualElement> condElem))
            {
                ClearConditions(condElem);
                Saves.ConditionsMapElement.Remove(choicePort);
            }
            
            _choicePortsTextField.TryGetValue(choicePort, out TextField textField);
            if (textField != null)
            {
                textField.RemoveFromHierarchy();
            }
            int idx = _choicePorts.IndexOf(choicePort);
            if (idx >= 0 && idx < Saves.ChoicesInNode.Count)
            {
                Saves.ChoicesInNode.RemoveAt(idx);
            }
            _choicePorts.Remove(choicePort);
            _choicePortsTextField.Remove(choicePort);

            graphView.RemoveElement(choicePort);
        });
        
        deleteChoiceButton.AddToClassList("ds-node__buttonDelete");
        choicePort.Add(deleteChoiceButton);
    }

    if (Saves.isMultipleChoice)
    {
        Button conditionsButton = DSElementUtility.CreateButton("Add Conditions", () => { AddConditionsBelowPort(choicePort, CreateConditionKeyField(choicePort)); });
        conditionsButton.AddToClassList("ds-node__button");
        choicePort.Add(conditionsButton);
    }

    outputContainer.Add(choicePort);

    _choicePorts.Add(choicePort);

    if (Saves.isMultipleChoice)
    {
        var label = CreateLabelChoiceTranslate();
        label.AddToClassList("ds-node__label-translate");
        
        AddConditionsBelowPort(choicePort, label, false);
        _choicePortsTextField[choicePort] = label;

        if (!string.IsNullOrEmpty(dropDownKey))
        {
            OnDropDownChoiceTranslate(choicePort, choiceDropdown, choiceData);
        }
    }

    choicePort.MarkDirtyRepaint();
    RefreshExpandedState();
    MarkDirtyRepaint();

    return (choicePort, choiceDropdown);
}


private void CreateSingleChoicePortNew(string dropDownKey = "")
{
    DSChoiceSaveData newChoice = new DSChoiceSaveData();
    newChoice.SaveDropDownKeyChoice(dropDownKey);

    Saves.ChoicesInNode.Add(newChoice);

    CreateSingleChoicePortForExisting(newChoice, dropDownKey);
}

private int FindChildIndexContainingPort(VisualElement container, Port choicePort)
{
    var children = container.Children().ToList();
    for (int i = 0; i < children.Count; i++)
    {
        if (children[i] == choicePort || IsAncestor(children[i], choicePort))
            return i;
    }
    return -1;
}

private int FindLabelIndexForPort(VisualElement container, int portIndex)
{
    var children = container.Children().ToList();
    for (int i = portIndex + 1; i < children.Count; i++)
    {
        var child = children[i];
        if (FindPortInElement(child) != null)
            return -1;

        if (child.ClassListContains("ds-node__label-translate"))
            return i;
    }
    return -1;
}

private Port FindPortInElement(VisualElement elem)
{
    if (elem == null) return null;
    if (elem is Port directPort) return directPort;

    foreach (var child in elem.Children())
    {
        var found = FindPortInElement(child);
        if (found != null) return found;
    }
    return null;
}

void AddConditionsBelowPort(Port choicePort, VisualElement elementToAdd, bool canBeDeleted = true)
{
    if (choicePort == null || elementToAdd == null) return;

    VisualElement container = FindContainerForPort(choicePort);
    if (container == null)
    {
        var fallback = extensionContainer ?? mainContainer ?? (VisualElement)this;
        fallback.Add(elementToAdd);
        elementToAdd.MarkDirtyRepaint();
        return;
    }

    int portIndex = FindChildIndexContainingPort(container, choicePort);
    if (portIndex < 0)
    {
        container.Add(elementToAdd);
        elementToAdd.MarkDirtyRepaint();
        return;
    }

    int labelIndexForThisPort = FindLabelIndexForPort(container, portIndex);

    int insertIndex;
    if (labelIndexForThisPort >= 0)
    {
        insertIndex = labelIndexForThisPort + 1;
    }
    else
    {
        insertIndex = portIndex + 1;
    }

    container.Insert(insertIndex, elementToAdd);

    bool isLabel = elementToAdd.ClassListContains("ds-node__label-translate");
    if (!isLabel)
    {
        elementToAdd.AddToClassList("ds-node__conditions-container");

        if (Saves.ConditionsMapElement == null)
            Saves.ConditionsMapElement = new Dictionary<Port, List<VisualElement>>();

        if (!Saves.ConditionsMapElement.ContainsKey(choicePort))
            Saves.ConditionsMapElement[choicePort] = new List<VisualElement>();

        Saves.ConditionsMapElement[choicePort].Add(elementToAdd);

        if (canBeDeleted)
        {
            Button butClearCondition = DSElementUtility.CreateButton("X", () =>
            {
                ClearKeyCondition(choicePort, elementToAdd);
            });
            butClearCondition.AddToClassList("ds-node__buttonDeleteCondition");
            elementToAdd.Add(butClearCondition);
        }
    }
    else
    {
        if (!_choicePortsTextField.ContainsKey(choicePort))
            _choicePortsTextField.Add(choicePort, (TextField)elementToAdd);
        else
            _choicePortsTextField[choicePort] = (TextField)elementToAdd;
    }
    
    
    elementToAdd.MarkDirtyRepaint();
    RefreshExpandedState();
    MarkDirtyRepaint();
}

private VisualElement CreateCompactLabeledEnum(string shortLabel, EnumField enumField, int labelWidth = 56)
{
    var wrap = new VisualElement();
    wrap.style.flexDirection = FlexDirection.Row;
    wrap.style.alignItems = Align.Center;
    wrap.style.marginRight = 6;
    wrap.style.height = 22;

    // short label
    var lbl = new Label(shortLabel);
    lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
    lbl.style.fontSize = 10;
    lbl.style.marginRight = 4;
    lbl.style.minWidth = labelWidth;
    lbl.style.maxWidth = labelWidth;
    lbl.style.unityTextAlign = TextAnchor.MiddleLeft;
    wrap.Add(lbl);

    // compact enum (hide default label and tighten spacing)
    enumField.label = null; // remove the built-in label
    enumField.style.minWidth = 90;
    enumField.style.maxWidth = 140;
    enumField.style.marginLeft = 0;
    enumField.style.marginRight = 0;

    wrap.Add(enumField);
    return wrap;
}

}

