using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BugsDatabase bugsDatabase;

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

        StopAllCoroutines();
        StartCoroutine(GameplayLoop(entry));
    }

    private IEnumerator GameplayLoop(CharacterEntry entry)
    {
        Debug.Log($"Démarrage du gameplay pour {entry.perso} (id {entry.id}).");
        while (true)
        {
            // TODO : ajouter la logique de gameplay ici
            yield return null;
        }
    }
}