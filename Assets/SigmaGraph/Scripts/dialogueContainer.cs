using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dialogueContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Button _nextButton;

    //public TextMeshProUGUI Dialogue_tmp => dialogueText;

    private GameObject CensorParent;


    public void InitializeDialogueContainer(DialogueManager dManager, string dialogue, string speakerName, Sprite characterSprite, bool IsMultipleChoice = false)
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        childContainer.gameObject.SetActive(true);

        if(IsMultipleChoice)
            _nextButton?.gameObject.SetActive(false);
        else
            _nextButton?.gameObject.SetActive(true);

        if (characterImage && characterSprite) characterImage.sprite = characterSprite;

                dialogueText.SetText(dialogue);
        dManager.CheckCensorship(ref CensorParent, dialogueText, parent: childContainer.transform);

        speakerNameText.SetText(speakerName);
    }
    
    public void HideContainer()
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        childContainer.gameObject.SetActive(false);
        _nextButton?.gameObject.SetActive(false);
    }

    public void OnClicNextButton(UnityEngine.Events.UnityAction Func)
    {
         _nextButton.onClick.AddListener(Func);
    }
}
