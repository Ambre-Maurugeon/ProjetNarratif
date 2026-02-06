using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dialogueContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image characterImage;
    
    
    public void InitializeDialogueContainer(string dialogue, string speakerName, Sprite characterSprite)
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        childContainer.gameObject.SetActive(true);
        if(characterImage != null) characterImage.sprite = characterSprite;
        
        dialogueText.SetText(dialogue);
        speakerNameText.SetText(speakerName);
    }
    
    public void HideContainer()
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        childContainer.gameObject.SetActive(false);
    }
}
