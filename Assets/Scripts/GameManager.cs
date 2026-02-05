using System;
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
    public int currentInsectId = 0;
    public SceneAsset SceneToLoad;
    public SceneAsset SceneHub;
    private Button DateBtn;
    private Button NextButton;
    private Button PrevButton;
    public Canvas UiCanva;
    [SerializeField] private GameObject sequencePlayerPrefab;


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

    private void Start()
    {
        if (NextButton == null)
        {
            if (GameObject.Find("NextBtn") != null)
            {
                NextButton = GameObject.Find("NextBtn").GetComponent<Button>();
            }
        }

        if (NextButton != null)
        {
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(() => NextInsect());
        }
        
        if (PrevButton == null)
        {
            if (GameObject.Find("PrevBtn") != null)
            {
                PrevButton = GameObject.Find("PrevBtn").GetComponent<Button>();
            }
        }
        if (PrevButton != null)
        {
            PrevButton.onClick.RemoveAllListeners();
            Debug.Log("Adding PrevInsect listener");
            PrevButton.onClick.AddListener(() => PrevInsect());
        }
        UpdateBug();
        LaunchSequence();
    }

    void OnEnable()
    {
        Pulse.OnEndRythm += StopRythmGame;
        DialogueManager.OnDialogueEnd += ReturnToHub;
    }
    
    void OnDisable()
    {
        Pulse.OnEndRythm -= StopRythmGame;
        DialogueManager.OnDialogueEnd -= ReturnToHub;
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
            return;
        }

        if (bugsDatabase.entries == null)
        {
            return;
        }

        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == id);
        if (entry == null)
        {
            return;
        }
        
        if (entry.isCompleted) 
        {
            return;
        }

        currentInsectId = id;
        StartCoroutine(StartInsectCoroutine());
    }

    private IEnumerator StartInsectCoroutine()
    {
        var transition = FindFirstObjectByType<TransitionScene>();

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
                transition = FindFirstObjectByType<TransitionScene>();
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

        if (UiCanva == null)
        {
            if (GameObject.Find("UICanva") != null)
            {
                UiCanva = GameObject.Find("UICanva").GetComponent<Canvas>();
            }
        }
        
        if (NextButton == null)
        {
            if (GameObject.Find("NextBtn") != null)
            {
                NextButton = GameObject.Find("NextBtn").GetComponent<Button>();
            }
        }
        if (NextButton != null)
        {
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(() => NextInsect());
        }
        if (PrevButton == null)
        {
            if (GameObject.Find("PrevBtn") != null)
            {
                PrevButton = GameObject.Find("PrevBtn").GetComponent<Button>();
            }
        }
        if (PrevButton != null)
        {
            PrevButton.onClick.RemoveAllListeners();
            PrevButton.onClick.AddListener(() => PrevInsect());
        }

        EnsureSequencePlayerExists();
        UpdateBug();

        if (currentInsectId < 0)
            return;

        if (bugsDatabase == null || bugsDatabase.entries == null)
        {
            return;
        }
        
        UpdateBug();

        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry == null)
        {
            return;
        }

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
            var dialogueManager = FindFirstObjectByType<DialogueManager>();
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

    public void ReturnToHub ()
    {
        var transition = FindFirstObjectByType<TransitionScene>();

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
            StartCoroutine(transition.FadeIn());
        }
        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        entry.isCompleted = true;
        SceneManager.LoadScene(SceneHub.name);
    }
    
    public void LaunchRythmGame()
    {
        var pulse = FindFirstObjectByType<Pulse>();
        pulse?.InitMusic();
        var DManager = FindFirstObjectByType<DialogueManager>();
        DManager.CanInteract= false;
    }
    
    public void StopRythmGame()
    {
        var DManager = FindFirstObjectByType<DialogueManager>();
        DManager.CanInteract= true;
    }
    
    public void NextInsect()
    {
        if (currentInsectId >= 4) return;
        currentInsectId++;
        UpdateBug();
    }
    
    public void PrevInsect()
    {
        if (currentInsectId <= 0) return;
        currentInsectId--;
        UpdateBug();
    }

    private void UpdateBug()
    {
        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry == null)
        {
            return;
        }

        if (DateBtn == null)
        {
            if (GameObject.Find("DateBtn") != null)
            {
                DateBtn = GameObject.Find("DateBtn").GetComponent<Button>();
            }
        }
        if (DateBtn != null)
        {
            DateBtn.onClick.RemoveAllListeners();
            DateBtn.onClick.AddListener(() => OnStartInsect(currentInsectId));
        }

        if (UiCanva != null)
        {
            var textTf = UiCanva.transform.Find("Name");
            if (textTf != null)
            {
                var text = textTf.GetComponent<TMPro.TextMeshProUGUI>();
                if (text != null)
                {
                    if (entry != null)
                    {
                        text.text = entry.perso;
                    }
                }
            }

            var spriteTf = UiCanva.transform.Find("Sprite");
            if (spriteTf != null)
            {
                var img = spriteTf.GetComponent<Image>();
                if (img != null)
                {
                    if (entry != null)
                    {
                        if (entry.characterSprite != null)
                            img.sprite = entry.characterSprite;
                    }
                }
            }

            for (int i = 0; i < 3; i++)
            {
                var descTf = UiCanva.transform.Find($"Desc {i + 1}");
                if (descTf == null) continue;

                var descText = descTf.GetComponent<TMPro.TextMeshProUGUI>();
                if (descText == null) continue;

                string value = "";
                if (entry != null && entry.description != null && entry.description.Length > i && !string.IsNullOrEmpty(entry.description[i]))
                {
                    value = entry.description[i];
                }

                descText.text = value;
            }
            
            var sprite = UiCanva.transform.Find("BgText");
            if (sprite != null)
            {
                var img = sprite.GetComponent<Image>();
                if (img != null)
                {
                    if (entry != null)
                    {
                        if (entry.BgSprite != null)
                            img.sprite = entry.BgSprite;
                    }
                }
            }
        }
    }

  private void EnsureSequencePlayerExists()
  {
      var player = GameObject.Find("SequencePlayer");
      if (player != null) return;
  
      if (sequencePlayerPrefab != null)
      {
          var go = Instantiate(sequencePlayerPrefab);
          go.name = "SequencePlayer";
          if (UiCanva != null)
              go.transform.SetParent(UiCanva.transform, false);
          return;
      }
  
      var seqGo = new GameObject("SequencePlayer", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(Animation));
      if (UiCanva != null)
      {
          seqGo.transform.SetParent(UiCanva.transform, false);
          var rt = seqGo.GetComponent<RectTransform>();
          rt.anchorMin = new Vector2(0.5f, 0.5f);
          rt.anchorMax = new Vector2(0.5f, 0.5f);
          rt.anchoredPosition = Vector2.zero;
          rt.sizeDelta = new Vector2(800, 600);
      }
      var img = seqGo.GetComponent<UnityEngine.UI.Image>();
      img.raycastTarget = false;
  }
  
  public void LaunchSequence()
  {
      if (bugsDatabase == null || bugsDatabase.entries == null) return;
  
      CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
      if (entry == null || entry.Sequences == null || entry.Sequences.Length == 0) return;
  
      int idx = Mathf.Clamp(entry.sequenceIndex, 0, entry.Sequences.Length - 1);
      AnimationClip clip = entry.Sequences[idx];
      if (clip == null) return;
  
      var DManager = FindFirstObjectByType<DialogueManager>();
      if (DManager != null) DManager.CanInteract = false;
  
      EnsureSequencePlayerExists();
      var player = GameObject.Find("SequencePlayer");
      bool playedViaAnimation = false;
  
      if (player != null)
      {
          var animation = player.GetComponentInChildren<Animation>();
          if (animation != null)
          {
              try
              {
      #if UNITY_EDITOR
                  if (!clip.legacy)
                  {
                      clip.legacy = true;
                      UnityEditor.EditorUtility.SetDirty(clip);
                      Debug.Log($"Marked clip '{clip.name}' as Legacy for Animation component.");
                  }
      #else
                  if (!clip.legacy)
                  {
                      Debug.LogWarning($"Clip '{clip.name}' n'est pas marqué Legacy. Marquez-le dans l'import settings pour l'utiliser avec le composant Animation.");
                  }
      #endif
  
                  var existing = animation.GetClip(clip.name);
                  if (existing != clip)
                  {
                      if (existing != null)
                          animation.RemoveClip(existing);
                      animation.AddClip(clip, clip.name);
                  }
  
                  animation.clip = clip;
                  var state = animation[clip.name];
                  if (state != null)
                      state.wrapMode = WrapMode.Once;
                  animation.Play(clip.name);
                  playedViaAnimation = true;
              }
              catch (Exception ex)
              {
                  Debug.LogWarning($"Impossible de lancer l'Animation: {ex.Message}");
                  playedViaAnimation = false;
              }
          }
      }
  
      entry.sequenceIndex = (idx + 1) % entry.Sequences.Length;
      StartCoroutine(HandleSequencePlayback(clip));
  }
    
    private IEnumerator HandleSequencePlayback(AnimationClip clip)
    {
        if (clip == null)
        {
            var Df = FindFirstObjectByType<DialogueManager>();
            if (Df != null) Df.CanInteract = true;
            yield break;
        }
    
        yield return new WaitForSeconds(clip.length);
    
        var DManager = FindFirstObjectByType<DialogueManager>();
        if (DManager != null) DManager.CanInteract = true;
    }
    
}