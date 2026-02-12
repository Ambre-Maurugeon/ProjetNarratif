using System;
using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class buttonChoiceController : MonoBehaviour
{
    // Button
    [SerializeField] private TextMeshProUGUI buttonText;

    private Button _button;
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

    public void InitializeButtonChoiceController(DialogueManager dManager, bool fillCondition, string text)
    {
        _lockState = !fillCondition;

        if (!_lockState)
        {
            buttonText.SetText(text);

            dManager.CheckCensorship(ref CensorParent, buttonText, parent: transform);
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
