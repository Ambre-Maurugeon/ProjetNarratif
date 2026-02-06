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

    private Coroutine sequenceCoroutine;
    private ImageAnimation sceneImageAnim;

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
        UpdateBug();

        if (bugsDatabase != null && bugsDatabase.entries != null)
        {
            if (sceneImageAnim == null)
                sceneImageAnim = FindSceneImageAnimation();

            sceneImageAnim?.GetImage();

            PlayCurrentSequenceNow();
        }
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
        sceneImageAnim = FindSceneImageAnimation();
        if (sceneImageAnim != null)
            sceneImageAnim.GetImage();

        if (UiCanva == null)
        {
            if (GameObject.Find("UICanva") != null)
                UiCanva = GameObject.Find("UICanva").GetComponent<Canvas>();
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
        if (UiCanva != null)
        {
            var seqTf = UiCanva.transform.Find("SequencePlayer");
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
        pulse?.PlayMusic();
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
        if (bugsDatabase == null || bugsDatabase.entries == null) return;
        CharacterEntry entry = bugsDatabase.entries.Find(e => e != null && e.id == currentInsectId);
        if (entry == null) return;

        if (DateBtn == null)
        {
            if (GameObject.Find("DateBtn") != null)
                DateBtn = GameObject.Find("DateBtn").GetComponent<Button>();
        }
        if (DateBtn != null)
            DateBtn.onClick.RemoveAllListeners();
        DateBtn?.onClick.AddListener(() => OnStartInsect(currentInsectId));

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

    public void PlayCurrentSequenceNow()
    {
        var DManager = FindFirstObjectByType<DialogueManager>();
        if (DManager != null) DManager.CanInteract = false;
        var entry = bugsDatabase?.entries?.Find(e => e != null && e.id == currentInsectId);
        if (entry == null) return;

        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(InternalPlaySequence(entry, entry.sequenceIndex));
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
}