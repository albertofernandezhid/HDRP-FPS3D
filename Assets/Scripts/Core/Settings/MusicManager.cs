using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Configuración Audio")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private float fadeDuration = 2.5f;
    [Range(0f, 1f)][SerializeField] private float menuMaxVolume = 0.6f;
    [Range(0f, 1f)][SerializeField] private float gameMaxVolume = 0.4f;

    [Header("Efecto de Pausa (Fade)")]
    [SerializeField] private bool usePauseFilter = true;
    [SerializeField] private float filterFadeSpeed = 5f;
    [SerializeField] private float pauseFrequency = 800f;
    [SerializeField] private float normalFrequency = 22000f;

    [Header("Listas de Reproducción")]
    [SerializeField] private List<AudioClip> menuPlaylist;
    [SerializeField] private List<AudioClip> gamePlaylist;

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioLowPassFilter filter;
    private bool isSourceAActive = true;
    private bool isFading = false;
    private List<AudioClip> currentPlaylist;
    private AudioClip lastPlayedClip;
    private float targetMaxVolume;
    private Coroutine filterCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(InitialFadeDelay());
    }

    IEnumerator InitialFadeDelay()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        DeterminarPlaylistYSonado();
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void SetupComponents()
    {
        sourceA = gameObject.AddComponent<AudioSource>();
        sourceB = gameObject.AddComponent<AudioSource>();
        filter = GetComponent<AudioLowPassFilter>();

        foreach (var s in new[] { sourceA, sourceB })
        {
            s.outputAudioMixerGroup = musicGroup;
            s.playOnAwake = false;
            s.loop = false;
            s.volume = 0;
            s.spatialBlend = 0f;
        }

        if (filter != null)
        {
            filter.enabled = true;
            filter.cutoffFrequency = normalFrequency;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DeterminarPlaylistYSonado();
    }

    private void DeterminarPlaylistYSonado()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        bool isMenu = sceneName == "MainMenu";

        List<AudioClip> targetPlaylist = isMenu ? menuPlaylist : gamePlaylist;
        targetMaxVolume = isMenu ? menuMaxVolume : gameMaxVolume;

        if (currentPlaylist != targetPlaylist)
        {
            currentPlaylist = targetPlaylist;
            PlayNextRandom();
        }
    }

    void Update()
    {
        if (usePauseFilter && filter != null)
        {
            float targetFreq = (Time.timeScale == 0) ? pauseFrequency : normalFrequency;
            if (!Mathf.Approximately(filter.cutoffFrequency, targetFreq))
            {
                filter.cutoffFrequency = Mathf.Lerp(filter.cutoffFrequency, targetFreq, filterFadeSpeed * Time.unscaledDeltaTime);
            }
        }

        if (isFading || currentPlaylist == null || currentPlaylist.Count == 0) return;

        AudioSource activeSource = isSourceAActive ? sourceA : sourceB;
        if (!activeSource.isPlaying)
        {
            PlayNextRandom();
        }
    }

    public void PlayNextRandom()
    {
        if (isFading || currentPlaylist == null || currentPlaylist.Count == 0) return;

        AudioClip nextClip;
        if (currentPlaylist.Count > 1)
        {
            do
            {
                nextClip = currentPlaylist[Random.Range(0, currentPlaylist.Count)];
            } while (nextClip == lastPlayedClip);
        }
        else
        {
            nextClip = currentPlaylist[0];
        }

        lastPlayedClip = nextClip;
        StartCoroutine(Crossfade(nextClip));
    }

    private IEnumerator Crossfade(AudioClip nextClip)
    {
        isFading = true;

        AudioSource activeSource = isSourceAActive ? sourceA : sourceB;
        AudioSource newSource = isSourceAActive ? sourceB : sourceA;

        newSource.clip = nextClip;
        newSource.volume = 0;
        newSource.Play();

        float t = 0;
        float startVol = activeSource.volume;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float percent = Mathf.SmoothStep(0, 1, t / fadeDuration);

            newSource.volume = percent * targetMaxVolume;
            activeSource.volume = (1 - percent) * startVol;
            yield return null;
        }

        newSource.volume = targetMaxVolume;
        activeSource.Stop();
        activeSource.volume = 0;

        isSourceAActive = !isSourceAActive;
        isFading = false;
    }
}