using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private ThrowingCameraController throwingCamera;
    [SerializeField] private Vector2 sizeOfOverlapCollider = new Vector2(0.25f, 0.25f);
    [SerializeField] private float angleOfOverlapCollider = 0;

    public void OnInteraction(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        if (throwingCamera.IsCameraPickedUp())
            throwingCamera.ReleaseCamera();
        else
            InteractWithObject();
    }

    private void InteractWithObject()
    {
        if (Physics2D.OverlapBox(throwingCamera.transform.position, sizeOfOverlapCollider, angleOfOverlapCollider) == GetComponent<Collider2D>())
            throwingCamera.PickUpCamera(transform);
    }
}
