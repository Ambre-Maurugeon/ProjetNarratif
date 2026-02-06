        using System.Collections;
        using UnityEngine;
        using UnityEngine.UI;
        
        [RequireComponent(typeof(Image))]
        public class ImageAnimation : MonoBehaviour
        {
            public Sprite[] sprites;
            public int framesPerSprite = 6;
            public bool loop = true;
            public bool destroyOnEnd = false;
        
            private int index = 0;
            private Image image;
            private Coroutine playCoroutine;
            public bool IsPlaying { get; private set; }
        
            private void Awake()
            {
                GetImage();
            }
        
            private void Start() { }
        
            public void GetImage()
            {
                if (image == null)
                    image = GetComponent<Image>();
            }
        
            public void ApplyData(SequenceDataSO data)
            {
                if (data == null) return;
                sprites = data.sprites;
                framesPerSprite = Mathf.Max(1, data.framesPerSprite);
                loop = data.loop;
                destroyOnEnd = data.destroyOnEnd;
            }
        
            public void ApplySequence(Sequence seq)
            {
                // Logs conservés : confirmation de l'application de la sequence
                Debug.Log("[ImageAnimation] ApplySequence -> applying struct Sequence");
                sprites = seq.sprites;
                framesPerSprite = Mathf.Max(1, seq.framesPerSprite);
                loop = seq.loop;
                destroyOnEnd = seq.destroyOnEnd;
                Debug.Log($"[ImageAnimation] ApplySequence -> sprites={(sprites != null ? sprites.Length.ToString() : "NULL")}, framesPerSprite={framesPerSprite}, loop={loop}, destroyOnEnd={destroyOnEnd}");
                if (sprites != null && sprites.Length > 0)
                {
                    for (int i = 0; i < Mathf.Min(5, sprites.Length); i++)
                        Debug.Log($"[ImageAnimation] ApplySequence -> sprite[{i}] name: {sprites[i]?.name ?? "NULL"}");
                }
            
                // Assurer qu'on a la référence Image et afficher le premier sprite immédiatement
                GetImage();
                if (image != null && sprites != null && sprites.Length > 0)
                    image.sprite = sprites[0];
            
                // Démarrer l'animation tout de suite pour le test
                Play(true);
            }
        
            public void Play(bool restart = true)
            {
                GetImage();
        
                if (IsPlaying && !restart) return;
        
                if (playCoroutine != null)
                    StopCoroutine(playCoroutine);
        
                playCoroutine = StartCoroutine(PlayCoroutine());
            }
        
            public void Stop(bool resetIndex = false)
            {
                if (playCoroutine != null)
                    StopCoroutine(playCoroutine);
                playCoroutine = null;
                IsPlaying = false;
                if (resetIndex)
                {
                    index = 0;
                    if (image != null && sprites != null && sprites.Length > 0)
                        image.sprite = sprites[index];
                }
            }
        
            private IEnumerator PlayCoroutine()
            {
                if (sprites == null || sprites.Length == 0)
                    yield break;
        
                IsPlaying = true;
                index = 0;
        
                while (loop || index < sprites.Length)
                {
                    if (image != null)
                        image.sprite = sprites[index];
        
                    int actualFrames = Mathf.Max(1, framesPerSprite);
                    for (int f = 0; f < actualFrames; f++)
                        yield return new WaitForFixedUpdate();
        
                    index++;
        
                    if (index >= sprites.Length)
                    {
                        if (loop)
                            index = 0;
                        else
                            break;
                    }
                }
        
                IsPlaying = false;
                playCoroutine = null;
        
                if (destroyOnEnd)
                    Destroy(gameObject);
            }
        }