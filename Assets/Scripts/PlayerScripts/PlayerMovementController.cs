using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float speed;
    private Vector2 movementInput;
    private bool movementDisabled;

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (!movementDisabled)
            MovePlayer();
    }

    private void MovePlayer()
    {
        transform.Translate(speed * Time.deltaTime * movementInput.x * Vector2.right);
    }

    public void DisableMovement() => movementDisabled = true;
    public void EnableMovement() => movementDisabled = false;

    public Vector2 GetMovementInput() => movementInput;
}
