using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// persistent black panel that fades between scenes, keep on a root object
public class TransitionHandler : MonoBehaviour
{
    public static TransitionHandler Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CanvasGroup _panel; // full-screen black Image with a CanvasGroup, intended to persist across scenes

    [Header("Fade")]
    [SerializeField, Min(0.01f)] private float _fadeDuration = 1f; // defaults
    [SerializeField] private bool _fadeInOnStart = true;

    private bool _isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this) // singleton pattern
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetAlpha(1f); // starts fully black
    }

    private void Start()
    {
        if (Instance == this && _fadeInOnStart) StartCoroutine(FadeIn(_fadeDuration));
    }

    public void LoadScene(string sceneName) // fade out, load, fade in
    {
        if (_isTransitioning || !Application.CanStreamedLevelBeLoaded(sceneName)) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    public IEnumerator FadeOut(float duration = 1f) => Fade(1f, duration); // fade to black

    public IEnumerator FadeIn(float duration = 1f) => Fade(0f, duration); // fade from black

    private IEnumerator TransitionRoutine(string sceneName) // full scene change sequence
    {
        _isTransitioning = true;
        yield return FadeOut(_fadeDuration);
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return null; // lets the new scene's Start methods run
        yield return FadeIn(_fadeDuration);
        _isTransitioning = false;
    }

    private IEnumerator Fade(float target, float duration) // eased alpha lerp on unscaled time
    {
        float start = _panel.alpha;
        _panel.blocksRaycasts = true;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            _panel.alpha = Mathf.SmoothStep(start, target, t / duration);
            yield return null;
        }
        SetAlpha(target);
    }

    private void SetAlpha(float alpha) // blocks UI clicks only while visible
    {
        _panel.alpha = alpha;
        _panel.blocksRaycasts = alpha > 0f;
    }
}
