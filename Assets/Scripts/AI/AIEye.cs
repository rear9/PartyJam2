using UnityEngine;

// opens the eye when idle, closes it when moving, blinks, and aims the pupil
[RequireComponent(typeof(AIController))]
public class AIEye : MonoBehaviour
{
    private enum GazeMode { Player, Forward, Random } // idle gaze behaviours
    private enum EyePhase { Closed, HalfOpen, Open } // visible eye frame

    [Header("References")]
    [SerializeField] private SpriteRenderer _shell;
    [SerializeField] private Animator _animator; // paused unless fully open
    [SerializeField] private Sprite _closedSprite;
    [SerializeField] private Sprite _halfOpenSprite;
    [SerializeField] private Transform _pupil;

    [Header("Eye State")]
    [SerializeField, Min(0f)] private float _openDelay = 0.25f;
    [SerializeField, Min(0.01f)] private float _halfOpenDuration = 0.1f;

    [Header("Blinking")]
    [SerializeField] private Vector2 _blinkInterval = new(5f, 10f); // blinks between 5-10 secs on default
    [SerializeField, Min(0.01f)] private float _blinkClosedDuration = 0.08f;

    [Header("Gaze")]
    [SerializeField, Min(0.1f)] private float _gazeInterval = 5f; // 5 seconds per gaze change
    [SerializeField] private Vector2 _eyeRadius = new(0.3f, 0.1f); // how far the eye can move in all directions
    [SerializeField, Range(0f, 1f)] private float _travel = 0.7f;
    [SerializeField, Min(0f)] private float _pupilSharpness = 10f;

    private AIController _controller;
    private EyePhase _phase;
    private GazeMode _gazeMode;
    private Vector2 _randomDirection;
    private Vector2 _anchor; // pupil rest position captured at start
    private float _idleTimer;
    private float _phaseTimer;
    private float _gazeTimer;
    private bool _wantOpen;
    private bool _isBlinking;

    private void Awake() => _controller = GetComponent<AIController>();

    private void Start()
    {
        enabled = _controller.IsReady;
        if (!enabled) return;
        if (_pupil != null) _anchor = _pupil.localPosition;
        _idleTimer = _openDelay;
        _wantOpen = !_controller.PlayerIsMoving;
        SetPhase(_wantOpen ? EyePhase.HalfOpen : EyePhase.Closed);
    }

    private void Update()
    {
        UpdateEyeState();
        UpdatePhase();
        if (_phase != EyePhase.Closed && _pupil != null) UpdateGaze();
    }

    private void UpdateEyeState() // dialogue forces open, movement closes, idle opens
    {
        _idleTimer = _controller.PlayerIsMoving ? 0f : _idleTimer + Time.deltaTime;
        bool shouldOpen = _controller.IsDialogueActive || _idleTimer >= _openDelay;
        if (shouldOpen == _wantOpen) return;

        _wantOpen = shouldOpen;
        _isBlinking = false; // cancels any blink in progress
        SetPhase(shouldOpen ? EyePhase.HalfOpen : EyePhase.Closed);
    }

    private void UpdatePhase() // advances opening and blink frames, counts down to next blink
    {
        _phaseTimer -= Time.deltaTime;
        if (_phaseTimer > 0f) return;

        if (_phase == EyePhase.Open) StartBlink();
        else if (_phase == EyePhase.HalfOpen) SetPhase(EyePhase.Open);
        else if (_isBlinking) SetPhase(EyePhase.HalfOpen);
    }

    private void StartBlink() // closes briefly, then reopens through the half-open frame
    {
        _isBlinking = true;
        SetPhase(EyePhase.Closed);
    }

    private void SetPhase(EyePhase phase) // swaps frame, animator and pupil for the phase
    {
        if (phase == EyePhase.HalfOpen && _halfOpenSprite == null) phase = EyePhase.Open;
        EyePhase previous = _phase;
        _phase = phase;
        _phaseTimer = phase switch
        {
            EyePhase.Open => Random.Range(_blinkInterval.x, _blinkInterval.y),
            EyePhase.HalfOpen => _halfOpenDuration,
            _ => _blinkClosedDuration
        };

        bool open = phase == EyePhase.Open;
        if (_animator != null) _animator.enabled = open;
        if (_pupil != null) _pupil.gameObject.SetActive(phase != EyePhase.Closed);

        Sprite frame = phase == EyePhase.Closed ? _closedSprite : _halfOpenSprite;
        if (!open && _shell != null && frame != null) _shell.sprite = frame;
        if (previous == EyePhase.Closed && phase != EyePhase.Closed && !_isBlinking) PickGaze(); // blinks keep the gaze
        if (open) _isBlinking = false;
    }

    private void UpdateGaze() // timed target picks, smooth pupil aim
    {
        _gazeTimer -= Time.deltaTime;
        if (_gazeTimer <= 0f && !_controller.IsDialogueActive) PickGaze();
        AimPupil(GetGazeDirection());
    }

    private void PickGaze() // equal chance of player, forward or random
    {
        _gazeTimer = _gazeInterval;
        _gazeMode = (GazeMode)Random.Range(0, 3);
        _randomDirection = Random.insideUnitCircle.normalized;
    }

    private Vector2 GetGazeDirection() // world direction from eye to its target
    {
        if (_controller.IsDialogueActive) return DirectionToPlayer();
        return _gazeMode switch
        {
            GazeMode.Player => DirectionToPlayer(),
            GazeMode.Forward => _controller.PlayerFacingRight ? Vector2.right : Vector2.left,
            _ => _randomDirection
        };
    }

    private Vector2 DirectionToPlayer() => _controller.Player.position - _pupil.parent.TransformPoint(_anchor);

    private void AimPupil(Vector2 worldDirection) // places pupil
    {
        Vector2 direction = _pupil.parent.InverseTransformDirection(worldDirection).normalized;
        float reach = Mathf.Abs(direction.x) / _eyeRadius.x + Mathf.Abs(direction.y) / _eyeRadius.y;
        Vector2 offset = reach > 0f ? direction / reach * _travel : Vector2.zero;
        _pupil.localPosition = Vector2.Lerp(_pupil.localPosition, _anchor + offset, 1f - Mathf.Exp(-_pupilSharpness * Time.deltaTime));
    }
}
