using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowingCameraController : MonoBehaviour
{
    private const float formulaNumber = 0.5f;
    private const float TrajectoryTimeStep = 0.1f;
    private const int TrajectoryStepCount = 20;

    [SerializeField] private Transform boundsAndCameraHolder;
    [SerializeField] private Transform boundsHolder;
    [SerializeField] private float lerpSpeed = 0.005f;
    [SerializeField] private float throwingForce = 1.5f;
    [SerializeField] private Transform hand;

    private Vector2 velocity;
    private Vector2 currentMousePosition;
    private Rigidbody2D throwingCameraRb;
    private bool isCameraPickedUp;
    private float throwingInput;
    private float startingGravityScale;
    private Vector3 originalScale;

    private void Awake()
    {
        throwingCameraRb = GetComponent<Rigidbody2D>();
        startingGravityScale = throwingCameraRb.gravityScale;
        originalScale = transform.localScale;
    }

    //Callback for throwing action from new input system
    public void StartThrowing(InputAction.CallbackContext context)
    {
        throwingInput = context.ReadValue<float>();
    }

    //Callback for stopping cammera in air from new input system
    public void StopCamera(InputAction.CallbackContext context)
    {
        StopCameraInAir();
    }

    private void StopCameraInAir()
    {
        throwingCameraRb.gravityScale = 0;
        throwingCameraRb.velocity = Vector3.zero;
    }

    private void Update()
    {
        MoveBoundsToCamera();

        if (!isCameraPickedUp)
            return;

        transform.position = hand.position;
        StartThrowingCamera();

        float angle = Mathf.Atan2(throwingCameraRb.velocity.y, throwingCameraRb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void MoveBoundsToCamera()
    {
        boundsHolder.position = Vector3.Lerp(boundsHolder.position, transform.position, lerpSpeed);
    }

    public void StartThrowingCamera()
    {
        throwingCameraRb.gravityScale = startingGravityScale;

        if (throwingInput > 0)
        {
            currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            velocity = ((Vector2)hand.position - currentMousePosition) * throwingForce;
            DrawTrajectory(hand);
        }

        if (throwingInput == 0 && hand.GetComponent<LineRenderer>().positionCount != 0)
            ThrowCamera(hand);
    }

    private void DrawTrajectory(Transform hand)
    {
        Vector3[] positions = new Vector3[TrajectoryStepCount];
        hand.GetComponent<LineRenderer>().positionCount = TrajectoryStepCount;

        for (int i = 0; i < TrajectoryStepCount; i++)
        {
            float t = i * TrajectoryTimeStep;
            Vector3 position = (Vector2)hand.position + velocity * t + formulaNumber * t * t * Physics2D.gravity;

            positions[i] = position;
        }
        hand.GetComponent<LineRenderer>().SetPositions(positions);
    }

    private void ThrowCamera(Transform hand)
    {
        throwingCameraRb.velocity = velocity;
        transform.SetParent(boundsAndCameraHolder);
        transform.localScale = originalScale;
        ClearTrajectory(hand);
    }

    private void ClearTrajectory(Transform hand)
    {
        hand.GetComponent<LineRenderer>().positionCount = 0;
        isCameraPickedUp = false;
    }

    public void PickUpCamera(Transform player)
    {
        isCameraPickedUp = true;
        transform.position = hand.position;
        Vector3 playerOriginalScale = player.localScale;
        transform.SetParent(player);
        transform.localScale = Vector3.Scale(playerOriginalScale, new Vector3(1 / playerOriginalScale.x, 1 / playerOriginalScale.y, 1 / playerOriginalScale.z));
    }

    public void ReleaseCamera()
    {
        isCameraPickedUp = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        transform.SetParent(boundsAndCameraHolder);
        transform.localScale = originalScale;
        ClearTrajectory(hand);
    }

    public bool IsCameraPickedUp() => isCameraPickedUp;
}
