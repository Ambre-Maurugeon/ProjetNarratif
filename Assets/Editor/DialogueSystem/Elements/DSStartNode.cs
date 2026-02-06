using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


public class DSStartNode : DSNode
{
    public override void Initialize(string nodeName, DSGraphView dsGraphView, Vector2 position)
    {
        base.Initialize("START", dsGraphView, position);
        DialogueType = DSDialogueType.Start;
    } 
    
    public override void Draw(Color color)
    {
        base.Draw(new Color(0.0f, 0.5f, 0.0f));
        
        if (Saves.ChoicesInNode.Count > 0)
        {
            foreach (DSChoiceSaveData choiceData in Saves.ChoicesInNode)
            {
                var output = CreateSingleChoicePortForExisting(choiceData);
                outputContainer.Add(output);

            }
        }
        else
        {
            var output =CreateSingleChoicePortNew();
            outputContainer.Add(output);

        }


        RefreshExpandedState();
    }

   
private Port CreateSingleChoicePortForExisting(DSChoiceSaveData choiceData)
{
    Port choicePort = this.CreatePort();
    choicePort.userData = choiceData;

    outputContainer.Add(choicePort);

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
