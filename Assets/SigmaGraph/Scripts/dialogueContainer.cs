using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class dialogueContainer : MonoBehaviour
{
    [System.Serializable]
    struct Background
    {
        public Image backgroundImg;
        public Sprite darkSprite;
    }
    [SerializeField] private Background _background;

    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image characterImage;
    [SerializeField] public Button nextButton;

    //public TextMeshProUGUI Dialogue_tmp => dialogueText;

    private GameObject CensorParent;

    private void Start()
    {
        if(MainEvents.IsDarkLevel)
            SetDarkAssets();
    }

    private void OnEnable()
    {
        MainEvents e = FindAnyObjectByType<MainEvents>();
        if (e != null)
            e.OnDarkLevel += SetDarkAssets;
    }

    private void OnDisable()
    {
        MainEvents e = FindAnyObjectByType<MainEvents>();
        if (e != null)
            e.OnDarkLevel -= SetDarkAssets;
    }

    private void SetDarkAssets()
    {
        //background
        if(_background.darkSprite !=null)
        _background.backgroundImg.sprite = _background.darkSprite;

        //next button
        nextButton.gameObject.GetComponent<Image>().sprite = (Sprite)Resources.Load("GA/UI/Dark/UI_fleche_sérieux", typeof(Sprite)); 
    }


    public void InitializeDialogueContainer(DialogueManager dManager, string dialogue, string speakerName, Sprite characterSprite, bool IsMultipleChoice = false)
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        childContainer.gameObject.SetActive(true);

        if(IsMultipleChoice)
            nextButton?.gameObject.SetActive(false);
        else
            nextButton?.gameObject.SetActive(true);

        if (characterImage && characterSprite) characterImage.sprite = characterSprite;

                dialogueText.SetText(dialogue);
        dManager.CheckCensorship(ref CensorParent, dialogueText, parent: childContainer.transform);

        speakerNameText?.SetText(speakerName);
    }
    
    public void HideContainer()
    {
        var childContainer = transform.GetChild(0);
        if (childContainer == null) return;
        childContainer.gameObject.SetActive(false);
        nextButton?.gameObject.SetActive(false);
    }

    public void OnClicNextButton(UnityEngine.Events.UnityAction Func)
    {
         nextButton.onClick.AddListener(Func);
    }
}
