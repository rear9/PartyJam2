using UnityEngine;

// follows the player at head height with smoothing and a bounded hover
[RequireComponent(typeof(AIController))]
public class AIMovement : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Vector2 _followOffset = new(0.8f, 1.2f); // x = distance behind player, y = height
    [SerializeField, Min(0f)] private float _followSharpness = 6f; // sharpness = how quickly the lerp happens

    [Header("Hover")]
    [SerializeField, Min(0f)] private float _hoverRadius = 0.15f;
    [SerializeField, Min(0f)] private float _hoverSharpness = 2f;
    [SerializeField] private Vector2 _hoverInterval = new(1f, 2.5f);
    
    private Camera _camera;
    private AIController _controller;
    private Vector2 _anchor;
    private Vector2 _hover;
    private Vector2 _hoverTarget;
    private float _hoverTimer;

    private void Awake() => _controller = GetComponent<AIController>();

    private void Start()
    {
        enabled = _controller.IsReady;
        if (!enabled) return;
        _camera = Camera.main;
        _anchor = GetAnchorTarget(); // snap to the anchor on spawn
    }

    private void LateUpdate() // runs after player movement and interpolation
    {
        _anchor = Vector2.Lerp(_anchor, GetAnchorTarget(), Smooth(_followSharpness));
        UpdateHover();
        Vector2 position = _anchor + _hover;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    private Vector2 GetAnchorTarget() // behind the player, opposite their facing
    {
        float side = _controller.PlayerFacingRight ? -1f : 1f;
        return (Vector2)_controller.Player.position + new Vector2(side * _followOffset.x, _followOffset.y);
    }

    private void UpdateHover() // eases toward a random point inside the radius
    {
        _hoverTimer -= Time.deltaTime;
        if (_hoverTimer <= 0f)
        {
            _hoverTarget = Random.insideUnitCircle * _hoverRadius;
            _hoverTimer = Random.Range(_hoverInterval.x, _hoverInterval.y);
        }
        _hover = Vector2.ClampMagnitude(Vector2.Lerp(_hover, _hoverTarget, Smooth(_hoverSharpness)), _hoverRadius);
    }

    private static float Smooth(float sharpness) => 1f - Mathf.Exp(-sharpness * Time.deltaTime); // frame-rate independent
}
