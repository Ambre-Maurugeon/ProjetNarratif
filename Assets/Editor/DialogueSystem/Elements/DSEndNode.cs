using System;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine;
using UnityEngine.UIElements;


public class DSEndNode : DSNode
{
        public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize("END", dsGraphView, position);
            DialogueType = DSDialogueType.End;
        }

public override void Draw(Color color)
{
    base.Draw(new Color(0.8f, 0.2f, 0.2f));
    
    if (Saves.ChoicesInNode.Count > 0)
    {
        foreach (DSChoiceSaveData choiceData in Saves.ChoicesInNode)
        {
            Port output = CreateSingleChoicePortForExisting(choiceData);
           inputContainer.Add(output);
        }
    }
    else
    {
        Port output = CreateSingleChoicePortNew();
        inputContainer.Add(output);
    }


    RefreshExpandedState();
}

   
private Port CreateSingleChoicePortForExisting(DSChoiceSaveData choiceData)
{
    Port choicePort = this.CreatePort("", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
    
    choicePort.userData = choiceData;

    inputContainer.Add(choicePort);
    choicePort.MarkDirtyRepaint();
    RefreshExpandedState();
    MarkDirtyRepaint();

    return choicePort;
}


private Port CreateSingleChoicePortNew(string dropDownKey = "")
{
    DSChoiceSaveData newChoice = new DSChoiceSaveData();
    newChoice.SaveDropDownKeyChoice(dropDownKey);

    Saves.ChoicesInNode.Add(newChoice);

    return CreateSingleChoicePortForExisting(newChoice);
}

}
