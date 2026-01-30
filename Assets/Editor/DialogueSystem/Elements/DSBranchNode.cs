using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


public class DSBranchNode : DSNode
{
     public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
    {
        base.Initialize("Branch (Experimental)", dsGraphView, position);
        DialogueType = DSDialogueType.Branch;
    }

    private int _outPortIndex = 0;
    private List<object> _outPorts = new List<object>();
    private bool _oneOfConditions = false;
    private Toggle _oneOfConditionsToggle;

    public override void Draw(Color color)
    {
        base.Draw(new Color(0.8f, 0.2f, 0.5f));
        
         _oneOfConditions = Saves.OnlyOneConditionNeeded;
     
            if (Saves.ChoicesInNode.Count > 0)
            {
                _outPortIndex = 0;
                foreach (DSChoiceSaveData choiceData in Saves.ChoicesInNode)
                {
                    Direction dir = choiceData.GetPortDirection();
                    var port = CreateSingleChoicePortForExisting(choiceData, dir);

                    if (dir == Direction.Input)
                    {
                        inputContainer.Add(port);
                    }
                    else
                    {
                        outputContainer.Add(port);
                    }
                }
            }
            else
            {
                var input = CreateSingleChoicePortNew("", Direction.Input);
                inputContainer.Add(input);

                var outputIf = CreateSingleChoicePortNew("", Direction.Output);
                var outputElse = CreateSingleChoicePortNew("", Direction.Output);
                outputContainer.Add(outputIf);
                outputContainer.Add(outputElse);

            }


            
            // ADD TOGGLE ONE OF CONDITIONS //
            
            _oneOfConditionsToggle = new Toggle("One of the conditions below")
            {
                value = _oneOfConditions,
                tooltip = "True -> si une des conditions est vraie la branche acceptée (OR) \n False ->  toutes doivent être vraies (AND).",
            };
            _oneOfConditionsToggle.RegisterValueChangedCallback(evt =>
            {
                _oneOfConditions = evt.newValue;
                Saves.OnlyOneConditionNeeded = evt.newValue;
            });
            
            // ADD CONDTIONS BUTTONS //
            
            Button addConditionsButtons = DSElementUtility.CreateButton("Add Condition", () =>
            {
                AddConitionsSc();
            });
            
            mainContainer.Add(_oneOfConditionsToggle);
            mainContainer.Add(addConditionsButtons);
            
            // ADD USS STYLES //

            addConditionsButtons.AddToClassList("ds-node__button");
            titleContainer.AddToClassList("ds-node__title-container");
            mainContainer.AddToClassList("ds-node__main-container");
            
            
            // AJOUTE LES CONDITIONS EXISTANTES AU IF PORT //
            foreach (var condition in Saves.ChoicesInNode[0].Conditions)
            {
                AddConitionsSc(condition);
            }
            
            RefreshExpandedState();
    }

    private void AddConitionsSc(ConditionsSC condition = null)
    {
        ObjectField conditionsField = new ObjectField("Conditions --->")
        {
            objectType = typeof(ConditionsSC),
            value = condition,
        };
        
        conditionsField.RegisterValueChangedCallback(evt =>
        {
            ConditionsSC newCondition = (ConditionsSC)evt.newValue;
            if (newCondition != null)
            {
                // AJOUTE LA CONDITION AU IF //
                Saves.ChoicesInNode[0].Conditions.Add(newCondition);
            }
        });
        
        Button removeButton = DSElementUtility.CreateButton("X", () =>
        {
            // SUPPRIME LA CONDITION DU IF //
            Saves.ChoicesInNode[0].Conditions.Remove((ConditionsSC)conditionsField.value);
            mainContainer.Remove(conditionsField);
        });
        
        conditionsField.Add(removeButton);
        mainContainer.Add(conditionsField);
    }


    private Port CreateSingleChoicePortForExisting(DSChoiceSaveData choiceData, Direction portDirection)
    {
        Port choicePort = this.CreatePort("", Orientation.Horizontal, portDirection, Port.Capacity.Single);
        choicePort.userData = choiceData;
        choiceData.SavePortDirection(portDirection);

        if (portDirection == Direction.Output)
        {
            if (_outPortIndex == 0)
            {
                choicePort.portName = "True";
                _outPortIndex++;
            }
            else if (_outPortIndex == 1)
            {
                choicePort.portName = "False";
                _outPortIndex++;
            }
            else
            {
                // AU CAS OU Y'A PLUS DE DEUX SORTIES ON LES NOMMES OutX // (pitié que ça n'arrive pas :'( )
                choicePort.portName = $"Out{_outPortIndex}";
                _outPortIndex++;
            }
        }
        else
        {
            choicePort.portName = "IF";
        }

        choicePort.MarkDirtyRepaint();
        RefreshExpandedState();
        MarkDirtyRepaint();

        return choicePort;
    }



private Port CreateSingleChoicePortNew(string dropDownKey, Direction portDirection)
{
    DSChoiceSaveData newChoice = new DSChoiceSaveData();
    newChoice.SaveDropDownKeyChoice(dropDownKey);

    Saves.ChoicesInNode.Add(newChoice);

    return CreateSingleChoicePortForExisting(newChoice, portDirection);
}
}
