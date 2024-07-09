using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static Vector2 SizeOfOverlapCollider = new Vector2(0.25f, 0.25f);
    private const float AngleOfOverlapCollider = 0;
    private const float DistanceFromHorizontalBounds = 0.5f;
    private const float DistanceFromVerticalBounds = 1f;

    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private Transform foot;
    [SerializeField] private Transform head;
    [SerializeField] private Transform hand;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask boundsMask;
    [SerializeField] private float raycastDistance;
    [SerializeField] private Transform leftBound;
    [SerializeField] private Transform rightBound;
    [SerializeField] private Transform topBound;
    [SerializeField] private Transform bottomBound;
    [SerializeField] private ThrowingCameraController throwingCamera;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float fallingGravityScale;
    [SerializeField] private float jumpingGravityScale;
    [SerializeField] private float coyoteTime = 0.2f;

    private Rigidbody2D playerRb;
    private Vector2 movementInput;
    private float jumpInput;
    private float coyoteTimeCounter;
    private bool isFacingRight = true;

    [SerializeField] private float bufferJumpDistance;
    private bool bufferedJump;

    void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        speed = 8;
    }

    //Callback for player movement from new input system
    public void OnMoveLeftOrRigth(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    //Callback for player jumping from new input system
    public void OnJump(InputAction.CallbackContext context)
    {
        jumpInput = context.ReadValue<float>();

        if (context.started)
        {
            if (CheckIfGrounded())
                Jump();
            else if (Physics2D.OverlapCircle(foot.position, bufferJumpDistance, groundMask))
                bufferedJump = true;
        }
    }

    private void Jump()
    {
        Debug.Log($"BufferedJump: {bufferedJump}");
        if (bufferedJump)
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, 0);
            playerRb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
        }
        else
            playerRb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);

        bufferedJump = false;
    }

    //Callback for player interaction with object from new input system
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
        if (Physics2D.OverlapBox(throwingCamera.transform.position, SizeOfOverlapCollider, AngleOfOverlapCollider) == GetComponent<Collider2D>())
            throwingCamera.PickUpCamera(transform);
    }

    private void Update()
    {
        BufferedJump();

        RealisticJumpGravity();

        // BufferedJump();

        // CoyoteJump();

        // SlideWall();
        // WallJump();

        // if (!isWallJumping)
        //     FlipPlayer();

        // KeepPlayerInBounds();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        transform.Translate(speed * Time.deltaTime * movementInput.x * Vector2.right);
    }

    private void RealisticJumpGravity()
    {
        if (playerRb.velocity.y < 0)
            playerRb.gravityScale = fallingGravityScale;
        else if (playerRb.velocity.y > 0 && jumpInput == 0)
            playerRb.gravityScale = lowJumpMultiplier;
        else
            playerRb.gravityScale = jumpingGravityScale;
    }

    private void CoyoteJump()
    {
        if (CheckIfGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;
    }

    public void BufferedJump()
    {
        if (bufferedJump && CheckIfGrounded())
            Jump();
    }

    private void FlipPlayer()
    {
        if ((isFacingRight && movementInput.x < 0) || (!isFacingRight && movementInput.x > 0))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x = -localScale.x;
            transform.localScale = localScale;
        }
    }

    // private void MovePlayer()
    // {
    //     if (isSliding)
    //         transform.Translate(movementInput.x * speed * Time.deltaTime * Vector2.right);
    //     else
    //         playerRb.velocity = new Vector2(movementInput.x * speed, Mathf.Clamp(playerRb.velocity.y, -10f, float.MaxValue));
    // }

    private bool IsWalled() => Physics2D.OverlapCircle(hand.position, 0.2f, wallMask);

    private bool CheckIfGrounded() => Physics2D.Raycast(foot.position, Vector2.down, raycastDistance, groundMask);

    private void KeepPlayerInBounds()
    {
        Collider2D colliderAfterTeleportation = Physics2D.OverlapCapsule(new Vector2(leftBound.position.x + DistanceFromHorizontalBounds, transform.position.y), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = new Vector2(leftBound.position.x + DistanceFromHorizontalBounds, transform.position.y);

        colliderAfterTeleportation = Physics2D.OverlapCapsule(new Vector2(rightBound.position.x - DistanceFromHorizontalBounds, transform.position.y), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = new Vector2(rightBound.position.x - DistanceFromHorizontalBounds, transform.position.y);

        colliderAfterTeleportation = Physics2D.OverlapCapsule(new Vector2(transform.position.x, bottomBound.position.y + DistanceFromVerticalBounds), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(head.position, Vector2.up, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = new Vector2(transform.position.x, bottomBound.position.y + DistanceFromVerticalBounds);

        colliderAfterTeleportation = Physics2D.OverlapCapsule(new Vector2(transform.position.x, topBound.position.y - DistanceFromVerticalBounds), SizeOfOverlapCollider, CapsuleDirection2D.Vertical, AngleOfOverlapCollider);

        if (Physics2D.Raycast(foot.position, Vector2.down, raycastDistance, boundsMask) && colliderAfterTeleportation == null)
            transform.position = new Vector2(transform.position.x, topBound.position.y - DistanceFromVerticalBounds);
    }


}
