using System;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
using UnityEngine.UIElements;


public static class DSElementUtility
{
    
    /// <summary>
    /// CE SCRIPT PERMET DE CREER DES ELEMENTS UI BASIQUES COMME DES BOUTONS, FOLDOUT, TEXTFIELD, ETC... // TRES PRATIQUE DE PASSER PAR ICI POUR CREER DES ELEMENTS UI PLUTOT QUE DE LE FAIRE A LA MANO DANS VOTRE SCRIPT NODE //
    /// </summary>
    /// <param name="text"></param>
    /// <param name="onClick"></param>
    /// <returns></returns>
    
    
    public static Button CreateButton(string text, Action onClick = null)
    {
        Button button = new Button(onClick)
        {
            text = text
        };

        return button;
    }

    public static Foldout CreateFoldout(string title, bool collapsed = false)
    {
        Foldout foldout = new Foldout()
        {
            text = title,
            value = !collapsed
        };

        return foldout;
    }

    public static Port CreatePort(this DSNode node, string portName = "",
        Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Output,
        Port.Capacity capacity = Port.Capacity.Single)
    {
        Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));

        port.portName = portName;

        return port;
    }

    public static TextField CreateTextField(string value = null, string label = null,
        EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        TextField textField = new TextField()
        {
            value = value,
            label = label
        };

        if (onValueChanged != null)
        {
            textField.RegisterValueChangedCallback(onValueChanged);
        }

        return textField;
    }

    public static TextField CreateTextArea(string value = null, string label = null,
        EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        TextField textArea = CreateTextField(value, label, onValueChanged);

        textArea.multiline = true;

        return textArea;
    }

    public static DropdownField CreateDropdownField(string value = null, string label = null)
    {
        DropdownField dropdownField = new DropdownField()
        {
            value = value,
            label = label
        };
        return dropdownField;
    }

    public static DropdownField CreateDropdownArea(string value = null, string label = null,
        EventCallback<ChangeEvent<string>> onValueChanged = null)
    {
        DropdownField dropdownField = CreateDropdownField(value, label);

        return dropdownField;
    }

    public static Label CreateLabelField (string value = null, string label = null)
    {
        Label LabelField = new Label()
        {
            text = value,
        };
        return LabelField;
    }
    public static Label CreateLabelArea(string value)
    {
        Label sentence = CreateLabelField(value);
        
        return sentence;
    }
}