using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class buttonChoiceController : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI buttonText;
    
    private bool lockState = false;
    
    private Button _button;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _button = GetComponent<Button>();
    }

    public void InitializeButtonChoiceController(bool fillCondition, string text)
    {
        lockState = !fillCondition;
        buttonText.SetText(text);
        if(_button == null) _button = GetComponent<Button>();
        _button.interactable = !lockState;
        
        if(_animator == null) _animator = GetComponent<Animator>();
        _animator.SetBool("Locked", lockState);
    }
    
    public void OnClicked()
    {
        _animator.SetTrigger("Clicked");

        if (lockState)
        {
            return;
        }
    }

}
