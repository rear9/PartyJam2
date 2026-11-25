using UnityEngine;

// shared player references and state read by the other AI scripts
public class AIController : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Transform _player;
    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField] private Rigidbody2D _playerBody;

    [Header("Player Movement")]
    [SerializeField, Min(0f)] private float _moveThreshold = 0.05f; // minimum speed before counted as 'moving'

    [Header("Dialogue State")]
    [SerializeField] private bool _isDialogueActive; // set by dialogue system later

    public Transform Player => _player;
    public bool IsReady => _player != null && _playerSprite != null && _playerBody != null;
    public bool PlayerFacingRight => !_playerSprite.flipX; // flipX false = facing right
    public bool PlayerIsMoving => Mathf.Abs(_playerBody.linearVelocity.x) > _moveThreshold;
    public bool IsDialogueActive { get => _isDialogueActive; set => _isDialogueActive = value; }
}
