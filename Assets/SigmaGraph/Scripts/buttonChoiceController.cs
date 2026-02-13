using System;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
#if UNITY_EDITOR
using UnityEditor.Rendering;
#endif
using UnityEngine;
using UnityEngine.UI;

public class buttonChoiceController : MonoBehaviour
{
    // Button
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private RawImage _rawLock;

    private Button _button;

    //Sprites
    [Serializable]
    struct BtnSprites {
        public Sprite darkBtn;
        public Sprite darkLock;
    }

    [SerializeField] BtnSprites _btnSprites;

    // Anims
    private Animator _animator;

    private bool _lockState = false;

    // Text
    private GameObject CensorParent;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _button = GetComponent<Button>();

        //Init
        Lock(_lockState);
    }

    private void Start()
    {
        // DarkLevel
        if (MainEvents.IsDarkLevel)
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
        //img
        _button.gameObject.GetComponent<Image>().sprite = _btnSprites.darkBtn;
        //chains
        _rawLock.texture = _btnSprites.darkLock.texture;
    }

    public void InitializeButtonChoiceController(DialogueManager dManager, bool fillCondition, string text)
    {
        _lockState = !fillCondition;

        if (!_lockState)
        {
            _buttonText.SetText(text);

            dManager.CheckCensorship(ref CensorParent, _buttonText, parent: transform);
        }

        if(_button == null) _button = GetComponent<Button>();
        _button.interactable = !_lockState;

        if (_animator == null) _animator = GetComponent<Animator>();
        _animator.SetBool("Locked", _lockState);

    }

    public void Lock(bool lockState)
    {
        _lockState = lockState;

        if (_button == null) _button = GetComponent<Button>();
        _button.interactable = !lockState;

        if (_animator == null) _animator = GetComponent<Animator>();
        _animator.SetBool("Locked", lockState);
    }
    
    public void OnClicked()
    {
        _animator.SetTrigger("Clicked");

        if (_lockState) return;
    }

}
