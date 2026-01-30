using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BugsDatabase bugsDatabase;

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
        // temps de transition lancement de scène
        SceneManager.LoadScene(SceneToLoad.name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentInsectId < 0)
            return;

        if (bugsDatabase == null || bugsDatabase.entries == null)
        {
            Debug.LogWarning("BugsDatabase non disponible après chargement de la scène.");
            return;
        }

        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry == null)
        {
            Debug.LogWarning($"Aucun insecte trouvé avec id {currentInsectId} après chargement de la scène.");
            return;
        }

        StopAllCoroutines();
        // fin de transition lancement de scène
        StartCoroutine(GameplayLoop(entry));
    }

    private IEnumerator GameplayLoop(CharacterEntry entry)
    {
        Debug.Log($"Démarrage du gameplay pour {entry.perso} (id {entry.id}).");
        while (true)
        {
            // ajouter la logique de gameplay ici
            yield return null;
        }
    }
}