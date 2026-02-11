using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameManagerData
{
    public static int SavedCurrentInsectId = 0;
    public static BugsDatabase SavedBugsDatabase = null;
    public static SceneAsset SavedSceneToLoad = null;
    public static SceneAsset SavedSceneHub = null;
    public static GameObject SavedTransitionPrefab = null;
    public static Canvas SavedSequenceCanvas = null;
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private BugsDatabase bugsDatabase;
    [SerializeField] private GameObject transitionPrefab;

    public static GameManager Instance { get; private set; }
    public int currentInsectId = 0;
    public SceneAsset SceneToLoad;
    public SceneAsset SceneHub;
    private Button DateBtn;
    private Button NextButton;
    private Button PrevButton;
    private Button Date1Btn;
    private Button Date2Btn;
    private Button Date3Btn;
    private Button Date4Btn;
    public Canvas UiCanva;
    public Canvas SequenceCanvas;

    private Coroutine sequenceCoroutine;
    private ImageAnimation sceneImageAnim;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("GameManager créé - première instance");
        }
        else
        {
            Debug.Log("GameManager détecté - transfert des données");
            GameManager oldInstance = Instance;
            
            TransferDataFromPreviousInstance(oldInstance, this);
            
            Instance = this;
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Destroy(oldInstance.gameObject);
            
            Debug.Log("Ancien GameManager détruit, nouveau en place");
        }
    }

    private void Start()
    {
        RestoreSavedData();
    }

    private void OnEnable()
    {
        Pulse.OnEndRythm += StopRythmGame;
        DialogueManager.OnDialogueEnd += ReturnToHub;
    }

    private void OnDisable()
    {
        Pulse.OnEndRythm -= StopRythmGame;
        DialogueManager.OnDialogueEnd -= ReturnToHub;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveData();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Debug.Log("GameManager détruit - données sauvegardées");
        }
    }

    private void SaveData()
    {
        Debug.Log("Sauvegarde des données du GameManager");
        GameManagerData.SavedCurrentInsectId = this.currentInsectId;
        GameManagerData.SavedBugsDatabase = this.bugsDatabase;
        GameManagerData.SavedSceneToLoad = this.SceneToLoad;
        GameManagerData.SavedSceneHub = this.SceneHub;
        GameManagerData.SavedTransitionPrefab = this.transitionPrefab;
        GameManagerData.SavedSequenceCanvas = this.SequenceCanvas;
    }

    private void RestoreSavedData()
    {
        if (GameManagerData.SavedBugsDatabase != null)
        {
            Debug.Log("Restauration des données du GameManager");
            this.currentInsectId = GameManagerData.SavedCurrentInsectId;
            this.bugsDatabase = GameManagerData.SavedBugsDatabase;
            this.SceneToLoad = GameManagerData.SavedSceneToLoad;
            this.SceneHub = GameManagerData.SavedSceneHub;
            this.transitionPrefab = GameManagerData.SavedTransitionPrefab;
            this.SequenceCanvas = GameManagerData.SavedSequenceCanvas;
            
            var bugsDbField = typeof(GameManager).GetField("bugsDatabase", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (bugsDbField != null)
                bugsDbField.SetValue(this, GameManagerData.SavedBugsDatabase);
            
            var transitionField = typeof(GameManager).GetField("transitionPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (transitionField != null)
                transitionField.SetValue(this, GameManagerData.SavedTransitionPrefab);
            
            Debug.Log($"Insecte restauré: {this.currentInsectId}");
        }
    }

    private void TransferDataFromPreviousInstance(GameManager previousInstance, GameManager newInstance)
    {
        if (previousInstance == null || newInstance == null) return;

        newInstance.currentInsectId = previousInstance.currentInsectId;
        newInstance.bugsDatabase = previousInstance.bugsDatabase;
        newInstance.SceneToLoad = previousInstance.SceneToLoad;
        newInstance.SceneHub = previousInstance.SceneHub;
        newInstance.SequenceCanvas = previousInstance.SequenceCanvas;
        
        var bugsDbField = typeof(GameManager).GetField("bugsDatabase", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (bugsDbField != null)
            bugsDbField.SetValue(newInstance, previousInstance.bugsDatabase);
        
        var transitionField = typeof(GameManager).GetField("transitionPrefab", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (transitionField != null)
            transitionField.SetValue(newInstance, previousInstance.transitionPrefab);
    }


    public void OnStartInsect(int id)
    {
        if (bugsDatabase == null || bugsDatabase.entries == null) return;
        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == id);
        if (entry == null || entry.isCompleted) return;
        currentInsectId = id;
        StartCoroutine(StartInsectCoroutine());
    }

    private IEnumerator StartInsectCoroutine()
    {
        var transition = FindFirstObjectByType<TransitionScene>();

        if (transition == null && transitionPrefab != null)
        {
            var go = Instantiate(transitionPrefab);
            if (go != null)
            {
                go.SetActive(true);
                transition = FindFirstObjectByType<TransitionScene>();
            }
        }

        if (transition != null)
            yield return StartCoroutine(transition.FadeIn());

        SceneManager.LoadScene(SceneToLoad.name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AreAllEntriesCompleted();
        if (sceneImageAnim != null)
            sceneImageAnim.GetImage();

        if (UiCanva == null)
        {
            if (GameObject.Find("UICanva") != null)
                UiCanva = GameObject.Find("UICanva").GetComponent<Canvas>();
        }

        if (SequenceCanvas == null)
        {
            if (GameObject.Find("SequenceCanvas") != null)
                SequenceCanvas = GameObject.Find("SequenceCanvas").GetComponent<Canvas>();
        }

        UpdateBug();

        if (currentInsectId < 0 || bugsDatabase == null || bugsDatabase.entries == null) return;

        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry == null) return;

        var transition = FindFirstObjectByType<TransitionScene>();
        if (transition == null && transitionPrefab != null)
        {
            var go = Instantiate(transitionPrefab);
            if (go != null)
            {
                go.SetActive(true);
                transition = FindFirstObjectByType<TransitionScene>();
            }
        }

        if (transition != null)
            StartCoroutine(HandleSceneLoadedWithFadeOut(entry, transition));
        else
        {
            StopAllCoroutines();
            StartCoroutine(GameplayLoop(entry));
        }
    }

    private ImageAnimation FindSceneImageAnimation()
    {
        if (SequenceCanvas != null)
        {
            var seqTf = SequenceCanvas.transform.Find("SequencePlayer");
            Instantiate(SequenceCanvas);
            if (seqTf != null)
            {
                var anim = seqTf.GetComponentInChildren<ImageAnimation>();
                if (anim != null) return anim;
            }
        }

        return FindFirstObjectByType<ImageAnimation>();
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
            var dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager != null)
            {
                if (entry.refDialogue != null)
                {
                }
            }
            yield return null;
        }
    }

    public void ReturnToHub()
    {
        var transition = FindFirstObjectByType<TransitionScene>();

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
            StartCoroutine(transition.FadeIn());

        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry != null) entry.isCompleted = true;

        SceneManager.LoadScene(SceneHub.name);
    }

    public void LaunchRythmGame()
    {
        var pulse = FindFirstObjectByType<Pulse>();
        pulse?.StartSequence();
        var DManager = FindFirstObjectByType<DialogueManager>();
        if (DManager != null) DManager.CanInteract = false;
    }

    public void StopRythmGame()
    {
        var DManager = FindFirstObjectByType<DialogueManager>();
        if (DManager != null) DManager.CanInteract = true;
    }

    public void NextInsect()
    {
        if (bugsDatabase == null || bugsDatabase.entries == null || bugsDatabase.entries.Count == 0) return;

        int nextId = int.MaxValue;
        bool found = false;

        foreach (var e in bugsDatabase.entries)
        {
            if (e == null) continue;
            if (e.id > currentInsectId && e.id < nextId)
            {
                nextId = e.id;
                found = true;
            }
        }

        if (!found) return;

        currentInsectId = nextId;
        UpdateBug();
    }

    public void PrevInsect()
    {
        if (bugsDatabase == null || bugsDatabase.entries == null || bugsDatabase.entries.Count == 0) return;

        int prevId = int.MinValue;
        bool found = false;

        foreach (var e in bugsDatabase.entries)
        {
            if (e == null) continue;
            if (e.id < currentInsectId && e.id > prevId)
            {
                prevId = e.id;
                found = true;
            }
        }

        if (!found) return;

        currentInsectId = prevId;
        UpdateBug();
    }

    private void UpdateBug()
    {
        if (bugsDatabase == null || bugsDatabase.entries == null) return;
        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry == null) return;

        if (DateBtn == null)
        {
            if (GameObject.Find("DateBtn") != null)
                DateBtn = GameObject.Find("DateBtn").GetComponent<Button>();
        }
        if (DateBtn != null)
        {
            DateBtn.onClick.RemoveAllListeners();
            DateBtn?.onClick.AddListener(() => OnStartInsect(currentInsectId));
        }

        if (NextButton == null)
        {
            if (GameObject.Find("NextBtn") != null)
                NextButton = GameObject.Find("NextBtn").GetComponent<Button>();
        }
        if (NextButton != null)
        {
            NextButton.onClick.RemoveAllListeners();
            NextButton?.onClick.AddListener(() => NextInsect());
        }

        if (PrevButton == null)
        {
            if (GameObject.Find("PrevBtn") != null)
                PrevButton = GameObject.Find("PrevBtn").GetComponent<Button>();
        }
        if (PrevButton != null)
        {
            PrevButton.onClick.RemoveAllListeners();
            PrevButton?.onClick.AddListener(() => PrevInsect());
        }
        
        if (Date1Btn == null)
        {
            if (GameObject.Find("Date1Btn") != null)
                Date1Btn = GameObject.Find("Date1Btn").GetComponent<Button>();
        }
        if (Date1Btn != null)
        {
            Date1Btn.onClick.RemoveAllListeners();
            Date1Btn?.onClick.AddListener(() => SetCurrentInsectId(0));
        }
        
        if (Date2Btn == null)
        {
            if (GameObject.Find("Date2Btn") != null)
                Date2Btn = GameObject.Find("Date2Btn").GetComponent<Button>();
        }
        if (Date2Btn != null)        {
            Date2Btn.onClick.RemoveAllListeners();
            Date2Btn?.onClick.AddListener(() => SetCurrentInsectId(1));
        }
        
        if (Date3Btn == null)
        {
            if (GameObject.Find("Date3Btn") != null)
                Date3Btn = GameObject.Find("Date3Btn").GetComponent<Button>();
        }
        if (Date3Btn != null)        {
            Date3Btn.onClick.RemoveAllListeners();
            Date3Btn?.onClick.AddListener(() => SetCurrentInsectId(2));
        }
        
        if (Date4Btn == null)
        {
            if (GameObject.Find("Date4Btn") != null)
                Date4Btn = GameObject.Find("Date4Btn").GetComponent<Button>();
        }
        if (Date4Btn != null)        {
            Date4Btn.onClick.RemoveAllListeners();
            Date4Btn?.onClick.AddListener(() => SetCurrentInsectId(3));
        }

        if (UiCanva != null)
        {
            var textTf = UiCanva.transform.Find("Name");
            if (textTf != null)
            {
                var text = textTf.GetComponent<TMPro.TextMeshProUGUI>();
                if (text != null && entry != null) text.text = entry.perso;
            }

            var spriteTf = UiCanva.transform.Find("Sprite");
            if (spriteTf != null)
            {
                var img = spriteTf.GetComponent<Image>();
                if (img != null && entry != null && entry.characterSprite != null)
                    img.sprite = entry.characterSprite;
            }

            for (int i = 0; i < 3; i++)
            {
                var descTf = UiCanva.transform.Find($"Desc {i + 1}");
                if (descTf == null) continue;

                var descText = descTf.GetComponent<TMPro.TextMeshProUGUI>();
                if (descText == null) continue;

                string value = "";
                if (entry != null && entry.description != null && entry.description.Length > i && !string.IsNullOrEmpty(entry.description[i]))
                    value = entry.description[i];

                descText.text = value;
            }

            var sprite = UiCanva.transform.Find("BgText");
            if (sprite != null)
            {
                var img = sprite.GetComponent<Image>();
                if (img != null && entry != null && entry.BgSprite != null)
                    img.sprite = entry.BgSprite;
            }
        }
    }

    public void SetCurrentInsectId(int id)
    {
        currentInsectId = id;
        UpdateBug();
    }

    private bool FindValidSequenceData(CharacterEntry entry, int startIdx, out int foundIdx, out Sequence seq)
    {
        foundIdx = -1;
        seq = default;
        if (entry == null || entry.Sequences == null || entry.Sequences.Length == 0) return false;
        int len = entry.Sequences.Length;
        for (int i = 0; i < len; i++)
        {
            int idx = (startIdx + i) % len;
            var s = entry.Sequences[idx];
            if (s.sprites != null && s.sprites.Length > 0)
            {
                foundIdx = idx;
                seq = s;
                return true;
            }
        }
        return false;
    }

    public void PlayCurrentSequenceNow(Sprite firstSprite)
    {
        var DManager = FindFirstObjectByType<DialogueManager>();
        if (DManager != null) DManager.CanInteract = false;
        var entry = bugsDatabase?.entries?.Find(e => e != null && e.id == currentInsectId);
        if (entry == null) return;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        int playIndex = entry.sequenceIndex;

        if (firstSprite != null && entry.Sequences != null && entry.Sequences.Length > 0)
        {
            for (int i = 0; i < entry.Sequences.Length; i++)
            {
                var seq = entry.Sequences[i];
                if (seq.sprites != null && seq.sprites.Length > 0 && seq.sprites[0] == firstSprite)
                {
                    playIndex = i;
                    break;
                }
            }
        }

        sequenceCoroutine = StartCoroutine(InternalPlaySequence(entry, playIndex));
    }

    private IEnumerator InternalPlaySequence(CharacterEntry entry, int playIndex)
    {
        if (entry == null)
        {
            sequenceCoroutine = null;
            yield break;
        }

        if (entry.Sequences != null && entry.Sequences.Length > 0)
        {
            int foundIdx = -1;
            Sequence seq = default;

            int clamped = Mathf.Clamp(playIndex, 0, entry.Sequences.Length - 1);
            seq = entry.Sequences[clamped];

            if (seq.sprites == null || seq.sprites.Length == 0)
            {
                if (!FindValidSequenceData(entry, playIndex, out foundIdx, out seq))
                {
                    entry.sequenceIndex = (entry.sequenceIndex + 1) % entry.Sequences.Length;
                    sequenceCoroutine = null;
                    yield break;
                }
            }
            else
            {
                foundIdx = clamped;
            }

            if (sceneImageAnim == null)
                sceneImageAnim = FindSceneImageAnimation();

            if (sceneImageAnim == null)
            {
                entry.sequenceIndex = (foundIdx + 1) % entry.Sequences.Length;
                sequenceCoroutine = null;
                yield break;
            }

            sceneImageAnim.Stop(true);
            sceneImageAnim.ApplySequence(seq);
            sceneImageAnim.GetImage();

            bool animEnded = false;
            System.Action handler = () =>
            {
                animEnded = true;
                entry.sequenceIndex = (foundIdx + 1) % entry.Sequences.Length;
                Destroy(sceneImageAnim.gameObject);
            };
            sceneImageAnim.AnimationEnded += handler;

            sceneImageAnim.Play(true);

            yield return new WaitUntil(() => animEnded);

            sceneImageAnim.AnimationEnded -= handler;

            sceneImageAnim.Stop(true);

            sequenceCoroutine = null;
            entry.sequenceIndex = (foundIdx + 1) % entry.Sequences.Length;
            yield break;
        }

        sequenceCoroutine = null;
        yield break;
    }

    private void OnApplicationQuit()
    {
        ResetCompletionFlags();
    }

    private void ResetCompletionFlags()
    {
        if (bugsDatabase == null || bugsDatabase.entries == null) return;

        foreach (var entry in bugsDatabase.entries)
        {
            if (entry != null)
                entry.isCompleted = false;
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(bugsDatabase);
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    public bool AreAllEntriesCompleted()
    {
        if (bugsDatabase == null || bugsDatabase.entries == null) return false;
        foreach (var entry in bugsDatabase.entries)
        {
            if (entry == null) continue;
            if (!entry.isCompleted) return false;
        }
        Debug.Log("All entries completed!");
        return true;
    }

    public void QuitGame() 
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
    #endif
    }
    public void Play() => SceneManager.LoadScene("Hub_Scene", LoadSceneMode.Single);
}

