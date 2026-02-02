using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BugsDatabase bugsDatabase;
    [SerializeField] private GameObject transitionPrefab;

    public static GameManager Instance { get; private set; }
    public int currentInsectId = -1;
    public SceneAsset SceneToLoad;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void CheckSuccess()
    {
        Debug.Log("Vérification de la réussite...");
    }

    public void OnStartInsect(int id)
    {
        if (bugsDatabase == null)
        {
            Debug.LogError("BugsDatabase non assignée dans l'inspector.");
            return;
        }

        if (bugsDatabase.entries == null)
        {
            Debug.LogError("La liste d'entrées est nulle dans BugsDatabase.");
            return;
        }

        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == id);
        if (entry == null)
        {
            Debug.LogWarning($"Aucun insecte trouvé avec id {id}.");
            return;
        }

        currentInsectId = id;
        StartCoroutine(StartInsectCoroutine());
    }

    private IEnumerator StartInsectCoroutine()
    {
        var transition = FindObjectOfType<TransitionScene>();

        if (transition != null && !transition.gameObject.activeInHierarchy)
        {
            transition.gameObject.SetActive(true);
        }

        if (transition == null && transitionPrefab != null)
        {
            var go = Instantiate(transitionPrefab);
            if (go != null)
            {
                go.SetActive(true);
                transition = go.GetComponent<TransitionScene>();
            }
        }

        if (transition != null)
        {
            yield return StartCoroutine(transition.FadeIn());
        }

        SceneManager.LoadScene(SceneToLoad.name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentInsectId < 0)
            return;

        if (bugsDatabase == null || bugsDatabase.entries == null)
        {
            return;
        }

        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry == null)
        {
            return;
        }

        var transition = FindObjectOfType<TransitionScene>();
        if (transition == null && transitionPrefab != null)
        {
            var go = Instantiate(transitionPrefab);
            if (go != null)
            {
                go.SetActive(true);
                Debug.Log("TransitionScene instancié dans la nouvelle scène.");
                transition = go.GetComponent<TransitionScene>();
            }
        }

        if (transition != null)
        {
            StartCoroutine(HandleSceneLoadedWithFadeOut(entry, transition));
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(GameplayLoop(entry));
        }
    }

    private IEnumerator HandleSceneLoadedWithFadeOut(CharacterEntry entry, TransitionScene transition)
    {
        if (!transition.gameObject.activeInHierarchy)
            transition.gameObject.SetActive(true);

        yield return StartCoroutine(transition.FadeOut());

        StopAllCoroutines();
        StartCoroutine(GameplayLoop(entry));
    }

    private IEnumerator GameplayLoop(CharacterEntry entry)
    {
        while (true)
        {
            var dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogWarning("DialogueManager introuvable dans la scène.");
            }
            else
            {
                if (entry.refDialogue == null)
                {
                    Debug.LogWarning($"L'entrée {entry.perso} n'a pas de refDialogue assigné.");
                }
                else
                {
                    dialogueManager.runtimeGraph = entry.refDialogue;
                }
            }
            yield return null;
        }
    }
}