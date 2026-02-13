using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameManagerData
{
    public static int SavedCurrentInsectId = 0;
    public static BugsDatabase SavedBugsDatabase = null;
#if UNITY_EDITOR
    public static SceneAsset SavedSceneToLoad = null;
    public static SceneAsset SavedSceneHub = null;
#endif
    public static GameObject SavedTransitionPrefab = null;
    public static Canvas SavedSequenceCanvas = null;
    public static GameObject SavedGlitchEffectPrefab = null;
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private BugsDatabase bugsDatabase;
    [SerializeField] private GameObject transitionPrefab;

    public static GameManager Instance { get; private set; }
    public int currentInsectId = 0;
#if UNITY_EDITOR
    public SceneAsset SceneToLoad;
    public SceneAsset SceneHub;
#endif
    private Button DateBtn;
    private Button NextButton;
    private Button PrevButton;
    private Button Date1Btn;
    private Button Date2Btn;
    private Button Date3Btn;
    private Button Date4Btn;
    public Canvas UiCanva;
    public Canvas SequenceCanvas;
    public GameObject GlitchEffectPrefab;
    private GameObject glitchEffectInstance;
    public Canvas MatchCanvas;

    private Coroutine sequenceCoroutine;
    private ImageAnimation sceneImageAnim;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            GameManager oldInstance = Instance;
            
            TransferDataFromPreviousInstance(oldInstance, this);
            
            Instance = this;
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Destroy(oldInstance.gameObject);
            
        }
    }

    private void Start()
    {
        RestoreSavedData();
    }

    private void OnEnable()
    {
        Pulse.OnEndRythm += StopRythmGame;
        DialogueManager.OnDialogueEnd += MatchLaunch;
    }

    private void OnDisable()
    {
        Pulse.OnEndRythm -= StopRythmGame;
        DialogueManager.OnDialogueEnd -= MatchLaunch;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveData();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void SaveData()
    {
        GameManagerData.SavedCurrentInsectId = this.currentInsectId;
        GameManagerData.SavedBugsDatabase = this.bugsDatabase;

#if UNITY_EDITOR
        GameManagerData.SavedSceneToLoad = this.SceneToLoad;
        GameManagerData.SavedSceneHub = this.SceneHub;
#endif

        GameManagerData.SavedTransitionPrefab = this.transitionPrefab;
        GameManagerData.SavedSequenceCanvas = this.SequenceCanvas;
        GameManagerData.SavedGlitchEffectPrefab = this.GlitchEffectPrefab;
    }

    private void RestoreSavedData()
    {
        if (GameManagerData.SavedBugsDatabase != null)
        {
            this.currentInsectId = GameManagerData.SavedCurrentInsectId;
            this.bugsDatabase = GameManagerData.SavedBugsDatabase;
#if UNITY_EDITOR

            this.SceneToLoad = GameManagerData.SavedSceneToLoad;
            this.SceneHub = GameManagerData.SavedSceneHub;
#endif

            this.transitionPrefab = GameManagerData.SavedTransitionPrefab;
            this.SequenceCanvas = GameManagerData.SavedSequenceCanvas;
            this.GlitchEffectPrefab = GameManagerData.SavedGlitchEffectPrefab;
            
            var bugsDbField = typeof(GameManager).GetField("bugsDatabase", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (bugsDbField != null)
                bugsDbField.SetValue(this, GameManagerData.SavedBugsDatabase);
            
            var transitionField = typeof(GameManager).GetField("transitionPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (transitionField != null)
                transitionField.SetValue(this, GameManagerData.SavedTransitionPrefab);
            
        }
    }

    private void TransferDataFromPreviousInstance(GameManager previousInstance, GameManager newInstance)
    {
        if (previousInstance == null || newInstance == null) return;

        newInstance.currentInsectId = previousInstance.currentInsectId;
        newInstance.bugsDatabase = previousInstance.bugsDatabase;
#if UNITY_EDITOR
        newInstance.SceneToLoad = previousInstance.SceneToLoad;
        newInstance.SceneHub = previousInstance.SceneHub;
#endif
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
        if (AreAllEntriesCompleted() != true)
        {
            if (entry == null || entry.isCompleted) return;
        }
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

        if (AreAllEntriesCompleted() != true)
        {
            if (transition != null) yield return StartCoroutine(transition.FadeIn());
        }

        var AudioManager = FindFirstObjectByType<AudioManager>();
        AudioManager.PlayAudio(0);
        

#if UNITY_EDITOR
        SceneManager.LoadScene(SceneToLoad.name);
#else
        SceneManager.LoadScene("MainScene");
#endif

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestoreSavedData();
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

#if UNITY_EDITOR
        if (scene.name == SceneHub.name) UpdateBug();
#else
        if (scene.name == "Hub_Scene") UpdateBug();
#endif
        var dialogueManager = FindFirstObjectByType<DialogueManager>();
        dialogueManager.runtimeGraph = entry.refDialogue;
        
        if (transition != null)
        {
            StartCoroutine(HandleSceneLoadedWithFadeOut(entry, transition));
        }
        else
        {
            StopAllCoroutines();
        }
    }
    
    private ImageAnimation FindSceneImageAnimation()
    {
        if (SequenceCanvas != null)
        {
            var instCanvas = Instantiate(SequenceCanvas);
            if (instCanvas != null)
            {
                instCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                Camera cam = Camera.main;
                if (cam == null)
                {
                    cam = FindObjectOfType<Camera>();
                }
                instCanvas.worldCamera = cam;
                var seqTf = instCanvas.transform.Find("SequencePlayer");
                if (seqTf != null)
                {
                    var anim = seqTf.GetComponentInChildren<ImageAnimation>();
                    if (anim != null) return anim;
                }
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
    }
    

    public void ReturnToHub()
    {
        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry != null) entry.isCompleted = true;
        
        if (AreAllEntriesCompleted() == true)
        {
            OnStartInsect(4);
#if UNITY_EDITOR
            SceneManager.LoadScene(SceneToLoad != null ? SceneToLoad.name : "MainScene");
#else
            SceneManager.LoadScene("MainScene");
#endif
        }
        else
        {
            Debug.Log("Returning to hub...");
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
        
        #if UNITY_EDITOR
                SceneManager.LoadScene(SceneHub.name);
        #else
                SceneManager.LoadScene("Hub_Scene");
        #endif
            
        }
        
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

        int nextId = currentInsectId + 1;
        if (nextId > 3)
        {
            nextId = 0;
        }
        
        var AudioManager = FindFirstObjectByType<AudioManager>();
        AudioManager.PlayAudio(2);
        
        currentInsectId = nextId;
        UpdateBug();
    }

    public void PrevInsect()
    {
        if (bugsDatabase == null || bugsDatabase.entries == null || bugsDatabase.entries.Count == 0) return;

        int prevId = currentInsectId - 1;
        if (prevId < 0)
        {
            prevId = 3;
        }

        var AudioManager = FindFirstObjectByType<AudioManager>();
        AudioManager.PlayAudio(2);
        
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
            
            if (entry.isCompleted)
            {
                ColorBlock colors = DateBtn.colors;
                colors.normalColor = new Color(0.30588236f, 1, 0, 1f);
                DateBtn.colors = colors;
                
                Image btnImage = DateBtn.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = new Color(0.30588236f, 1, 0, 1f);
                }
            }
            else
            {
                ColorBlock colors = DateBtn.colors;
                colors.normalColor = new Color(1, 1, 1, 1f);
                DateBtn.colors = colors;
                
                Image btnImage = DateBtn.GetComponent<Image>();
                if (btnImage != null)
                {
                    btnImage.color = new Color(1, 1, 1, 1f);
                }
            }
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
            UpdateDateButtonColor(Date1Btn, 0);
        }
        
        if (Date2Btn == null)
        {
            if (GameObject.Find("Date2Btn") != null)
                Date2Btn = GameObject.Find("Date2Btn").GetComponent<Button>();
        }
        if (Date2Btn != null)        {
            Date2Btn.onClick.RemoveAllListeners();
            Date2Btn?.onClick.AddListener(() => SetCurrentInsectId(1));
            UpdateDateButtonColor(Date2Btn, 1);
        }
        
        if (Date3Btn == null)
        {
            if (GameObject.Find("Date3Btn") != null)
                Date3Btn = GameObject.Find("Date3Btn").GetComponent<Button>();
        }
        if (Date3Btn != null)        {
            Date3Btn.onClick.RemoveAllListeners();
            Date3Btn?.onClick.AddListener(() => SetCurrentInsectId(2));
            UpdateDateButtonColor(Date3Btn, 2);
        }
        
        if (Date4Btn == null)
        {
            if (GameObject.Find("Date4Btn") != null)
                Date4Btn = GameObject.Find("Date4Btn").GetComponent<Button>();
        }
        if (Date4Btn != null)        {
            Date4Btn.onClick.RemoveAllListeners();
            Date4Btn?.onClick.AddListener(() => SetCurrentInsectId(3));
            UpdateDateButtonColor(Date4Btn, 3);
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

    private void UpdateDateButtonColor(Button dateBtn, int insectId)
    {
        if (dateBtn == null || bugsDatabase == null || bugsDatabase.entries == null) return;
        
        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == insectId);
        if (entry == null) return;
        
        if (entry.isCompleted)
        {
            ColorBlock colors = dateBtn.colors;
            colors.normalColor = new Color(0.30588236f, 1, 0, 1f);
            dateBtn.colors = colors;
            
            Image btnImage = dateBtn.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = new Color(0.30588236f, 1, 0, 1f);
            }
        }
        else
        {
            ColorBlock colors = dateBtn.colors;
            colors.normalColor = new Color(1, 1, 1, 1f);
            dateBtn.colors = colors;
            
            Image btnImage = dateBtn.GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.color = new Color(1, 1, 1, 1f);
            }
        }
    }

    public void SetCurrentInsectId(int id)
    {
        
        var AudioManager = FindFirstObjectByType<AudioManager>();
        AudioManager.PlayAudio(2);
        
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
    public void PlayEndSequenceNow(int sequenceIndex)
    {
        var DManager = FindFirstObjectByType<DialogueManager>();
        if (DManager != null) DManager.CanInteract = false;
        var entry = bugsDatabase?.entries?.Find(e => e != null && e.id == currentInsectId);
        if (entry == null) return;

        entry.hasMatched = true;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        int playIndex = sequenceIndex;

        sequenceCoroutine = StartCoroutine(InternalPlayEndSequence(entry, playIndex));
    }
    public void PlayCurrentGlitchedSequenceNow(Sprite firstSprite)
    {
        var DManager = FindFirstObjectByType<DialogueManager>();
        if (DManager != null) DManager.CanInteract = false;
        var entry = bugsDatabase?.entries?.Find(e => e != null && e.id == currentInsectId);
        if (entry == null) return;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        GameObject glitchInstance = null;
        if (GlitchEffectPrefab != null)
        {
            glitchInstance = Instantiate(GlitchEffectPrefab);
            if (glitchInstance != null)
                glitchInstance.SetActive(true);
        }

        glitchEffectInstance = glitchInstance;

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
        
        var AudioManager = FindFirstObjectByType<AudioManager>();
        AudioManager.PlayAudio(1);
        
        sequenceCoroutine = StartCoroutine(InternalPlayGlitchSequence(entry, playIndex));
    }

    private IEnumerator InternalPlayGlitchSequence(CharacterEntry entry, int playIndex)
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
                Destroy(glitchEffectInstance.gameObject);
                var DManager = FindFirstObjectByType<DialogueManager>();
                if (DManager != null) DManager.CanInteract = true;
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
                var DManager = FindFirstObjectByType<DialogueManager>();
                if (DManager != null) DManager.CanInteract = true;
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
    private IEnumerator InternalPlayEndSequence(CharacterEntry entry, int playIndex)
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
                var DManager = FindFirstObjectByType<DialogueManager>();
                if (DManager != null) DManager.CanInteract = true;
                ReturnToHub();
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

    private void MatchLaunch()
    {
        MatchCanvas.gameObject.SetActive(true);
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
        for (int id = 0; id <= 3; id++)
        {
            var entry = bugsDatabase.entries.Find(e => e != null && e.id == id);
            if (entry == null || !entry.isCompleted) return false;
        }
        return true;
    }

    //Menu
    public void QuitGame() 
    {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    public void Play() => SceneManager.LoadScene("Hub_Scene");

    // Options
    public void SwitchLanguage()
    {

    }

}

