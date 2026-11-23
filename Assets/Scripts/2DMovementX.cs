using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

// Basic horizontal movement for a 2D character.
[RequireComponent(typeof(Rigidbody2D))]
public class SimpleSideMovement2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private InputActionReference _moveAction; // Vector2 action, only x is used
    [SerializeField, Min(0f)] private float _moveSpeed = 1.25f;

    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _SpriteRenderer;
    private Rigidbody2D _rb;
    private float _moveInput;

    public float MoveInput => _moveInput; // for animation and footsteps

    private float XPosLastFrame;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    private void OnEnable()
    {
        _moveAction.action.Enable();
    }

    private void OnDisable() // stops sliding when movement is switched off
    {
        _moveAction.action.Disable();
        _moveInput = 0f;
        if (_rb != null) _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
    }

    private void Update()
    {
        _moveInput = _moveAction.action.ReadValue<Vector2>().x;
        
        if (_moveInput != 0)
        {
            _animator.SetBool("IsRunning", true);
        }
        else
        {
            _animator.SetBool("IsRunning", false);
        }
        FlipCharacterX();
    }

    private void FlipCharacterX()
    {
        if (transform.position.x > XPosLastFrame)
        {   //moving right
            _SpriteRenderer.flipX = false; 
        }
        else if (transform.position.x < XPosLastFrame)
        {
            //moving left
            _SpriteRenderer.flipX = true;
        }

        XPosLastFrame = transform.position.x;

    }
    private void FixedUpdate() => _rb.linearVelocity = new Vector2(_moveInput * _moveSpeed, _rb.linearVelocity.y);
}
