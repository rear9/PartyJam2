using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private IInteracble InteractableInRange = null;

    public GameObject interactIcon;
    void Start()
    {
        interactIcon.SetActive(false);

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            InteractableInRange?.Interact();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out IInteracble interactable) && interactable.Caninteract())
        {
            InteractableInRange = interactable;
            interactIcon.SetActive(true);

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteracble interactable) && interactable == InteractableInRange)
        {
            InteractableInRange = null;
            interactIcon.SetActive(false);

        }
    }
}
