using System.Runtime.CompilerServices;
using NaughtyAttributes.Test;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class MainEvents : MonoBehaviour
{
    [Serializable]
    struct PassOrMatch
    {
        public GameObject passBtn;
        public GameObject matchBtn;
    }
    [Header("PassOrMatch")]
    [SerializeField] private PassOrMatch _passOrMatchBtns;

    [Header("GameOver")]
    [SerializeField] private GameObject _gameOverCanvas;

    [Header("Décors")]
    [SerializeField] private Image _TV;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Pass Or Match
    #region PassOrMatch

    public void PassOnly() => LockButton(true, _passOrMatchBtns.matchBtn);
    public void MatchOnly() => LockButton(true, _passOrMatchBtns.passBtn);

    private void LockButton(bool lockState, GameObject buttonGO)
    {
        // interact
        Button button = buttonGO.GetComponent<Button>();
        button.interactable = !lockState;

        //anim lock (if activate)
        Animator animator = buttonGO.GetComponent<Animator>();
        animator.SetBool("Locked", lockState);

        //if deactivate (secu)
        buttonChoiceController controller = buttonGO.GetComponent<buttonChoiceController>();
        controller.Lock(lockState);
    }

    #endregion

    // GameOver
    #region GameOver
    public void FakeCrash() {
        _gameOverCanvas.SetActive(true);
        Invoke("LoadMenu",3f);
    }

    private void LoadMenu() => SceneManager.LoadScene("Menu");
    #endregion

    //Decors
    #region Decors

    // TV
    public void PutOnTV(Sprite image) => _TV.sprite = image;

    #endregion
}
