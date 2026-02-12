using System.Runtime.CompilerServices;
using NaughtyAttributes.Test;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using UnityEditor.SearchService;
using System.Collections;

public class MainEvents : MonoBehaviour
{
    private Transform _othersTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _othersTransform = _TV.gameObject.transform.parent;
    }

    // Pass Or Match
    #region PassOrMatch

    [Serializable]
    struct PassOrMatch
    {
        public GameObject passBtn;
        public GameObject matchBtn;
    }
    [Header("PassOrMatch")]
    [SerializeField] private PassOrMatch _passOrMatchBtns;

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

    [Header("GameOver")]
    [SerializeField] private GameObject _gameOverCanvas;

    public void FakeCrash() {
        _gameOverCanvas.SetActive(true);
        Invoke("LoadMenu",3f);
    }

    private void LoadMenu() => SceneManager.LoadScene("Menu");
    #endregion

    //Decors
    #region Decors

    [Header("Décors")]
    [SerializeField] private Image _TV;
    [SerializeField] private Image _background;

    // TV
    public void PutOnTV(Sprite image) => _TV.sprite = image;

    #endregion

    // Sequences
    #region Sequences

    [Serializable]
    struct Romemouche
    {
        public Sprite scene01;
        public Sprite scene02;
        public Sprite scene03;
    }

    [SerializeField] private Romemouche _romemoucheScenes;

    public void PlaySequenceRomemouche()
    {
        StartCoroutine("LaunchRomemouche");
    }

    private IEnumerator LaunchRomemouche()
    {
        ChangeImage(_background, _romemoucheScenes.scene01);
        yield return new WaitForSeconds(1.5f);
        ChangeImage(_background, _romemoucheScenes.scene02);
        yield return new WaitForSeconds(1.5f);
        ChangeImage(_background, _romemoucheScenes.scene03);
    }

    private void ChangeImage(Image from, Sprite to)
    {
        from.sprite = to;
    }
    #endregion

    // Anims
    #region Anims
    [Header("Anims")]
    [SerializeField] GameObject _flash;

    public void Flash()
    {
        GameObject flash = Instantiate(_flash, _othersTransform);
        Destroy(flash, 0.5f);
    }

    #endregion
     
}
