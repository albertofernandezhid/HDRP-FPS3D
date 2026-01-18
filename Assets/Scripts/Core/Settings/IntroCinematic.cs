using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroCinematic : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string nextSceneName = "Tutorial";

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1.2f;

    [Header("Skip Button")]
    [SerializeField] private CanvasGroup skipButtonCanvas;
    [SerializeField] private float skipButtonDelay = 3f;

    private PlayableDirector director;
    private bool isLoading = false;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        director.stopped += OnTimelineFinished;

        if (fadeCanvas != null)
            fadeCanvas.alpha = 0f;

        if (skipButtonCanvas != null)
        {
            skipButtonCanvas.alpha = 0f;
            skipButtonCanvas.interactable = false;
            skipButtonCanvas.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        StartCoroutine(ShowSkipButtonDelayed());
    }

    private IEnumerator ShowSkipButtonDelayed()
    {
        yield return new WaitForSeconds(skipButtonDelay);

        if (skipButtonCanvas != null)
        {
            skipButtonCanvas.alpha = 1f;
            skipButtonCanvas.interactable = true;
            skipButtonCanvas.blocksRaycasts = true;
        }
    }

    private void OnTimelineFinished(PlayableDirector obj)
    {
        StartOutro();
    }

    public void SkipIntro()
    {
        StartOutro();
    }

    private void StartOutro()
    {
        if (isLoading) return;
        isLoading = true;

        director.stopped -= OnTimelineFinished;
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 1f;
        SceneManager.LoadScene(nextSceneName);
    }
}
