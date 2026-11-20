using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Handles a QTE's timing, input and result animation.
public class QTECore : MonoBehaviour
{
    private enum Phase { SpawnFade, Running, Result, Finished } // QTE lifecycle states

    [Header("References")]
    [SerializeField] private RectTransform _outerCircle;
    [SerializeField] private Image _keyImage;

    [Header("Appearance")]
    [SerializeField, Min(0.01f)] private float _spawnFadeDuration = 0.2f;
    [SerializeField, Min(0.01f)] private float _resultAnimationDuration = 1f;
    [SerializeField, Min(1f)] private float _resultScaleMult = 1.25f;
    [SerializeField] private Color _successColour = new(0.2f, 0.9f, 0.35f, 1f);
    [SerializeField] private Color _missColour = new(0.95f, 0.2f, 0.2f, 1f);

    private RectTransform _targetCircle;
    private CanvasGroup _canvasGroup;
    private Image[] _images;
    private Vector3 _outerStartScale;
    private Vector3 _baseScale;
    private Key _expectedKey;
    private float _duration;
    private float _timingWindow;
    private float _targetTime;
    private float _elapsed;
    private float _phaseElapsed;
    private bool _resultSuccess;
    private Phase _phase;
    private Action<bool> _onCompleted;

    public void Initialise(Sprite promptSprite, Key expectedKey, float d, float window, Action<bool> onCompleted) // instantiated from QTEManager
    {
        _onCompleted = onCompleted;
        _targetCircle = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>(); // using a CanvasGroup so I don't have to do transparency fading for all UI children
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        _images = GetComponentsInChildren<Image>(true);

        if (_outerCircle == null || _keyImage == null || promptSprite == null || _targetCircle == null ||
            _targetCircle.rect.width <= 0f || _outerCircle.rect.width <= 0f || Mathf.Approximately(_outerCircle.localScale.x, 0f))
        {
            Complete(false); // kill & report the current QTE if parameters aren't as expected
            return;
        }

        _expectedKey = expectedKey;
        _duration = Mathf.Max(0.1f, d); // Mathf.Max enforces minimums
        _timingWindow = Mathf.Max(0f, window); // ensure the timing window can't dip below 0
        _baseScale = transform.localScale;
        _outerStartScale = _outerCircle.localScale;
        _elapsed = 0f;
        _phaseElapsed = 0f;
        
        float outerWidth = _outerCircle.rect.width * Mathf.Abs(_outerStartScale.x); // calculate starting width from rectTransform * scale
        float targetRatio = Mathf.Clamp01(_targetCircle.rect.width / outerWidth); // size of target circle relative to outer
        _targetTime = _duration * (1f - targetRatio); // distance in seconds from outer circle to inner circle

        _keyImage.sprite = promptSprite;
        _keyImage.preserveAspect = true; // prevents prompt from getting stretched and squashed
        _canvasGroup.alpha = 0f;
        _phase = Phase.SpawnFade;
    }

    private void Update()
    {
        if (_phase == Phase.SpawnFade) UpdateSpawnFade(); // select an update function based on enum
        else if (_phase == Phase.Running) UpdateQTE();
        else if (_phase == Phase.Result) UpdateResult();
    }

    private void UpdateSpawnFade() // lerp transparency to fade QTE in
    {
        CheckInput(); // edge-case if input is pressed during fade-in
        if (_phase != Phase.SpawnFade) return;
        _phaseElapsed += Time.deltaTime;
        
        float t = Mathf.Clamp01(_phaseElapsed / _spawnFadeDuration);
        _canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t); // use easing
        if (t < 1f) return;

        _canvasGroup.alpha = 1f;
        _phaseElapsed = 0f;
        _phase = Phase.Running;
    }

    private void UpdateQTE() // linear downscale of outer circle until zero
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);
        _outerCircle.localScale = Vector3.Lerp(_outerStartScale, Vector3.zero, t);

        CheckInput();
        if (_phase == Phase.Running && _elapsed >= _duration) BeginResult(false); // if outer circle reaches zero, return false result
    }

    private void CheckInput() // check if key press matches expected input within timing window
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return; // edge-case

        bool correctPressed = false;
        bool incorrectPressed = false;
        foreach (var key in keyboard.allKeys)
        {
            if (!key.wasPressedThisFrame) continue;
            if (key.keyCode == _expectedKey) correctPressed = true;
            else incorrectPressed = true;
        }

        if (!correctPressed && !incorrectPressed) return;
        bool inWindow = Mathf.Abs(_elapsed - _targetTime) <= _timingWindow;
        BeginResult(correctPressed && !incorrectPressed && inWindow);
    }

    private void BeginResult(bool success)
    {
        if (_phase != Phase.SpawnFade && _phase != Phase.Running) return; // edge-case to prevent duplicates
        _resultSuccess = success;
        _phaseElapsed = 0f;
        TintImages(success ? _successColour : _missColour); // instant colouring based on result (green = hit / red = miss)
        _phase = Phase.Result;
    }

    private void UpdateResult() // lerp transparency & scale up to 'pop' QTE out
    {
        _phaseElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_phaseElapsed / _resultAnimationDuration);
        float eased = Mathf.SmoothStep(0f, 1f, t);
        transform.localScale = Vector3.Lerp(_baseScale, _baseScale * _resultScaleMult, eased);
        _canvasGroup.alpha = 1f - eased;
        if (t >= 1f) Complete(_resultSuccess);
    }

    private void TintImages(Color tint) // changes colour
    {
        foreach (Image image in _images)
        {
            if (image == null) continue;
            Color original = image.color;
            image.color = new Color(tint.r, tint.g, tint.b, original.a);
        }
    }

    private void Complete(bool success) // clears delegate, notifies manager & cleans up
    {
        if (_phase == Phase.Finished) return;
        _phase = Phase.Finished;
        Action<bool> callback = _onCompleted;
        _onCompleted = null;
        callback?.Invoke(success);
        Destroy(gameObject);
    }
}
