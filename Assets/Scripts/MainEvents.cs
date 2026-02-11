using System.Runtime.CompilerServices;
using NaughtyAttributes.Test;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainEvents : MonoBehaviour
{
    [System.Serializable]
    struct PassOrMatch
    {
        public GameObject passBtn;
        public GameObject matchBtn;
    }

    [SerializeField] private PassOrMatch _passOrMatchBtns;

    [SerializeField] private GameObject _gameOverCanvas;


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
}
